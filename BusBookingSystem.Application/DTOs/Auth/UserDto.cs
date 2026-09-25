
using BusBookingSystem.Core.Enums;

namespace BusBookingSystem.Application.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        //public string IdentityUserId { get; set; } = default!;
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? OtherNames { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public required UserType UserType { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
