/* This file defines a TripService class that implements the ITripService interface. 
   It is intended to provide methods for managing trips in the Bus Booking System application.
*/

using BusBookingSystem.Application.Common;
using BusBookingSystem.Application.DTOs;
using BusBookingSystem.Application.Interfaces.Services;
using BusBookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.Infrastructure.Services
{
    public class TripService : ITripService
    {
        private readonly AppDbContext _context;

        public TripService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<IReadOnlyList<TripListResultDto>>> GetAllTripsAsync()
        {
            var trips = await _context.Trips
                .Select(t => new TripListResultDto
                {
                    TripId = t.Id,
                    OriginDepotName = t.Route.Origin.Name,
                    DestinationDepotName = t.Route.Destination.Name,
                    DepartureTime = t.DepartureTime,
                    ArrivalTime = t.ArrivalTime,
                    SeatFare = t.SeatFare,
                    CargoFare = t.CargoFare,
                    AvailableSeats = t.Bus.NumberOfSeats - t.SeatsBooked,
                    HasCargoSpace = t.Bus.HasCargoSpace,
                    AvailableCargoCapacity = t.Bus.CargoCapacity - t.CargoCapacityBooked
                }).AsNoTracking().ToListAsync();

            return Result<IReadOnlyList<TripListResultDto>>.Ok(trips, "Trips retrieved successfully.");
        }

        public async Task<Result<TripDto>> GetTripByIdAsync(Guid tripId)
        {

            var trip = await _context.Trips
                .Where(t => t.Id == tripId)
                .Select(t => new TripDto
                {
                    TripId = t.Id,
                    Route = $"{t.Route.Origin.Name} - {t.Route.Destination.Name}",
                    BusFleetNumber = t.Bus.FleetNumber,
                    BusType = t.Bus.BusType.ToString(),
                    OriginDepotName = t.Route.Origin.Name,
                    DestinationDepotName = t.Route.Destination.Name,
                    DepartureTime = t.DepartureTime,
                    ArrivalTime = t.ArrivalTime,
                    SeatFare = t.SeatFare,
                    CargoFare = t.CargoFare,
                    TotalSeats = t.Bus.NumberOfSeats,
                    AvailableSeats = t.Bus.NumberOfSeats - t.SeatsBooked,
                    HasCargoSpace = t.Bus.HasCargoSpace,
                    TotalCargoCapacity = t.Bus.CargoCapacity,
                    AvailableCargoCapacity = t.Bus.CargoCapacity - t.CargoCapacityBooked
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return trip is null
                ? Result<TripDto>.NotFound("Trip was not found.")
                : Result<TripDto>.Ok(trip, "Trip retrieved successfully.");
        }

        public async Task<Result<IReadOnlyList<TripListResultDto>>> SearchTripAsync(Guid originDepotId, Guid destinationDepotId, DateOnly departureDate)
        {
            // validate input parameters

            if (originDepotId == Guid.Empty)
            {
                return Result<IReadOnlyList<TripListResultDto>>.Invalid("Origin depot ID cannot be empty.", nameof(originDepotId));
            }

            if (destinationDepotId == Guid.Empty)
            {
                return Result<IReadOnlyList<TripListResultDto>>.Invalid("Destination depot ID cannot be empty.", nameof(destinationDepotId));
            }

            if (originDepotId == destinationDepotId)
            {
                return Result<IReadOnlyList<TripListResultDto>>.Invalid("Origin and destination depot IDs cannot be the same.");
            }

            if (departureDate == DateOnly.MinValue)
            {
                return Result<IReadOnlyList<TripListResultDto>>.Invalid("Departure date cannot be empty.", nameof(departureDate));
            }





            var trips = await _context.Trips
                .Where(t => t.Route.OriginId == originDepotId &&
                            t.Route.DestinationId == destinationDepotId &&
                            t.DepartureTime.Date == departureDate.ToDateTime(TimeOnly.MinValue).Date)
                .Select(t => new TripListResultDto
                {
                    TripId = t.Id,
                    //Bus = t.Bus.FleetNumber,
                    OriginDepotName = t.Route.Origin.Name,
                    DestinationDepotName = t.Route.Destination.Name,
                    DepartureTime = t.DepartureTime,
                    ArrivalTime = t.ArrivalTime,
                    SeatFare = t.SeatFare,
                    CargoFare = t.CargoFare,
                    //TotalSeats = t.Bus.NumberOfSeats,
                    AvailableSeats = t.Bus.NumberOfSeats - t.SeatsBooked,
                    HasCargoSpace = t.Bus.HasCargoSpace,
                    //TotalCargoCapacity = t.Bus.CargoCapacity,
                    AvailableCargoCapacity = t.Bus.CargoCapacity - t.CargoCapacityBooked
                }).AsNoTracking().ToListAsync();

            return Result<IReadOnlyList<TripListResultDto>>.Ok(trips, "Trips searched successfully.");
        }
    }
}
