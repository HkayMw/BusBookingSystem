using System;
using System.Collections.Generic;
using System.Text;

namespace BusBookingSystem.Core.Entities
{
    internal class Customer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string? Address { get; set; }
    }
}
