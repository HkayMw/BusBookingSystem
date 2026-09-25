namespace BusBookingSystem.Core.Entities
{
    public class Depot
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string DepotCode { get; set; }= default!;
        public required string Name { get; set; }
        public required Address Address { get; set; }
        public string? PhoneNumber { get; set; }
        //TODO: consider using decimal,double, or NetTopologySuite's Point for latitude and longitude instead of string for better precision and validation
        public string? Latitude { get; set; } 
        public string? Longitude { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get;set; }
    }
}
