

namespace BusBookingSystem.Application.DTOs
{
    public class BookingResponseDto
    {
        public Guid BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string TripSummary { get; set; } = string.Empty;
        public int NumberOfSeats { get; set; }
        public int CargoWeight { get; set; }
        public int SeatsFare { get; set; }
        public int CargoFare { get; set; }
        public int TotalFare { get; set; }
    }
}
