
namespace BusBookingSystem.Application.DTOs
{
    public class BookingRequestDto
    {
        // public Guid UserId { get; set; }
        public Guid TripId { get; set; }
        public int NumberOfSeats { get; set; }
        public int CargoWeight { get; set; } // TODO: consider cargo volume
    }
}
