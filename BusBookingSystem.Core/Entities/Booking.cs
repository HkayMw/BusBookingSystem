using BusBookingSystem.Core.Enums;

namespace BusBookingSystem.Core.Entities
{
    public class Booking
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public required User User { get; set; }
        public Guid TripId { get; set; }
        public required Trip Trip { get; set; }
        public required int NumberOfSeats { get; set; }
        public int CargoWeight { get; set; }
        public required string BookingReference { get; set; } // TODO: Generate a unique booking reference
        public BookingStatus BookingStatus { get; set; }
        public byte[] RowVersion { get; set; } = default!;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
