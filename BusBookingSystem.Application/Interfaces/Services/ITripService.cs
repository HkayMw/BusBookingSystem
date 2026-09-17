

using BusBookingSystem.Application.DTOs;

namespace BusBookingSystem.Application.Interfaces.Services
{
    public interface ITripService
    {
        Task<IReadOnlyList<TripListResultDto>> GetAllTripsAsync();
        Task<IReadOnlyList<TripListResultDto>> SearchTripAsync
            (
                Guid originDepotId,
                Guid destinationDepotId,
                DateOnly departureDate
            );

    }
}
