/* This file
 */
using BusBookingSystem.API.Models;
using BusBookingSystem.API.Extensions;
using BusBookingSystem.Application.DTOs;
using BusBookingSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController(ITripService tripService) : ControllerBase
    {
        private readonly ITripService _tripService = tripService;

        [HttpGet]
        [EndpointSummary("Get all available trips.")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TripListResultDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<TripListResultDto>>>> GetAllTrips()
        {
            var result = await _tripService.GetAllTripsAsync();
            var response = result.ToApiResponse();

            return StatusCode(response.StatusCode, response.Body);
        }

        [HttpGet("search")]
        [EndpointSummary("Search trips by origin, destination, and departure date.")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TripListResultDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<TripListResultDto>>>> SearchTrips([FromQuery] Guid originDepotId, [FromQuery] Guid destinationDepotId, [FromQuery] DateOnly departureDate)
        {
            var result = await _tripService.SearchTripAsync(originDepotId, destinationDepotId, departureDate);
            var response = result.ToApiResponse();

            return StatusCode(response.StatusCode, response.Body);

        }

        [HttpGet("{tripId:guid}")]
        [EndpointSummary("Get details for a selected trip.")]
        [ProducesResponseType(typeof(ApiResponse<TripDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<TripDto>>> GetTripById(Guid tripId)
        {
            var result = await _tripService.GetTripByIdAsync(tripId);
            var response = result.ToApiResponse();

            return StatusCode(response.StatusCode, response.Body);
        }
    }
}
