using BusBookingSystem.API.Extensions;
using BusBookingSystem.API.Models;
using BusBookingSystem.Application.DTOs;
using BusBookingSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            this._authService = authService;
        }

        [HttpPost("register")]
        [EndpointSummary("Register a new customer account.")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserDto>>> RegisterAsync(UserRegisterRequestDto userRegisterRequestDto)
        {
            {
                var result = await _authService.RegisterAsync(userRegisterRequestDto);
                var response = result.ToApiResponse();

                return StatusCode(response.StatusCode, response.Body);

            }
        }

        [HttpPost("login")]
        [EndpointSummary("Authenticate a customer and issue a JWT.")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> LoginAsync(UserLoginRequestDto userLoginRequestDto)
        {
            {
                var result = await _authService.LoginAsync(userLoginRequestDto);
                var response = result.ToApiResponse();

                return StatusCode(response.StatusCode, response.Body);

            }
        }



    }
}
