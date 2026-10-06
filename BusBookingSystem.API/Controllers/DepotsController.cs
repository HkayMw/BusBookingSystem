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
    public class DepotsController(IDepotService depotService) : ControllerBase
    {
        private readonly IDepotService _depotService = depotService;

        [HttpGet]
        [EndpointSummary("Get active depots for trip search.")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DepotListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DepotListDto>>>> GetActiveDepots()
        {
            var result = await _depotService.GetActiveDepotsAsync();
            var response = result.ToApiResponse();

            return StatusCode(response.StatusCode, response.Body);
        }
    }
}
