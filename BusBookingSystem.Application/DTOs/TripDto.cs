using System;
using System.Collections.Generic;
using System.Text;

namespace BusBookingSystem.Application.DTOs
{
    public class TripDto
    {
        public Guid Id { get; set; }
        public string TripCode { get; set; } = null!;
        public Guid RouteId { get; set; }
        public string RouteName { get; set; } = null!;
        public Guid BusId { get; set; }
        public string BusFleetNumber { get; set; } = null!;
        public DateTimeOffset DepartureTime { get; set; }
        public DateTimeOffset ArrivalTime { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}
