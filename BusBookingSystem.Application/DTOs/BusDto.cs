

namespace BusBookingSystem.Application.DTOs
{
    public class BusDto
    {
        public Guid Id { get; set; }
        public string? FleetNumber { get; set; }
        public string Model { get; set; } = null!;
        public int Capacity { get; set; }
        public string RegistrationNumber { get; set; } = null!;
        public string ManufacturerName { get; set; } = null!;
        public int ManufactureYear { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsActive { get; set; }
        //public DateTimeOffset CreatedAt { get; set; }
        //public DateTimeOffset? UpdatedAt { get; set; }


    }
}
