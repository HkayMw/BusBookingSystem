using BusBookingSystem.Core.Enums;

namespace BusBookingSystem.Core.Entities
{
    

    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string IdentityUserId { get; set; } = null!;
        public required string UserName { get; set; } = null!;
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? OtherNames { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPhone { get; set; }
        public required UserType UserType { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
