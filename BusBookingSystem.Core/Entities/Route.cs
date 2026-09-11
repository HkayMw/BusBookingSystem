namespace BusBookingSystem.Core.Entities
{
    public class Route
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public required Depot OriginDepot { get; set; }
        public required Depot DestinationDepot { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
