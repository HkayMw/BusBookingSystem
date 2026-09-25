using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Application.DTOs
{
    public class UserRegisterRequestDto
    {
        [Required, StringLength(50)]
        public required string FirstName { get; set; }

        [Required, StringLength(50)]
        public required string LastName { get; set; }

        [StringLength(100)]
        public string? OtherNames { get; set; }

        [Required, EmailAddress, StringLength(320)]
        public required string Email { get; set; }

        [Phone, StringLength(20)]
        public string? Phone { get; set; }

        [Required, StringLength(100, MinimumLength = 8)]
        public required string Password { get; set; }
    }
}