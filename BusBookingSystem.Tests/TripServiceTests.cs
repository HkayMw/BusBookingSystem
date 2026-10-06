using BusBookingSystem.Application.Common;
using BusBookingSystem.Core.Entities;
using BusBookingSystem.Core.Enums;
using BusBookingSystem.Infrastructure.Data;
using BusBookingSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.Tests;

public class TripServiceTests
{
    private static readonly DateOnly DepartureDate = new(2026, 10, 12);
    private static readonly DateTimeOffset DepartureTime = new(2026, 10, 12, 8, 0, 0, TimeSpan.Zero);

    #region SearchTripAsync

    [Fact]
    public async Task SearchTripAsync_ReturnsMatchingTrip_WithAvailability()
    {
        // Arrange
        await using var context = CreateContext();
        var lilongwe = CreateDepot("LILONGWE-001", "Lilongwe Central", "Lilongwe");
        var blantyre = CreateDepot("BLANTYRE-001", "Blantyre Central", "Blantyre");
        var trip = CreateTrip(CreateBus(lilongwe), lilongwe, blantyre, DepartureTime, seatsBooked: 6, cargoCapacityBooked: 40);
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        var service = new TripService(context);

        // Act
        var result = await service.SearchTripAsync(lilongwe.Id, blantyre.Id, DepartureDate);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.NotNull(result.Data);

        var found = Assert.Single(result.Data!);
        Assert.Equal(trip.Id, found.TripId);
        Assert.Equal("Lilongwe Central", found.OriginDepotName);
        Assert.Equal("Blantyre Central", found.DestinationDepotName);
        Assert.Equal(34, found.AvailableSeats);
        Assert.Equal(160, found.AvailableCargoCapacity);
    }

    [Fact]
    public async Task SearchTripAsync_ExcludesTripsOnOtherRoutes()
    {
        // Arrange
        await using var context = CreateContext();
        var lilongwe = CreateDepot("LILONGWE-001", "Lilongwe Central", "Lilongwe");
        var blantyre = CreateDepot("BLANTYRE-001", "Blantyre Central", "Blantyre");
        var mzuzu = CreateDepot("MZUZU-001", "Mzuzu Depot", "Mzuzu");
        var bus = CreateBus(lilongwe);

        var matchingTrip = CreateTrip(bus, lilongwe, blantyre, DepartureTime);
        var otherRouteTrip = CreateTrip(bus, lilongwe, mzuzu, DepartureTime);
        context.Trips.AddRange(matchingTrip, otherRouteTrip);
        await context.SaveChangesAsync();

        var service = new TripService(context);

        // Act
        var result = await service.SearchTripAsync(lilongwe.Id, blantyre.Id, DepartureDate);

        // Assert
        Assert.True(result.Success);
        var found = Assert.Single(result.Data!);
        Assert.Equal(matchingTrip.Id, found.TripId);
    }

    [Fact]
    public async Task SearchTripAsync_ExcludesTripsOnOtherDates()
    {
        // Arrange
        await using var context = CreateContext();
        var lilongwe = CreateDepot("LILONGWE-001", "Lilongwe Central", "Lilongwe");
        var blantyre = CreateDepot("BLANTYRE-001", "Blantyre Central", "Blantyre");
        var bus = CreateBus(lilongwe);

        var matchingTrip = CreateTrip(bus, lilongwe, blantyre, DepartureTime);
        var nextDayTrip = CreateTrip(bus, lilongwe, blantyre, DepartureTime.AddDays(1));
        context.Trips.AddRange(matchingTrip, nextDayTrip);
        await context.SaveChangesAsync();

        var service = new TripService(context);

        // Act
        var result = await service.SearchTripAsync(lilongwe.Id, blantyre.Id, DepartureDate);

        // Assert
        Assert.True(result.Success);
        var found = Assert.Single(result.Data!);
        Assert.Equal(matchingTrip.Id, found.TripId);
    }

    [Fact]
    public async Task SearchTripAsync_ReturnsEmptyList_WhenNoTripsMatch()
    {
        // Arrange
        await using var context = CreateContext();
        var lilongwe = CreateDepot("LILONGWE-001", "Lilongwe Central", "Lilongwe");
        var blantyre = CreateDepot("BLANTYRE-001", "Blantyre Central", "Blantyre");
        context.Trips.Add(CreateTrip(CreateBus(lilongwe), lilongwe, blantyre, DepartureTime.AddDays(3)));
        await context.SaveChangesAsync();

        var service = new TripService(context);

        // Act
        var result = await service.SearchTripAsync(lilongwe.Id, blantyre.Id, DepartureDate);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data!);
    }

    private static readonly Guid SameDepotId = Guid.NewGuid();

    public static TheoryData<Guid, Guid, DateOnly> InvalidSearchInputs => new()
    {
        { Guid.Empty, Guid.NewGuid(), DepartureDate },   // missing origin
        { Guid.NewGuid(), Guid.Empty, DepartureDate },   // missing destination
        { SameDepotId, SameDepotId, DepartureDate },     // origin equals destination
        { Guid.NewGuid(), Guid.NewGuid(), DateOnly.MinValue } // missing date
    };

    [Theory]
    [MemberData(nameof(InvalidSearchInputs))]
    public async Task SearchTripAsync_ReturnsBadRequest_ForInvalidInput(Guid originDepotId, Guid destinationDepotId, DateOnly departureDate)
    {
        // Arrange
        await using var context = CreateContext();
        var service = new TripService(context);

        // Act
        var result = await service.SearchTripAsync(originDepotId, destinationDepotId, departureDate);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultStatus.BadRequest, result.Status);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
    }

    #endregion

    #region GetTripByIdAsync

    [Fact]
    public async Task GetTripByIdAsync_ReturnsTripDetails()
    {
        // Arrange
        await using var context = CreateContext();
        var lilongwe = CreateDepot("LILONGWE-001", "Lilongwe Central", "Lilongwe");
        var blantyre = CreateDepot("BLANTYRE-001", "Blantyre Central", "Blantyre");
        var trip = CreateTrip(CreateBus(lilongwe), lilongwe, blantyre, DepartureTime, seatsBooked: 6, cargoCapacityBooked: 40);
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        var service = new TripService(context);

        // Act
        var result = await service.GetTripByIdAsync(trip.Id);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.NotNull(result.Data);

        var details = result.Data!;
        Assert.Equal(trip.Id, details.TripId);
        Assert.Equal("Lilongwe Central", details.OriginDepotName);
        Assert.Equal("Blantyre Central", details.DestinationDepotName);
        Assert.Equal("BUS-101", details.BusFleetNumber);
        Assert.Equal(DepartureTime, details.DepartureTime);
        Assert.Equal(3500m, details.SeatFare);
        Assert.Equal(40, details.TotalSeats);
        Assert.Equal(34, details.AvailableSeats);
        Assert.Equal(160, details.AvailableCargoCapacity);
    }

    [Fact]
    public async Task GetTripByIdAsync_ReturnsNotFound_WhenTripDoesNotExist()
    {
        // Arrange
        await using var context = CreateContext();
        var service = new TripService(context);

        // Act
        var result = await service.GetTripByIdAsync(Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Null(result.Data);
    }

    #endregion

    #region Helpers

    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static Depot CreateDepot(string code, string name, string city) => new()
    {
        DepotCode = code,
        Name = name,
        IsActive = true,
        Address = new Address
        {
            City = city,
            Region = "Central",
            Country = "Malawi"
        }
    };

    // 40 seats, 200 cargo capacity.
    private static Bus CreateBus(Depot homeDepot) => new()
    {
        FleetNumber = "BUS-101",
        Model = "Coaster",
        NumberOfSeats = 40,
        CargoCapacity = 200,
        HasCargoSpace = true,
        BusType = BusType.Coach,
        RegistrationNumber = "ABC-123",
        ManufacturerName = "Toyota",
        ManufactureYear = 2023,
        BusStatus = BusStatus.Available,
        HomeDepot = homeDepot,
        RowVersion = new byte[] { 1 }
    };

    private static Trip CreateTrip(
        Bus bus,
        Depot origin,
        Depot destination,
        DateTimeOffset departureTime,
        int seatsBooked = 0,
        int cargoCapacityBooked = 0) => new()
    {
        Bus = bus,
        Route = new Route
        {
            Origin = origin,
            Destination = destination
        },
        SeatFare = 3500m,
        CargoFare = 800m,
        SeatsBooked = seatsBooked,
        CargoCapacityBooked = cargoCapacityBooked,
        DepartureTime = departureTime,
        ArrivalTime = departureTime.AddHours(5),
        RowVersion = new byte[] { 1 }
    };

    #endregion
}
