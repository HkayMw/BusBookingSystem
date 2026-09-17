/* This file 
 */

using BusBookingSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepotsController(IDepotService depotService) : ControllerBase
    {
        private readonly IDepotService _depotService = depotService;

        [HttpGet]
        public async Task<IActionResult> GetActiveDepots()
        {
            var depots = await _depotService.GetActiveDepotsAsync();
            return Ok(depots);
        }
    }
}
