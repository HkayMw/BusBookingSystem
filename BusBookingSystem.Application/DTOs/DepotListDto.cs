


namespace BusBookingSystem.Application.DTOs
{
    public class DepotListDto
    {
        public Guid Id { get; set; }
        public string DepotCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? City { get; set; } = null!; 
    }
}
