
using BusBookingSystem.Application.Common;
using BusBookingSystem.Application.DTOs;

namespace BusBookingSystem.Application.Interfaces.Services
{
    public interface ITripService
    {
        Task<Result<IReadOnlyList<TripListResultDto>>> GetAllTripsAsync();
        Task<Result<IReadOnlyList<TripListResultDto>>> SearchTripAsync
            (
                Guid originDepotId,
                Guid destinationDepotId,
                DateOnly departureDate
            );
        Task<Result<TripDto>> GetTripByIdAsync(Guid tripId);

    }
}
