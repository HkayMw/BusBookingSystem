


namespace BusBookingSystem.Application.DTOs
{
    public class DepotListDto
    {
        public Guid Id { get; set; }
        public string DepotCode { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? City { get; set; } = default!; 
    }
}
