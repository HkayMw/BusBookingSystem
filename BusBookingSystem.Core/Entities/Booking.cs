using System;
using System.Collections.Generic;
using System.Text;

namespace BusBookingSystem.Core.Entities
{
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled
    }
    public class Booking
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Guid UserId { get; set; }
        public required Guid TripId { get; set; }
        public required int NumberOfSeats { get; set; }
        public required string BookingReference { get; set; } // TODO: Generate a unique booking reference
        public BookingStatus BookingStatus { get; set; }


    }
}
