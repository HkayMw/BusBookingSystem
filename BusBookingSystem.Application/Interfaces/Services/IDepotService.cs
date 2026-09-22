using BusBookingSystem.Application.Common;
using BusBookingSystem.Application.DTOs;

namespace BusBookingSystem.Application.Interfaces.Services
{
    public interface IDepotService
    {
        Task<Result<IReadOnlyList<DepotListDto>>> GetActiveDepotsAsync();
    }
}
