namespace BusBookingSystem.Core.Entities
{
    public class Depot
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string DepotCode { get; set; }= null!;
        public required string Name { get; set; }
        public Address Address { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get;set; }
    }
}
