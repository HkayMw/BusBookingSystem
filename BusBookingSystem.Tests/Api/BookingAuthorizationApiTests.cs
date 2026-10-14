using BusBookingSystem.API.Models;
using BusBookingSystem.Application.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

namespace BusBookingSystem.Tests.Api;

public class BookingAuthorizationApiTests(BookingApiFactory factory) : IClassFixture<BookingApiFactory>
{
    private const string BookingUrl = "/api/booking";

    [Fact]
    public async Task CreateBooking_ReturnsUnauthorized_WithoutToken()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(BookingUrl, CreateBookingRequest());

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateBooking_ReturnsUnauthorized_WithMalformedToken()
    {
        // Arrange
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-jwt");

        // Act
        var response = await client.PostAsJsonAsync(BookingUrl, CreateBookingRequest());

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateBooking_ReturnsUnauthorized_WithTokenSignedByAnotherKey()
    {
        // Arrange: a well-formed token signed with a key the API does not trust (e.g. a rotated-out secret)
        var client = factory.CreateClient();
        var foreignToken = CreateToken("AnotherSecretThatTheApiDoesNotTrust-0123456789");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", foreignToken);

        // Act
        var response = await client.PostAsJsonAsync(BookingUrl, CreateBookingRequest());

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateBooking_ReachesBookingService_WithValidToken()
    {
        // Arrange: register and log in through the API to get a real token
        var client = factory.CreateClient();
        var email = $"api-{Guid.NewGuid():N}@example.com";
        const string password = "Passw0rd!";

        var register = await client.PostAsJsonAsync("/api/auth/register", new UserRegisterRequestDto
        {
            FirstName = "Chikondi",
            LastName = "Banda",
            Email = email,
            Password = password
        });
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        var login = await client.PostAsJsonAsync("/api/auth/login", new UserLoginRequestDto { Email = email, Password = password });
        var loginBody = await login.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        var token = loginBody?.Data?.Token;
        Assert.False(string.IsNullOrWhiteSpace(token));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act: the trip does not exist, so a 404 from the service proves authentication passed
        var response = await client.PostAsJsonAsync(BookingUrl, CreateBookingRequest());

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.Equal("Trip was not found.", body?.Message);
    }

    private static BookingRequestDto CreateBookingRequest() => new()
    {
        TripId = Guid.NewGuid(),
        NumberOfSeats = 1
    };

    private static string CreateToken(string secret)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }),
            Expires = DateTime.UtcNow.AddMinutes(10),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }
}
