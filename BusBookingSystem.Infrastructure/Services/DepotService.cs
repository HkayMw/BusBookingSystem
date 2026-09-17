/*
    This file defines a DepotService class that implements the IDepotService interface. 
    It uses Entity Framework Core to interact with the database.

    The service provides various methods to manage depots
*/

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

        /* The GetActiveDepotsAsync method retrieves a list of active depots from the database. 
           It uses LINQ to filter depots based on their IsActive property and projects the results into DepotListDto objects. 
           The method returns a read-only list of DepotListDto objects asynchronously.
        */
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
