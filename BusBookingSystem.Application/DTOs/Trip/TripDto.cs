
namespace BusBookingSystem.Application.DTOs
{
    public class TripDto
    {
        public Guid TripId { get; set; } // from t.Id
        public string Route { get; set; } = default!; // from t.Route.Origin.Name + " - " + t.Route.Destination.Name
        public string BusFleetNumber { get; set; } = default!; // from t.Bus.FleetNumber
        public string BusType { get; set; } = default!; // from t.Bus.BusType.ToString()
        public string OriginDepotName { get; set; } = default!; // from t.Route.Origin.Name
        public string DestinationDepotName { get; set; } = default!; // from t.Route.Destination.Name
        public DateTimeOffset DepartureTime { get; set; } // from t.DepartureTime
        public DateTimeOffset? ArrivalTime { get; set; } // from t.ArrivalTime
        public decimal SeatFare { get; set; } // from t.SeatFare
        public decimal? CargoFare { get; set; } // from t.CargoFare
        public int TotalSeats { get; set; } // from t.Bus.NumberOfSeats
        public int AvailableSeats { get; set; } // from t.Bus.NumberOfSeats - t.SeatsBooked
        public bool HasCargoSpace { get; set; } // from t.Bus.HasCargoSpace
        public int TotalCargoCapacity { get; set; } // from t.Bus.CargoCapacity
        public int AvailableCargoCapacity { get; set; } // from t.Bus.CargoCapacity - t.CargoCapacityBooked
    }
}
