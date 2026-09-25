


namespace BusBookingSystem.Application.DTOs
{
    public class DepotDto
    {
        public Guid Id { get; set; }
        public string DepotCode { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Address { get; set; } = default!; // Flattened address property
        public string? PhoneNumber { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public bool IsActive { get; set; }
        //public DateTimeOffset CreatedAt { get; set; }
        //public DateTimeOffset? UpdatedAt { get; set; }
    }
}
