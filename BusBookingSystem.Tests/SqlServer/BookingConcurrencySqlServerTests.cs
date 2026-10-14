using BusBookingSystem.Application.Common;
using BusBookingSystem.Application.DTOs;
using BusBookingSystem.Core.Entities;
using BusBookingSystem.Core.Enums;
using BusBookingSystem.Infrastructure.Data;
using BusBookingSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BusBookingSystem.Tests.SqlServer;

/// <summary>
/// Behaviour the InMemory provider cannot show: real transactions, RowVersion concurrency and check constraints.
/// </summary>
[Trait("Category", "SqlServer")]
public class BookingConcurrencySqlServerTests(SqlServerDatabaseFixture database) : IClassFixture<SqlServerDatabaseFixture>
{
    [SkippableFact]
    public async Task CreateBookingAsync_ReturnsConflict_WhenAnotherBookingTakesTheLastSeatFirst()
    {
        Skip.IfNot(database.IsAvailable, database.UnavailableReason);

        // Arrange: one seat left on the trip
        var (tripId, firstCustomerId, secondCustomerId) = await SeedTripAsync(seatsBooked: 39);

        // The second customer's booking commits after the first customer's service has read the trip,
        // but before it saves, which is the race two simultaneous requests can produce.
        Result<BookingResponseDto>? secondResult = null;
        var interceptor = new RunOnceBeforeSaveInterceptor(async () =>
        {
            await using var otherContext = database.CreateContext();
            secondResult = await new BookingService(otherContext).CreateBookingAsync(
                secondCustomerId, new BookingRequestDto { TripId = tripId, NumberOfSeats = 1 });
        });

        await using var context = database.CreateContext(interceptor);
        var service = new BookingService(context);

        // Act
        var firstResult = await service.CreateBookingAsync(
            firstCustomerId, new BookingRequestDto { TripId = tripId, NumberOfSeats = 1 });

        // Assert
        Assert.NotNull(secondResult);
        Assert.Equal(ResultStatus.Created, secondResult!.Status);
        Assert.Equal(ResultStatus.Conflict, firstResult.Status);

        await using var verify = database.CreateContext();
        var trip = await verify.Trips.AsNoTracking().SingleAsync(t => t.Id == tripId);
        var bookings = await verify.Bookings.AsNoTracking().Where(b => b.TripId == tripId).ToListAsync();
        Assert.Equal(40, trip.SeatsBooked);
        var booking = Assert.Single(bookings);
        Assert.Equal(secondCustomerId, booking.UserId);
    }

    [SkippableFact]
    public async Task Database_RejectsBookingWithNegativeCargoWeight()
    {
        Skip.IfNot(database.IsAvailable, database.UnavailableReason);

        // Arrange
        var (tripId, customerId, _) = await SeedTripAsync(seatsBooked: 0);
        await using var context = database.CreateContext();
        context.Bookings.Add(new Booking
        {
            UserId = customerId,
            TripId = tripId,
            NumberOfSeats = 1,
            CargoWeight = -5,
            BookingReference = $"BK-NEG-{Guid.NewGuid():N}"[..20],
            BookingStatus = BookingStatus.Confirmed
        });

        // Act
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        // Assert
        Assert.Contains("CK_Booking_CargoWeight_NonNegative", exception.InnerException?.Message);
    }

    private async Task<(Guid TripId, Guid FirstCustomerId, Guid SecondCustomerId)> SeedTripAsync(int seatsBooked)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var origin = new Depot
        {
            DepotCode = $"ORG-{suffix}",
            Name = "Lilongwe Central",
            Address = new Address { City = "Lilongwe", Region = "Central", Country = "Malawi" }
        };
        var destination = new Depot
        {
            DepotCode = $"DST-{suffix}",
            Name = "Blantyre Central",
            Address = new Address { City = "Blantyre", Region = "South", Country = "Malawi" }
        };
        var bus = new Bus
        {
            FleetNumber = $"BUS-{suffix}",
            Model = "Coaster",
            NumberOfSeats = 40,
            CargoCapacity = 200,
            HasCargoSpace = true,
            BusType = BusType.Coach,
            RegistrationNumber = $"REG-{suffix}",
            ManufacturerName = "Toyota",
            ManufactureYear = 2023,
            BusStatus = BusStatus.Available,
            HomeDepot = origin
        };
        var trip = new Trip
        {
            Bus = bus,
            Route = new Route { Origin = origin, Destination = destination },
            SeatFare = 3500m,
            CargoFare = 800m,
            SeatsBooked = seatsBooked,
            DepartureTime = new DateTimeOffset(2026, 10, 12, 8, 0, 0, TimeSpan.Zero),
            ArrivalTime = new DateTimeOffset(2026, 10, 12, 13, 30, 0, TimeSpan.Zero)
        };
        var firstCustomer = CreateCustomer($"first-{suffix}@example.com");
        var secondCustomer = CreateCustomer($"second-{suffix}@example.com");

        await using var context = database.CreateContext();
        context.Trips.Add(trip);
        context.Users.AddRange(firstCustomer, secondCustomer);
        await context.SaveChangesAsync();

        return (trip.Id, firstCustomer.Id, secondCustomer.Id);
    }

    private static User CreateCustomer(string email) => new()
    {
        FirstName = "Test",
        LastName = "Customer",
        Email = email,
        PasswordHash = "hash",
        UserType = UserType.Customer
    };

    /// <summary>Runs an action once, just before the intercepted context sends its changes to the database.</summary>
    private sealed class RunOnceBeforeSaveInterceptor(Func<Task> action) : SaveChangesInterceptor
    {
        private bool _hasRun;

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (!_hasRun)
            {
                _hasRun = true;
                await action();
            }

            return result;
        }
    }
}
