using AutoMapper;
using BusBookingSystem.Application.Common;
using BusBookingSystem.Application.DTOs;
using BusBookingSystem.Core.Entities;
using BusBookingSystem.Core.Enums;
using BusBookingSystem.Infrastructure.Data;
using BusBookingSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace BusBookingSystem.Tests;

public class AuthServiceTests
{
    private const string Password = "Passw0rd!";

    #region RegisterAsync

    [Fact]
    public async Task RegisterAsync_CreatesCustomer_WithHashedPassword()
    {
        // Arrange
        await using var context = CreateContext();
        var service = CreateService(context);

        // Act
        var result = await service.RegisterAsync(CreateRegisterRequest("chikondi@example.com"));

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ResultStatus.Created, result.Status);
        Assert.NotNull(result.Data);
        Assert.Equal("chikondi@example.com", result.Data!.Email);
        Assert.Equal(UserType.Customer, result.Data.UserType);

        var user = await context.Users.AsNoTracking().SingleAsync();
        Assert.Equal(result.Data.Id, user.Id);
        Assert.Equal(UserType.Customer, user.UserType);
        Assert.NotEqual(Password, user.PasswordHash);
        Assert.NotEqual(
            PasswordVerificationResult.Failed,
            new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, Password));
    }

    [Fact]
    public async Task RegisterAsync_ReturnsConflict_WhenEmailAlreadyExists()
    {
        // Arrange
        await using var context = CreateContext();
        var service = CreateService(context);
        await service.RegisterAsync(CreateRegisterRequest("chikondi@example.com"));

        // Act
        var result = await service.RegisterAsync(CreateRegisterRequest("CHIKONDI@example.com"));

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Equal(1, await context.Users.CountAsync());
    }

    #endregion

    #region LoginAsync

    [Fact]
    public async Task LoginAsync_ReturnsToken_ForValidCredentials()
    {
        // Arrange
        await using var context = CreateContext();
        var service = CreateService(context);
        await service.RegisterAsync(CreateRegisterRequest("chikondi@example.com"));

        // Act
        var result = await service.LoginAsync(new UserLoginRequestDto { Email = "chikondi@example.com", Password = Password });

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.NotNull(result.Data);
        Assert.False(string.IsNullOrWhiteSpace(result.Data!.Token));
        Assert.Equal("chikondi@example.com", result.Data.UserDto?.Email);
    }

    [Fact]
    public async Task LoginAsync_ReturnsUnauthorized_ForUnknownEmail()
    {
        // Arrange
        await using var context = CreateContext();
        var service = CreateService(context);

        // Act
        var result = await service.LoginAsync(new UserLoginRequestDto { Email = "nobody@example.com", Password = Password });

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultStatus.UnAuthorized, result.Status);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task LoginAsync_ReturnsUnauthorized_ForWrongPassword()
    {
        // Arrange
        await using var context = CreateContext();
        var service = CreateService(context);
        await service.RegisterAsync(CreateRegisterRequest("chikondi@example.com"));

        // Act
        var result = await service.LoginAsync(new UserLoginRequestDto { Email = "chikondi@example.com", Password = "WrongPassw0rd!" });

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultStatus.UnAuthorized, result.Status);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task LoginAsync_DoesNotRevealWhetherEmailExists()
    {
        // Arrange
        await using var context = CreateContext();
        var service = CreateService(context);
        await service.RegisterAsync(CreateRegisterRequest("chikondi@example.com"));

        // Act
        var unknownEmail = await service.LoginAsync(new UserLoginRequestDto { Email = "nobody@example.com", Password = Password });
        var wrongPassword = await service.LoginAsync(new UserLoginRequestDto { Email = "chikondi@example.com", Password = "WrongPassw0rd!" });

        // Assert
        Assert.Equal(unknownEmail.Status, wrongPassword.Status);
        Assert.Equal(unknownEmail.Message, wrongPassword.Message);
    }

    #endregion

    #region Helpers

    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static AuthService CreateService(AppDbContext context)
    {
        // Same maps as the API registers in Program.cs.
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UserRegisterRequestDto, User>();
            cfg.CreateMap<User, UserDto>();
        }, NullLoggerFactory.Instance).CreateMapper();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Secret"] = "TestJwtSecretKeyThatIsLongEnoughForHmacSha256"
            })
            .Build();

        return new AuthService(context, mapper, configuration);
    }

    private static UserRegisterRequestDto CreateRegisterRequest(string email) => new()
    {
        FirstName = "Chikondi",
        LastName = "Banda",
        Email = email,
        Password = Password
    };

    #endregion
}
