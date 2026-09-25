

namespace BusBookingSystem.Application.DTOs
{
    public class RouteDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Origin { get; set; } = default!;
        public string Destination { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
