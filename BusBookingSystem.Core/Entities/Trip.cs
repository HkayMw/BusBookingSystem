namespace BusBookingSystem.Core.Entities
{
    public class Trip
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Bus Bus { get; set; }
        public required Route Route { get; set; }
        public required decimal BaseFare { get; set; }
        public int SeatsBooked { get; set; } = 0;
        public byte[]? RowVersion { get; set; }
        public required DateTimeOffset DepartureTime { get; set; }
        public DateTimeOffset? ArrivalTime { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
