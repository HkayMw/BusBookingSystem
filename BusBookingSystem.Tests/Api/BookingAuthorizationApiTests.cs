using BusBookingSystem.API.Models;
using BusBookingSystem.Application.DTOs;
using BusBookingSystem.Core.Entities;
using BusBookingSystem.Core.Enums;
using BusBookingSystem.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BusBookingSystem.Tests.Api;

public class BookingAuthorizationApiTests(BookingApiFactory factory) : IClassFixture<BookingApiFactory>
{
    private const string BookingUrl = "/api/bookings";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

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
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.PostAsJsonAsync(BookingUrl, CreateBookingRequest());

        // Assert: the trip does not exist, so a 404 from the service proves authentication passed
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);
        Assert.Equal("Trip was not found.", body?.Message);
    }

    #region Get Booking By Id for owner tests
    [Fact]
    public async Task GetBookingById_ReturnsUnauthorized_WithoutToken()
    {
        // Arrange
        var client = factory.CreateClient();
        var bookingId = Guid.NewGuid();
        // Act
        var response = await client.GetAsync($"{BookingUrl}/{bookingId}");
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetBookingById_ReturnsUnauthorized_WithMalformedToken()
    {
        // Arrange
        var client = factory.CreateClient();
        var bookingId = Guid.NewGuid();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-jwt");
        // Act
        var response = await client.GetAsync($"{BookingUrl}/{bookingId}");
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetBookingById_ReturnsUnauthorized_WithTokenSignedByAnotherKey()
    {
        // Arrange: a well-formed token signed with a key the API does not trust (e.g. a rotated-out secret)
        var client = factory.CreateClient();
        var bookingId = Guid.NewGuid();
        var foreignToken = CreateToken("AnotherSecretThatTheApiDoesNotTrust-0123456789");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", foreignToken);
        // Act
        var response = await client.GetAsync($"{BookingUrl}/{bookingId}");
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetBookingById_ReturnsNotFound_WithValidTokenForUnknownBooking()
    {
        // Arrange: a real customer token, but a booking ID that does not exist
        var client = await CreateAuthenticatedClientAsync();

        // Act: proves the route, authentication, the user ID claim and the NotFound -> 404 mapping
        var response = await client.GetAsync($"{BookingUrl}/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);
        Assert.False(body?.Success);
        Assert.Equal("Booking not found.", body?.Message);
    }

    [Fact]
    public async Task GetBookingById_ReturnsBookingForOwner_AndNotFoundForAnotherCustomer()
    {
        // Arrange: seed the data directly; there is no endpoint for creating trips
        var (ownerId, otherCustomerId, bookingId) = await SeedBookingAsync();

        var ownerClient = CreateClientFor(ownerId);
        var otherCustomerClient = CreateClientFor(otherCustomerId);

        // Act
        var ownerResponse = await ownerClient.GetAsync($"{BookingUrl}/{bookingId}");
        var otherCustomerResponse = await otherCustomerClient.GetAsync($"{BookingUrl}/{bookingId}");

        // Assert: the owner sees the booking with its stored fares
        Assert.Equal(HttpStatusCode.OK, ownerResponse.StatusCode);
        var ownerBody = await ownerResponse.Content.ReadFromJsonAsync<ApiResponse<BookingResponseDto>>(JsonOptions);
        Assert.NotNull(ownerBody?.Data);
        Assert.Equal(bookingId, ownerBody!.Data!.BookingId);
        Assert.Equal("Chikondi Banda", ownerBody.Data.CustomerName);
        Assert.Contains("\"bookingStatus\":\"Pending\"", await ownerResponse.Content.ReadAsStringAsync());
        Assert.Equal(7000m, ownerBody.Data.SeatsFare);
        Assert.Equal(2400m, ownerBody.Data.CargoFare);
        Assert.Equal(9400m, ownerBody.Data.TotalFare);

        // Assert: another customer gets 404, which does not reveal that the booking exists
        Assert.Equal(HttpStatusCode.NotFound, otherCustomerResponse.StatusCode);
    }

    [Fact]
    public async Task GetBookingById_ReturnsUnauthorized_WhenTokenUserDoesNotExist()
    {
        // Arrange: a correctly signed token for a user ID that was never saved
        var client = CreateClientFor(Guid.NewGuid());

        // Act: without the check in OnTokenValidated, this request would reach the service and return 404
        var response = await client.GetAsync($"{BookingUrl}/{Guid.NewGuid()}");

        // Assert: rejected by authentication (empty body), not by the service (ApiResponse body)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetBookingById_ReturnsUnauthorized_WhenTokenUserIsInactive()
    {
        // Arrange: the owner holds a valid token, then the account is deactivated
        var (ownerId, _, bookingId) = await SeedBookingAsync();
        var ownerClient = CreateClientFor(ownerId);
        await DeactivateUserAsync(ownerId);

        // Act: without the check in OnTokenValidated, the owner would still get 200 for their own booking
        var response = await ownerClient.GetAsync($"{BookingUrl}/{bookingId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync());
    }

    #endregion

    #region Helpers

    private static BookingRequestDto CreateBookingRequest() => new()
    {
        TripId = Guid.NewGuid(),
        NumberOfSeats = 1
    };

    /// <summary>Registers and logs in a new customer through the API and returns a client carrying their token.</summary>
    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
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
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var loginBody = await login.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>(JsonOptions);
        var token = loginBody?.Data?.Token;
        Assert.False(string.IsNullOrWhiteSpace(token));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>Returns a client with a token the API trusts, for a user seeded directly in the database.</summary>
    private HttpClient CreateClientFor(Guid userId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateToken(BookingApiFactory.JwtSecret, userId));
        return client;
    }

    /// <summary>
    /// Seeds two customers, a trip and one booking for the first customer straight into the
    /// factory's InMemory database. The booking is seeded rather than created through
    /// POST /api/bookings because InMemory does not generate RowVersion values.
    /// </summary>
    private async Task<(Guid OwnerId, Guid OtherCustomerId, Guid BookingId)> SeedBookingAsync()
    {
        // Unique suffix: every test in this class shares one InMemory database.
        var suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();

        var origin = new Depot
        {
            DepotCode = $"LLW-{suffix}",
            Name = "Lilongwe Central",
            Address = new Address { City = "Lilongwe", Region = "Central", Country = "Malawi" }
        };

        var destination = new Depot
        {
            DepotCode = $"BTC-{suffix}",
            Name = "Blantyre City",
            Address = new Address { City = "Blantyre", Region = "Southern", Country = "Malawi" }
        };

        var bus = new Bus
        {
            FleetNumber = $"F-{suffix}",
            Model = "ZK6907H",
            NumberOfSeats = 40,
            CargoCapacity = 200,
            HasCargoSpace = true,
            BusType = BusType.Coach,
            RegistrationNumber = $"LL {suffix}",
            ManufacturerName = "YUTONG",
            ManufactureYear = 2020,
            BusStatus = BusStatus.Available,
            HomeDepot = origin,
            RowVersion = new byte[] { 1 }
        };

        var trip = new Trip
        {
            Bus = bus,
            Route = new Route { Origin = origin, Destination = destination },
            SeatFare = 3500m,
            CargoFare = 800m,
            SeatsBooked = 2,
            CargoCapacityBooked = 3,
            DepartureTime = DateTimeOffset.UtcNow.AddDays(1),
            ArrivalTime = DateTimeOffset.UtcNow.AddDays(1).AddHours(5),
            RowVersion = new byte[] { 1 }
        };

        var owner = new User
        {
            FirstName = "Chikondi",
            LastName = "Banda",
            Email = $"owner-{suffix}@example.com",
            PasswordHash = "hash",
            UserType = UserType.Customer
        };

        var otherCustomer = new User
        {
            FirstName = "Chrispin",
            LastName = "Phiri",
            Email = $"other-{suffix}@example.com",
            PasswordHash = "hash",
            UserType = UserType.Customer
        };

        var booking = new Booking
        {
            User = owner,
            Trip = trip,
            NumberOfSeats = 2,
            CargoWeight = 3,
            SeatsFare = 7000m,
            CargoFare = 2400m,
            TotalFare = 9400m,
            BookingReference = $"BK-TEST-{suffix}",
            BookingStatus = BookingStatus.Pending,
            RowVersion = new byte[] { 1 }
        };

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.AddRange(origin, destination, bus, trip, owner, otherCustomer, booking);
        await db.SaveChangesAsync();

        return (owner.Id, otherCustomer.Id, booking.Id);
    }

    /// <summary>Marks a seeded user inactive, as an administrator would after the user has logged in.</summary>
    private async Task DeactivateUserAsync(Guid userId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.FindAsync(userId);
        Assert.NotNull(user);
        user.IsActive = false;
        await db.SaveChangesAsync();
    }

    private static string CreateToken(string secret, Guid? userId = null)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, (userId ?? Guid.NewGuid()).ToString()) }),
            Expires = DateTime.UtcNow.AddMinutes(10),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }

    #endregion
}
