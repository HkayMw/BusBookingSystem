using BusBookingSystem.Application.Common;
using BusBookingSystem.Core.Entities;
using BusBookingSystem.Infrastructure.Data;
using BusBookingSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.Tests;

public class TripServiceTests
{
    [Fact]
    public async Task SearchTripAsync_ReturnsTripsMatchingOriginDestinationAndDate()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);

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

        var wrongDestinationDepot = new Depot
        {
            DepotCode = "MZUZU-001",
            Name = "Mzuzu Depot",
            IsActive = true,
            Address = new Address
            {
                City = "Mzuzu",
                Region = "North",
                Country = "Malawi"
            }
        };

        var bus = new Bus
        {
            FleetNumber = "BUS-101",
            Model = "Coaster",
            NumberOfSeats = 40,
            CargoCapacity = 200,
            HasCargoSpace = true,
            BusType = BusBookingSystem.Core.Enums.BusType.Coach,
            RegistrationNumber = "ABC-123",
            ManufacturerName = "Toyota",
            ManufactureYear = 2023,
            BusStatus = BusBookingSystem.Core.Enums.BusStatus.Available,
            HomeDepot = originDepot,
            RowVersion = new byte[] { 1 }
        };

        var matchingRoute = new Route
        {
            Origin = originDepot,
            Destination = destinationDepot
        };

        var differentRoute = new Route
        {
            Origin = originDepot,
            Destination = wrongDestinationDepot
        };

        var departureDate = new DateOnly(2026, 10, 12);
        var matchingTrip = new Trip
        {
            Bus = bus,
            Route = matchingRoute,
            SeatFare = 3500m,
            CargoFare = 800m,
            SeatsBooked = 6,
            CargoCapacityBooked = 40,
            DepartureTime = new DateTimeOffset(2026, 10, 12, 8, 0, 0, TimeSpan.Zero),
            ArrivalTime = new DateTimeOffset(2026, 10, 12, 13, 30, 0, TimeSpan.Zero),
            RowVersion = new byte[] { 1 }
        };

        var nonMatchingTrip = new Trip
        {
            Bus = bus,
            Route = differentRoute,
            SeatFare = 3200m,
            CargoFare = 700m,
            SeatsBooked = 2,
            CargoCapacityBooked = 30,
            DepartureTime = new DateTimeOffset(2026, 10, 13, 9, 0, 0, TimeSpan.Zero),
            ArrivalTime = new DateTimeOffset(2026, 10, 13, 12, 0, 0, TimeSpan.Zero),
            RowVersion = new byte[] { 2 }
        };

        context.Depots.AddRange(originDepot, destinationDepot, wrongDestinationDepot);
        context.Buses.Add(bus);
        context.Routes.AddRange(matchingRoute, differentRoute);
        context.Trips.AddRange(matchingTrip, nonMatchingTrip);
        await context.SaveChangesAsync();

        var service = new TripService(context);

        // Act
        var result = await service.SearchTripAsync(originDepot.Id, destinationDepot.Id, departureDate);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.NotNull(result.Data);

        var trips = result.Data!.ToList();
        Assert.Single(trips);
        Assert.Equal(matchingTrip.Id, trips[0].TripId);
        Assert.Equal("Lilongwe Central", trips[0].OriginDepotName);
        Assert.Equal("Blantyre Central", trips[0].DestinationDepotName);
        Assert.Equal(34, trips[0].AvailableSeats);
        Assert.Equal(160, trips[0].AvailableCargoCapacity);
    }

    [Fact]
    public async Task SearchTripAsync_ReturnsBadRequest_WhenOriginAndDestinationAreSame()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new TripService(context);
        var sameDepotId = Guid.NewGuid();
        var departureDate = new DateOnly(2026, 10, 12);

        // Act
        var result = await service.SearchTripAsync(sameDepotId, sameDepotId, departureDate);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultStatus.BadRequest, result.Status);
        Assert.Contains("cannot be the same", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
