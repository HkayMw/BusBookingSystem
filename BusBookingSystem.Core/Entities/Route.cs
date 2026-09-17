namespace BusBookingSystem.Core.Entities
{
    public class Route
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public Guid OriginId { get; set; }
        public required Depot Origin { get; set; }
        public Guid DestinationId { get; set; }
        public required Depot Destination { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
