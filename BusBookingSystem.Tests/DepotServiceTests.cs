using BusBookingSystem.Application.Common;
using BusBookingSystem.Core.Entities;
using BusBookingSystem.Infrastructure.Data;
using BusBookingSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.Tests;

public class DepotServiceTests
{
    [Fact]
    public async Task GetActiveDepotsAsync_OnlyReturnsActiveDepots()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);

        context.Depots.AddRange(
            new Depot
            {
                DepotCode = "ACTIVE-001",
                Name = "Mzuzu Depot",
                IsActive = true,
                Address = new Address
                {
                    City = "Mzuzu",
                    Region = "Northen",
                    Country = "Malawi"
                }
            },
            new Depot
            {
                DepotCode = "INACTIVE-001",
                Name = "Jenda Depot",
                IsActive = false,
                Address = new Address
                {
                    City = "Mzimba",
                    Region = "Northen",
                    Country = "Malawi"
                }
            });

        await context.SaveChangesAsync();

        var service = new DepotService(context);

        // Act
        var result = await service.GetActiveDepotsAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.NotNull(result.Data);
        var depots = result.Data!.ToList();

        Assert.Single(depots);
        Assert.Equal("ACTIVE-001", depots[0].DepotCode);
        Assert.Equal("Mzuzu Depot", depots[0].Name);
        Assert.Equal("Mzuzu", depots[0].City);
        Assert.DoesNotContain(depots, d => d.DepotCode == "INACTIVE-001");
    }

    [Fact]
    public async Task GetActiveDepotsAsync_ReturnsEmptyList_WhenNoDepotsAreActive()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);

        context.Depots.Add(
            new Depot
            {
                DepotCode = "INACTIVE-001",
                Name = "Jenda Depot",
                IsActive = false,
                Address = new Address
                {
                    City = "Mzimba",
                    Region = "Northen",
                    Country = "Malawi"
                }
            });

        await context.SaveChangesAsync();

        var service = new DepotService(context);

        // Act
        var result = await service.GetActiveDepotsAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data!);
    }
}
