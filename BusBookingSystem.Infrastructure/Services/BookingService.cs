
using BusBookingSystem.Application.Common;
using BusBookingSystem.Application.DTOs;
using BusBookingSystem.Application.Exceptions;
using BusBookingSystem.Application.Interfaces.Services;
using BusBookingSystem.Core.Entities;
using BusBookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<BookingResponseDto>> CreateBookingAsync(BookingRequestDto requestDto)
        {
            if (requestDto is null)
            {
                return Result<BookingResponseDto>.Invalid("Booking request cannot be null.");
            }

            if (requestDto.UserId == Guid.Empty)
            {
                return Result<BookingResponseDto>.Invalid("User ID is required.", nameof(requestDto.UserId));
            }

            if (requestDto.TripId == Guid.Empty)
            {
                return Result<BookingResponseDto>.Invalid("Trip ID is required.", nameof(requestDto.TripId));
            }

            if (requestDto.NumberOfSeats < 0)
            {
                return Result<BookingResponseDto>.Invalid("Number of seats can not be negative.", nameof(requestDto.NumberOfSeats));
            }

            if (requestDto.CargoWeight < 0)
            {
                return Result<BookingResponseDto>.Invalid("Cargo weight cannot be negative.", nameof(requestDto.CargoWeight));
            }

            if (requestDto.CargoWeight == 0 && requestDto.NumberOfSeats == 0)
            {
                return Result<BookingResponseDto>.Invalid("A booking must include at least one seat or cargo quantity.", nameof(requestDto));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var userExists = await _context.Set<User>()
                    .AsNoTracking()
                    .AnyAsync(u => u.Id == requestDto.UserId);

                if (!userExists)
                {
                    await transaction.RollbackAsync();
                    return Result<BookingResponseDto>.NotFound("User was not found.");
                }

                var trip = await _context.Trips
                    .Include(t => t.Bus)
                    .Include(t => t.Route)
                    .ThenInclude(r => r.Origin)
                    .Include(t => t.Route)
                    .ThenInclude(r => r.Destination)
                    .FirstOrDefaultAsync(t => t.Id == requestDto.TripId);

                if (trip is null)
                {
                    await transaction.RollbackAsync();
                    return Result<BookingResponseDto>.NotFound("Trip was not found.");
                }

                var availableSeats = trip.Bus.NumberOfSeats - trip.SeatsBooked;
                if (requestDto.NumberOfSeats > availableSeats)
                {
                    await transaction.RollbackAsync();
                    return Result<BookingResponseDto>.Conflict($"Not enough seats available. Requested: {requestDto.NumberOfSeats}, Available: {availableSeats}.");
                }

                if (requestDto.CargoWeight > 0)
                {
                    if (!trip.Bus.HasCargoSpace)
                    {
                        await transaction.RollbackAsync();
                        return Result<BookingResponseDto>.Conflict("This bus does not support cargo bookings.");
                    }

                    var availableCargoCapacity = trip.Bus.CargoCapacity - trip.CargoCapacityBooked;
                    if (requestDto.CargoWeight > availableCargoCapacity)
                    {
                        await transaction.RollbackAsync();
                        return Result<BookingResponseDto>.Conflict($"Not enough cargo capacity available. Requested: {requestDto.CargoWeight}, Available: {availableCargoCapacity}.");
                    }
                }

                var bookingReference = $"BK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

                var booking = new Booking
                {
                    UserId = requestDto.UserId,
                    TripId = requestDto.TripId,
                    NumberOfSeats = requestDto.NumberOfSeats,
                    BookingReference = bookingReference,
                    BookingStatus = BusBookingSystem.Core.Enums.BookingStatus.Confirmed,
                    User = await _context.Set<User>().FirstAsync(u => u.Id == requestDto.UserId),
                    Trip = trip
                };

                trip.SeatsBooked += requestDto.NumberOfSeats;

                if (requestDto.CargoWeight > 0)
                {
                    trip.CargoCapacityBooked += requestDto.CargoWeight;
                }

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var seatsFare = (int)(trip.SeatFare * requestDto.NumberOfSeats);
                var cargoFare = trip.CargoFare.HasValue
                    ? (int)(trip.CargoFare.Value * requestDto.CargoWeight)
                    : 0;

                return Result<BookingResponseDto>.Ok(new BookingResponseDto
                {
                    BookingId = booking.Id,
                    BookingReference = booking.BookingReference,
                    TripSummary = $"{trip.Route.Origin.Name} → {trip.Route.Destination.Name} on {trip.DepartureTime:ddd, dd MMM yyyy}",
                    NumberOfSeats = booking.NumberOfSeats,
                    CargoWeight = requestDto.CargoWeight,
                    SeatsFare = seatsFare,
                    CargoFare = cargoFare,
                    TotalFare = seatsFare + cargoFare
                }, "Booking created successfully.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
