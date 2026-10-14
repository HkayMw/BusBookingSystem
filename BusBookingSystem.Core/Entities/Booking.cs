using BusBookingSystem.Core.Enums;

namespace BusBookingSystem.Core.Entities
{
    public class Booking
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public Guid TripId { get; set; }
        public Trip Trip { get; set; } = default!;
        public required int NumberOfSeats { get; set; }
        public int CargoWeight { get; set; }
        public required string BookingReference { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public byte[] RowVersion { get; set; } = default!;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
