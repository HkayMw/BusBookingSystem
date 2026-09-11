using System;
using System.Collections.Generic;
using System.Text;

namespace BusBookingSystem.Core.Entities
{
    public class Trip
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Guid BusId { get; set; }
        public required Guid RouteId { get; set; }
        public required DateTimeOffset DepartureTime { get; set; }
        public DateTimeOffset? ArrivalTime { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
