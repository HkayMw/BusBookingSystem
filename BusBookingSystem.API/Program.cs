using BusBookingSystem.API;
using BusBookingSystem.API.Models;
using BusBookingSystem.Application.Interfaces.Services;
using BusBookingSystem.Infrastructure.Data;
using BusBookingSystem.Infrastructure.Mapping;
using BusBookingSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


// The secret is not stored in appsettings.json. Development: dotnet user-secrets. Other environments: JwtSettings__Secret env var.
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException(
        "JwtSettings:Secret is missing. Set it with 'dotnet user-secrets set \"JwtSettings:Secret\" \"<value>\" --project BusBookingSystem.API' " +
        "or the JwtSettings__Secret environment variable.");
}

byte[] key = Encoding.UTF8.GetBytes(jwtSecret);
if (key.Length < 32)
{
    throw new InvalidOperationException("JwtSettings:Secret must be at least 32 bytes for HMAC-SHA256.");
}

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // A valid signature isn't enough: reject tokens whose user was deleted or deactivated after login.
    options.Events.OnTokenValidated = async context =>
    {
        var userIdClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            context.Fail("The token has no valid user ID.");
            return;
        }
        var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var isActiveUser = await dbContext.Users.AnyAsync(u => u.Id == userId && u.IsActive);
        if (!isActiveUser)
        {
            context.Fail("User does not exist or is inactive.");
        }
    };
});
builder.Services.AddAuthorization();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services

builder.Services.AddScoped<IDepotService, DepotService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddAutoMapper(o => o.AddProfile<MappingProfile>());

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, CancellationToken) =>
    {
        document.Components ??= new();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter JWT Bearer token"
            }
        };

        document.Security = [
            new OpenApiSecurityRequirement
                {
                    {new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
                }
            ];

        return Task.CompletedTask;
    });
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

        return new BadRequestObjectResult(
            new ApiResponse<object>
            {
                Success = false,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "The request is invalid.",
                Errors = errors
            });
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Seed development data for manual testing
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DevelopmentDataSeeder.SeedAsync(context);

    app.MapOpenApi();
    app.MapScalarApiReference();

}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
