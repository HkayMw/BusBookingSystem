using BusBookingSystem.Application.DTOs;

namespace BusBookingSystem.Application.Interfaces.Services
{
    public interface IDepotService
    {
        Task<IReadOnlyList<DepotListDto>> GetActiveDepotsAsync();
    }
}
