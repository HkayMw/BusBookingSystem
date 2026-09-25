
using BusBookingSystem.Application.Common;
using BusBookingSystem.Application.DTOs;
using BusBookingSystem.Core.Entities;

namespace BusBookingSystem.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<Result<UserDto>> RegisterAsync(UserRegisterRequestDto userRegisterRequestDto);
        Task<Result<AuthResponseDto>> LoginAsync(UserLoginRequestDto userLoginRequestDto);
        Task<bool> IsEmailExistAsync(String email);
        //TODO: Logout
    }
}
