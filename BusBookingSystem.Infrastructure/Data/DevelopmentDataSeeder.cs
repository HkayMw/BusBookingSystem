using BusBookingSystem.Core.Entities;
using BusBookingSystem.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.Infrastructure.Data
{
    public class DevelopmentDataSeeder
    {
        public static async Task SeedAsync(AppDbContext dbContext)
        {
            // 0. useful variables
            var malawiTime = TimeSpan.FromHours(2);
            var today = DateTimeOffset.UtcNow.ToOffset(malawiTime).Date;
            var tomorrow7am = new DateTimeOffset(today.AddDays(1).AddHours(7), malawiTime);
            var tomorrow9am = new DateTimeOffset(today.AddDays(1).AddHours(9), malawiTime);
            var tomorrow2pm = new DateTimeOffset(today.AddDays(1).AddHours(14), malawiTime);
            //var dayAfterTomorrow7pm = new DateTimeOffset(today.AddDays(2).AddHours(7), malawiTime);

            // 1. Idempotency guard: if any depot exists, assume data has already been seeded
            if (await dbContext.Depots.AnyAsync())
            {
                return;
            }


            // 2. Depots
            var lilongweDepot = new Depot
            {
                DepotCode = "LLW-CEN",
                Name = "Lilongwe Central",
                Address = new Address
                {
                    City = "Lilongwe",
                    Region = "Central",
                    Country = "Malawi"
                }
            };

            var blantyreDepot = new Depot
            {
                DepotCode = "BTC-001",
                Name = "Blantyre City",
                Address = new Address
                {
                    City = "Blantyre",
                    Region = "Southern",
                    Country = "Malawi"
                }
            };

            var mzuzuDepot = new Depot
            {
                DepotCode = "MZI-MZC",
                Name = "Mzuzu City",
                Address = new Address
                {
                    City = "Mzuzu",
                    Region = "Northern",
                    Country = "Malawi"
                }
            };

            var jendaDepot = new Depot
            {
                DepotCode = "MZI-JND",
                Name = "Jenda Depot",
                IsActive = false,
                Address = new Address
                {
                    City = "Mzimba",
                    Region = "Northern",
                    Country = "Malawi"
                }
            };

            var dedzaDepot = new Depot
            {
                DepotCode = "DDZ-TWN",
                Name = "Dedza Depot",
                Address = new Address
                {
                    City = "Dedza",
                    Region = "Central",
                    Country = "Malawi"
                }
            };

            dbContext.Depots.AddRange(lilongweDepot, blantyreDepot, mzuzuDepot, jendaDepot, dedzaDepot);

            // 3. Buses
            var lilongweBus = new Bus
            {
                ManufacturerName = "YUTONG",
                FleetNumber = "001",
                Model = "ZK6907H",
                ManufactureYear = 2015,
                RegistrationNumber = "LL 1043",
                BusType = BusType.Coach,
                BusStatus = BusStatus.Available,
                NumberOfSeats = 41,
                HasCargoSpace = true,
                CargoCapacity = 1000,
                HomeDepot = lilongweDepot
            };

            var lilongweMiniBus = new Bus
            {
                ManufacturerName = "YUTONG",
                FleetNumber = "004",
                Model = "ZK6600H",
                ManufactureYear = 2015,
                RegistrationNumber = "LL 1044",
                BusType = BusType.Minibus,
                BusStatus = BusStatus.Available,
                NumberOfSeats = 25,
                HasCargoSpace = false,
                HomeDepot = lilongweDepot
            };

            var mzuzuBus = new Bus
            {
                ManufacturerName = "YUTONG",
                FleetNumber = "002",
                Model = "C12PRO",
                ManufactureYear = 2023,
                RegistrationNumber = "MZ 1057",
                BusType = BusType.Coach,
                BusStatus = BusStatus.Available,
                NumberOfSeats = 65,
                HasCargoSpace = true,
                CargoCapacity = 1000,
                HomeDepot = mzuzuDepot
            };

            var blantyreBus = new Bus
            {
                ManufacturerName = "YUTONG",
                FleetNumber = "003",
                Model = "D12",
                ManufactureYear = 2018,
                RegistrationNumber = "BT 1098",
                BusType = BusType.Coach,
                BusStatus = BusStatus.Available,
                NumberOfSeats = 56,
                HasCargoSpace = true,
                CargoCapacity = 1000,
                HomeDepot = blantyreDepot
            };

            dbContext.Buses.AddRange(lilongweBus, lilongweMiniBus, blantyreBus, mzuzuBus);

            // 4. Routes
            var lilongweToDedza = new Route
            {
                Origin = lilongweDepot,
                Destination = dedzaDepot
            };
            var lilongweToBlantyre = new Route
            {
                Origin = lilongweDepot,
                Destination = blantyreDepot
            };

            var blantyreToMzuzu = new Route
            {
                Origin = blantyreDepot,
                Destination = mzuzuDepot,
            };

            var blantyreToLilongwe = new Route
            {
                Origin = blantyreDepot,
                Destination = lilongweDepot
            };

            var lilongweToMzuzu = new Route
            {
                Origin = lilongweDepot,
                Destination = mzuzuDepot,
            };

            var mzuzuToLilongwe = new Route
            {
                Origin = mzuzuDepot,
                Destination = lilongweDepot,
            };


            dbContext.Routes.AddRange(lilongweToBlantyre, blantyreToLilongwe, lilongweToMzuzu, mzuzuToLilongwe, blantyreToMzuzu, lilongweToDedza);

            // 5. Trips
            var blantyreToLilongweTrip = new Trip
            {
                Bus = blantyreBus,
                Route = blantyreToLilongwe,
                SeatFare = 70000m,
                CargoFare = 800m,
                DepartureTime = tomorrow7am
            };

            var lilongweToBlantyreTrip = new Trip
            {
                Bus = blantyreBus,
                Route = lilongweToBlantyre,
                SeatFare = 70000m,
                CargoFare = 800m,
                DepartureTime = tomorrow2pm
            };

            var mzuzuToLilongweTrip = new Trip
            {
                Bus = mzuzuBus,
                Route = mzuzuToLilongwe,
                SeatFare = 80000m,
                CargoFare = 1000m,
                DepartureTime = tomorrow7am
            };

            var lilongweToMzuzuTrip = new Trip
            {
                Bus = mzuzuBus,
                Route = lilongweToMzuzu,
                SeatFare = 80000m,
                CargoFare = 1000m,
                DepartureTime = tomorrow2pm
            };

            var lilongweToDedzaTrip = new Trip
            {
                Bus = lilongweMiniBus,
                Route = lilongweToDedza,
                SeatFare = 30000m,
                DepartureTime = tomorrow9am
            };

            dbContext.Trips.AddRange(blantyreToLilongweTrip, lilongweToBlantyreTrip, mzuzuToLilongweTrip, lilongweToMzuzuTrip, lilongweToDedzaTrip);

            await dbContext.SaveChangesAsync();



        }
    }
}
