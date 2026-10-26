using BusBookingSystem.API.Extensions;
using BusBookingSystem.API.Models;
using BusBookingSystem.Application.Common;
using BusBookingSystem.Application.DTOs;
using BusBookingSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingsController(IBookingService bookingService) : ControllerBase
    {
        private readonly IBookingService _bookingService = bookingService;


        [HttpPost]
        [EndpointSummary("Create a booking for the authenticated customer.")]
        [ProducesResponseType(typeof(ApiResponse<BookingResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<BookingResponseDto>>> CreateBooking([FromBody] BookingRequestDto requestDto)
        {
            if (User.GetUserId() is not Guid userId)
            {
                var responseEr = Result<BookingResponseDto>.UnAuthorized("No login session active.").ToApiResponse();
                return StatusCode(responseEr.StatusCode, responseEr.Body);
            }

            var result = await _bookingService.CreateBookingAsync(userId, requestDto);
            var response = result.ToApiResponse();

            return StatusCode(response.StatusCode, response.Body);
        }

        [HttpGet("{bookingId}")]
        [EndpointSummary("Get a booking by its ID.")]
        [ProducesResponseType(typeof(ApiResponse<BookingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BookingResponseDto>>> GetBookingById(Guid bookingId)
        {
            if (User.GetUserId() is not Guid userId)
            {
                var responseEr = Result<BookingResponseDto>.UnAuthorized("No login session active.").ToApiResponse();
                return StatusCode(responseEr.StatusCode, responseEr.Body);
            }

            var result = await _bookingService.GetBookingByIdAsync(userId, bookingId);
            var response = result.ToApiResponse();

            return StatusCode(response.StatusCode, response.Body);
        }
    }
}
