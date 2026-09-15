using BusBookingSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepotsController(IDepotService depotService) : ControllerBase
    {
        private readonly IDepotService _depotService = depotService;

        [HttpGet]
        public async Task<IActionResult> GetAllDepots()
        {
            var depots = await _depotService.GetActiveDepotsAsync();
            return Ok(depots);
        }
    }
}
