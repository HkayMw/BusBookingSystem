
namespace BusBookingSystem.Application.DTOs
{
    public class TripListResultDto
    {
        public Guid TripId { get; set; }
        //public string Bus { get; set; } = default!; 
        public string OriginDepotName { get; set; } = default!;
        public string DestinationDepotName { get; set; } = default!;
        public DateTimeOffset DepartureTime { get; set; }
        public DateTimeOffset? ArrivalTime { get; set; }
        public decimal SeatFare { get; set; }
        public decimal? CargoFare { get; set; }
        //public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public bool HasCargoSpace { get; set; }
        //public int TotalCargoCapacity { get; set; }
        public int AvailableCargoCapacity { get; set; }
    }
}
