namespace BusBookingSystem.Core.Entities
{
    public class Trip
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BusId { get; set; }
        public Bus Bus { get; set; } = null!;
        public Guid RouteId { get; set; }
        public Route Route { get; set; } = null!;
        public required decimal SeatFare { get; set; }
        public decimal? CargoFare { get; set; }
        public int SeatsBooked { get; set; } = 0;
        public int CargoCapacityBooked { get; set; } = 0;
        public byte[] RowVersion { get; set; } = null!;
        public required DateTimeOffset DepartureTime { get; set; }
        public DateTimeOffset? ArrivalTime { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
