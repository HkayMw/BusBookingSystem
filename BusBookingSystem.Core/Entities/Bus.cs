using BusBookingSystem.Core.Enums;

namespace BusBookingSystem.Core.Entities
{
    public class Bus
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string FleetNumber { get; set; } 
        //public string? Name { get; set; }
        public required string Model { get; set; }
        public required int NumberOfSeats { get; set; }
        public int CargoCapacity { get; set; }
        public required bool HasCargoSpace { get; set; }
        public required BusType BusType { get; set; }
        public required string RegistrationNumber { get; set; }
        public required string ManufacturerName { get; set; }
        public required int ManufactureYear { get; set; }
        public BusStatus BusStatus { get; set; }
        public Guid HomeDepotId { get; set; }
        public Depot HomeDepot { get; set; } = default!;
        public byte[] RowVersion { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }

    }
}
