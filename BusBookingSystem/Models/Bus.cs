namespace BusBookingSystem.Models
{
    public class Bus
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string BusNumber { get; set; }
        public required string BusModel { get; set; }
        public int Capacity { get; set; }
        public required string RegistrationNumber { get; set; }
        public DateTime ManufactureDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
