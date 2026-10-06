using BusBookingSystem.Application.Common;
using BusBookingSystem.Core.Entities;
using BusBookingSystem.Core.Enums;
using BusBookingSystem.Infrastructure.Data;
using BusBookingSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BusBookingSystem.Tests;

public class BookingServiceTests
{
    // Seeded bus: 40 seats, 200 cargo capacity. Seeded trip fares: 3500 per seat, 800 per cargo unit.
    private const int BusSeats = 40;
    private const int BusCargoCapacity = 200;

    #region Request validation

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(0, 0)]
    public async Task CreateBookingAsync_ReturnsBadRequest_WhenSeatAndCargoQuantitiesAreInvalid(int numberOfSeats, int cargoWeight)
    {
        // Arrange
        await using var context = CreateContext();
        var (user, trip) = await SeedTripAsync(context);
        var service = new BookingService(context);
        var request = new BookingRequestDto { TripId = trip.Id, NumberOfSeats = numberOfSeats, CargoWeight = cargoWeight };

        // Act
        var result = await service.CreateBookingAsync(user.Id, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultStatus.BadRequest, result.Status);
        Assert.Empty(context.Bookings);
    }

    #endregion

    #region Missing user or trip

    [Fact]
    public async Task CreateBookingAsync_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        await using var context = CreateContext();
        var (_, trip) = await SeedTripAsync(context);
        var service = new BookingService(context);
        var request = new BookingRequestDto { TripId = trip.Id, NumberOfSeats = 1 };

        // Act
        var result = await service.CreateBookingAsync(Guid.NewGuid(), request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Empty(context.Bookings);
    }

    [Fact]
    public async Task CreateBookingAsync_ReturnsNotFound_WhenTripDoesNotExist()
    {
        // Arrange
        await using var context = CreateContext();
        var (user, _) = await SeedTripAsync(context);
        var service = new BookingService(context);
        var request = new BookingRequestDto { TripId = Guid.NewGuid(), NumberOfSeats = 1 };

        // Act
        var result = await service.CreateBookingAsync(user.Id, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Empty(context.Bookings);
    }

    #endregion

    #region Capacity rules

    [Fact]
    public async Task CreateBookingAsync_ReturnsConflict_WhenRequestedSeatsExceedAvailableSeats()
    {
        // Arrange
        await using var context = CreateContext();
        var (user, trip) = await SeedTripAsync(context, seatsBooked: BusSeats - 2);
        var service = new BookingService(context);
        var request = new BookingRequestDto { TripId = trip.Id, NumberOfSeats = 3 };

        // Act
        var result = await service.CreateBookingAsync(user.Id, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Empty(context.Bookings);

        var storedTrip = await GetStoredTripAsync(context, trip.Id);
        Assert.Equal(BusSeats - 2, storedTrip.SeatsBooked);
    }

    [Fact]
    public async Task CreateBookingAsync_AllowsBookingTheLastAvailableSeats()
    {
        // Arrange
        await using var context = CreateContext();
        var (user, trip) = await SeedTripAsync(context, seatsBooked: BusSeats - 2);
        var service = new BookingService(context);
        var request = new BookingRequestDto { TripId = trip.Id, NumberOfSeats = 2 };

        // Act
        var result = await service.CreateBookingAsync(user.Id, request);

        // Assert
        Assert.True(result.Success);

        var storedTrip = await GetStoredTripAsync(context, trip.Id);
        Assert.Equal(BusSeats, storedTrip.SeatsBooked);
    }

    [Fact]
    public async Task CreateBookingAsync_ReturnsConflict_WhenBusHasNoCargoSpace()
    {
        // Arrange
        await using var context = CreateContext();
        var (user, trip) = await SeedTripAsync(context, hasCargoSpace: false);
        var service = new BookingService(context);
        var request = new BookingRequestDto { TripId = trip.Id, CargoWeight = 10 };

        // Act
        var result = await service.CreateBookingAsync(user.Id, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Empty(context.Bookings);
    }

    [Fact]
    public async Task CreateBookingAsync_ReturnsConflict_WhenCargoExceedsAvailableCapacity()
    {
        // Arrange
        await using var context = CreateContext();
        var (user, trip) = await SeedTripAsync(context, cargoCapacityBooked: BusCargoCapacity - 10);
        var service = new BookingService(context);
        var request = new BookingRequestDto { TripId = trip.Id, CargoWeight = 11 };

        // Act
        var result = await service.CreateBookingAsync(user.Id, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Empty(context.Bookings);

        var storedTrip = await GetStoredTripAsync(context, trip.Id);
        Assert.Equal(BusCargoCapacity - 10, storedTrip.CargoCapacityBooked);
    }

    #endregion

    #region Successful bookings

    [Fact]
    public async Task CreateBookingAsync_CreatesConfirmedSeatBooking_AndReservesSeats()
    {
        // Arrange
        await using var context = CreateContext();
        var (user, trip) = await SeedTripAsync(context, seatsBooked: 5, cargoCapacityBooked: 40);
        var service = new BookingService(context);
        var request = new BookingRequestDto { TripId = trip.Id, NumberOfSeats = 2 };

        // Act
        var result = await service.CreateBookingAsync(user.Id, request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.NotNull(result.Data);

        var response = result.Data!;
        Assert.Equal(2, response.NumberOfSeats);
        Assert.Equal(0, response.CargoWeight);
        Assert.Equal(7000, response.SeatsFare);
        Assert.Equal(0, response.CargoFare);
        Assert.Equal(7000, response.TotalFare);

        var booking = await context.Bookings.AsNoTracking().SingleAsync();
        Assert.Equal(response.BookingId, booking.Id);
        Assert.Equal(user.Id, booking.UserId);
        Assert.Equal(trip.Id, booking.TripId);
        Assert.Equal(BookingStatus.Confirmed, booking.BookingStatus);
        Assert.Equal(2, booking.NumberOfSeats);
        Assert.Equal(0, booking.CargoWeight);

        var storedTrip = await GetStoredTripAsync(context, trip.Id);
        Assert.Equal(7, storedTrip.SeatsBooked);
        Assert.Equal(40, storedTrip.CargoCapacityBooked);
    }

    [Fact]
    public async Task CreateBookingAsync_CreatesCargoOnlyBooking_AndReservesCargoCapacity()
    {
        // Arrange
        await using var context = CreateContext();
        var (user, trip) = await SeedTripAsync(context, seatsBooked: 5, cargoCapacityBooked: 40);
        var service = new BookingService(context);
        var request = new BookingRequestDto { TripId = trip.Id, CargoWeight = 25 };

        // Act
        var result = await service.CreateBookingAsync(user.Id, request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(0, result.Data!.SeatsFare);
        Assert.Equal(20000, result.Data.CargoFare);
        Assert.Equal(20000, result.Data.TotalFare);

        var booking = await context.Bookings.AsNoTracking().SingleAsync();
        Assert.Equal(0, booking.NumberOfSeats);
        Assert.Equal(25, booking.CargoWeight);

        var storedTrip = await GetStoredTripAsync(context, trip.Id);
        Assert.Equal(5, storedTrip.SeatsBooked);
        Assert.Equal(65, storedTrip.CargoCapacityBooked);
    }

    #endregion

    #region Helpers

    private static AppDbContext CreateContext()
    {
        // BookingService opens a transaction, which the InMemory provider does not support.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new AppDbContext(options);

        // SQL Server generates RowVersion; the InMemory provider does not, so supply one for new bookings.
        context.SavingChanges += (_, _) =>
        {
            foreach (var entry in context.ChangeTracker.Entries<Booking>().Where(e => e.State == EntityState.Added))
            {
                entry.Entity.RowVersion ??= new byte[] { 1 };
            }
        };

        return context;
    }

    private static async Task<(User User, Trip Trip)> SeedTripAsync(
        AppDbContext context,
        int seatsBooked = 0,
        int cargoCapacityBooked = 0,
        bool hasCargoSpace = true)
    {
        var originDepot = new Depot
        {
            DepotCode = "LILONGWE-001",
            Name = "Lilongwe Central",
            IsActive = true,
            Address = new Address
            {
                City = "Lilongwe",
                Region = "Central",
                Country = "Malawi"
            }
        };

        var destinationDepot = new Depot
        {
            DepotCode = "BLANTYRE-001",
            Name = "Blantyre Central",
            IsActive = true,
            Address = new Address
            {
                City = "Blantyre",
                Region = "South",
                Country = "Malawi"
            }
        };

        var bus = new Bus
        {
            FleetNumber = "BUS-101",
            Model = "Coaster",
            NumberOfSeats = BusSeats,
            CargoCapacity = hasCargoSpace ? BusCargoCapacity : 0,
            HasCargoSpace = hasCargoSpace,
            BusType = BusType.Coach,
            RegistrationNumber = "ABC-123",
            ManufacturerName = "Toyota",
            ManufactureYear = 2023,
            BusStatus = BusStatus.Available,
            HomeDepot = originDepot,
            RowVersion = new byte[] { 1 }
        };

        var trip = new Trip
        {
            Bus = bus,
            Route = new Route
            {
                Origin = originDepot,
                Destination = destinationDepot
            },
            SeatFare = 3500m,
            CargoFare = hasCargoSpace ? 800m : null,
            SeatsBooked = seatsBooked,
            CargoCapacityBooked = cargoCapacityBooked,
            DepartureTime = new DateTimeOffset(2026, 10, 12, 8, 0, 0, TimeSpan.Zero),
            ArrivalTime = new DateTimeOffset(2026, 10, 12, 13, 30, 0, TimeSpan.Zero),
            RowVersion = new byte[] { 1 }
        };

        var user = new User
        {
            FirstName = "Chikondi",
            LastName = "Banda",
            Email = "chikondi@example.com",
            PasswordHash = "hash",
            UserType = UserType.Customer
        };

        context.Depots.AddRange(originDepot, destinationDepot);
        context.Buses.Add(bus);
        context.Trips.Add(trip);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return (user, trip);
    }

    private static Task<Trip> GetStoredTripAsync(AppDbContext context, Guid tripId) =>
        context.Trips.AsNoTracking().SingleAsync(t => t.Id == tripId);

    #endregion
}
