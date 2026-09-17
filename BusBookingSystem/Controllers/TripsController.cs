/* This file
 */

using BusBookingSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController(ITripService tripService) : ControllerBase
    {
        private readonly ITripService _tripService = tripService;

        [HttpGet]
        public async Task<IActionResult> GetAllTrips()
        {
            var trips = await _tripService.GetAllTripsAsync();
            return Ok(trips);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchTrips([FromQuery] Guid originDepotId, [FromQuery] Guid destinationDepotId, [FromQuery] DateOnly departureDate)
        {
            var trips = await _tripService.SearchTripAsync(originDepotId, destinationDepotId, departureDate);
            return Ok(trips);
        }
    }
}
