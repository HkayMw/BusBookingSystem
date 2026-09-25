using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Application.DTOs
{
    public class UserLoginRequestDto
    {
        [Required, EmailAddress, MaxLength(320)]
        public required string Email { get; set; }

        [Required, MinLength(8), MaxLength(100)]
        public required string Password { get; set; }
    }
}