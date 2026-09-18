using BusBookingSystem.Application.DTOs;
using BusBookingSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController(IBookingService bookingService) : ControllerBase
    {
        private readonly IBookingService _bookingService = bookingService;

        [HttpPost]
        public async Task<ActionResult<BookingResponseDto>> CreateBooking([FromBody] BookingRequestDto  requestDto)
        {
            var booking = await _bookingService.CreateBookingAsync(requestDto);
            return Ok(booking);
        }
    }
}
