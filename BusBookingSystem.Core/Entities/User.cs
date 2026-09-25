using BusBookingSystem.Core.Enums;

namespace BusBookingSystem.Core.Entities
{
    

    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        //public string IdentityUserId { get; set; } = default!;
        //public required string UserName { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? OtherNames { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public string? Phone { get; set; }
        public required UserType UserType { get; set; } = UserType.Customer;
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
