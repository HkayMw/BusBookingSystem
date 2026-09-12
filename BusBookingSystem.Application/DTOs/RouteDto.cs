

namespace BusBookingSystem.Application.DTOs
{
    public class RouteDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Origin { get; set; } = null!;
        public string Destination { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
