namespace BusBookingSystem.Core.Entities
{
    public class Address
    {
        public string? Addressline1 { get; set; }
        public string? Addressline2 { get; set; }
        public required string City { get; set; }
        public required string Region { get; set; }
        public required string Country { get; set; }
        public int? PostalCode { get; set; }
    }
}
