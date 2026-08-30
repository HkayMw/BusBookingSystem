using System;
using System.Collections.Generic;
using System.Text;

namespace BusBookingSystem.Core.Entities
{
    internal class Bus
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FleetNumber { get; set; }=null!;
        public required string Model { get; set; }
        public required int Capacity { get; set; }
        public required string RegistrationNumber { get; set; }
        public required string ManufacturerName { get; set; }
        public required int ManufactureYear { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }

    }
}
