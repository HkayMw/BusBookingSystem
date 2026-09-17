/* This file
 */

using BusBookingSystem.Application.DTOs;
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
        public async Task<ActionResult<IEnumerable<TripListResultDto>>> GetAllTrips()
        {
            var trips = await _tripService.GetAllTripsAsync();
            //return trips is null ? NotFound() : Ok(trips);
            return Ok(trips);

        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TripListResultDto>>> SearchTrips([FromQuery] Guid originDepotId, [FromQuery] Guid destinationDepotId, [FromQuery] DateOnly departureDate)
        {
            var trips = await _tripService.SearchTripAsync(originDepotId, destinationDepotId, departureDate);
            return Ok(trips);

        }

        [HttpGet("{tripId:guid}")]
        public async Task<ActionResult<TripDto>> GetTripById(Guid tripId)
        {
            var trip = await _tripService.GetTripByIdAsync(tripId);

            return trip is null ? NotFound() : Ok(trip);
        }
    }
}
