

using BusBookingSystem.Core.Enums;

namespace BusBookingSystem.Application.DTOs
{
    public class BookingResponseDto
    {
        public Guid BookingId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string BookingReference { get; set; } = string.Empty;
        public BookingStatus BookingStatus { get; set; }
        public string TripSummary { get; set; } = string.Empty;
        public int NumberOfSeats { get; set; }
        public int CargoWeight { get; set; }
        public decimal SeatsFare { get; set; }
        public decimal CargoFare { get; set; }
        public decimal TotalFare { get; set; }
    }
}
