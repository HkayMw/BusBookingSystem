

namespace BusBookingSystem.Application.DTOs
{
    public class BusDto
    {
        public Guid Id { get; set; }
        public string? FleetNumber { get; set; }
        public string Model { get; set; } = default!;
        public int Capacity { get; set; }
        public string RegistrationNumber { get; set; } = default!;
        public string ManufacturerName { get; set; } = default!;
        public int ManufactureYear { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsActive { get; set; }
        //public DateTimeOffset CreatedAt { get; set; }
        //public DateTimeOffset? UpdatedAt { get; set; }


    }
}
