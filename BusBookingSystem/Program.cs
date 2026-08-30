//using BusBookingSystem.Core.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen();

var app = builder.Build();

//// Test Bus model
//var testBus = new Bus
//{
//    BusNumber = "BUS-001",
//    BusModel = "Volvo Coach",
//    Capacity = 50,
//    RegistrationNumber = "MZU-123",
//    ManufactureDate = new DateTime(2020, 1, 1),
//    IsActive = true
//};
//Console.WriteLine($"Test Hkay Bus: {testBus.Id} with {testBus.Capacity} seats");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
