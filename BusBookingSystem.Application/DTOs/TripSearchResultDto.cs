
namespace BusBookingSystem.Application.DTOs
{
    public class TripSearchResultDto
    {
        public Guid TripId { get; set; }
        public Guid RouteId { get; set; }
        //public Guid BusId { get; set; }
        public string BusName { get; set; } = null!;
        //public Guid OriginId { get; set; }
        public string OriginName { get; set; } = null!;
        //public Guid DestinationId { get; set; }
        public string DestinationName { get; set; } = null!;
        public DateTime DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public decimal BaseFare { get; set; }
        public int AvailableSeats { get; set; }
    }
}
