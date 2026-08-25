namespace BusBookingSystem.Models
{
    public class Bus
    {
        public int Id { get; set; }
        public string BusNumber { get; set; }
        public string BusModel { get; set; }
        public int Capacity { get; set; }
        public string RegistrationNumber { get; set; }
        public DateTime ManufactureDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
