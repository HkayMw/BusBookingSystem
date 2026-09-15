using BusBookingSystem.Application.DTOs;
using BusBookingSystem.Application.Interfaces.Services;
using BusBookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.Infrastructure.Services
{
    public class DepotService : IDepotService
    {
        private readonly AppDbContext _context;

        public DepotService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<DepotListDto>> GetActiveDepotsAsync()
        {
            // Implementation for getting active depots

            var activeDepots = await _context.Depots
                .Where(d => d.IsActive)
                .Select(d => new DepotListDto
                {
                    Id = d.Id,
                    DepotCode = d.DepotCode,
                    Name = d.Name,
                    City = d.Address.City
                }).AsNoTracking().ToListAsync();

            return activeDepots;
        }
    }

}
