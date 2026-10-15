
using BusBookingSystem.Application.Common;
using BusBookingSystem.Application.DTOs;
using BusBookingSystem.Application.Interfaces.Services;
using BusBookingSystem.Core.Entities;
using BusBookingSystem.Core.Enums;
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

        /// <summary>
        /// Creates a new booking for a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="requestDto">The booking request details.</param>
        /// <returns>The created booking if successful, otherwise an error result.</returns>
        public async Task<Result<BookingResponseDto>> CreateBookingAsync(Guid userId, BookingRequestDto requestDto)
        {
            if (requestDto is null)
            {
                return Result<BookingResponseDto>.BadRequest("Booking request cannot be null.");
            }

            if (userId == Guid.Empty)
            {
                return Result<BookingResponseDto>.BadRequest("User ID is required.", nameof(userId));
            }

            if (requestDto.TripId == Guid.Empty)
            {
                return Result<BookingResponseDto>.BadRequest("Trip ID is required.", nameof(requestDto.TripId));
            }

            if (requestDto.NumberOfSeats < 0)
            {
                return Result<BookingResponseDto>.BadRequest("Number of seats can not be negative.", nameof(requestDto.NumberOfSeats));
            }

            if (requestDto.CargoWeight < 0)
            {
                return Result<BookingResponseDto>.BadRequest("Cargo weight cannot be negative.", nameof(requestDto.CargoWeight));
            }

            if (requestDto.CargoWeight == 0 && requestDto.NumberOfSeats == 0)
            {
                return Result<BookingResponseDto>.BadRequest("A booking must include at least one seat or cargo quantity.", nameof(requestDto));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var customerName = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.FirstName + " " + u.LastName)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (customerName is null)
                {
                    await transaction.RollbackAsync();
                    return Result<BookingResponseDto>.UnAuthorized("User was not found.");
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

                    if (trip.CargoFare is null)
                    {
                        await transaction.RollbackAsync();
                        return Result<BookingResponseDto>.Conflict("Cargo bookings are not available for this trip.");
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
                    UserId = userId,
                    TripId = requestDto.TripId,
                    NumberOfSeats = requestDto.NumberOfSeats,
                    CargoWeight = requestDto.CargoWeight,
                    SeatsFare = trip.SeatFare * requestDto.NumberOfSeats,
                    CargoFare = (trip.CargoFare ?? 0m) * requestDto.CargoWeight,
                    TotalFare = (trip.SeatFare * requestDto.NumberOfSeats) + ((trip.CargoFare ?? 0m) * requestDto.CargoWeight),
                    BookingReference = bookingReference,
                    BookingStatus = BookingStatus.Pending,
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

                var bookingDto = new BookingResponseDto
                {
                    BookingId = booking.Id,
                    CustomerName = customerName,
                    BookingStatus = booking.BookingStatus,
                    BookingReference = booking.BookingReference,
                    TripSummary = $"{trip.Route.Origin.Name} → {trip.Route.Destination.Name} on {trip.DepartureTime:ddd, dd MMM yyyy}",
                    NumberOfSeats = booking.NumberOfSeats,
                    CargoWeight = booking.CargoWeight,
                    SeatsFare = booking.SeatsFare,
                    CargoFare = booking.CargoFare,
                    TotalFare = booking.TotalFare
                };

                return Result<BookingResponseDto>.Created(bookingDto, "Booking created successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                // Trip.RowVersion changed after we read it: another booking took capacity first.
                await transaction.RollbackAsync();
                return Result<BookingResponseDto>.Conflict("This trip was just updated by another booking. Please check availability and try again.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // <summary>
        /// Retrieves a booking by its ID for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <returns>The booking if found, otherwise a not found result.</returns>
        public async Task<Result<BookingResponseDto>> GetBookingByIdAsync(Guid userId, Guid bookingId)
        {
            if (userId == Guid.Empty)
            {
                return Result<BookingResponseDto>.BadRequest("User ID is required.", nameof(userId));
            }

            if (bookingId == Guid.Empty)
            {
                return Result<BookingResponseDto>.BadRequest("Booking ID is required.", nameof(bookingId));
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return Result<BookingResponseDto>.UnAuthorized("User not found.");
            }

            var booking = await _context.Bookings
                .Where(b => b.Id == bookingId && b.UserId == userId).Select(b => new BookingResponseDto
                {
                    BookingId = b.Id,
                    CustomerName = b.User.FirstName + " " + b.User.LastName,
                    BookingReference = b.BookingReference,
                    BookingStatus = b.BookingStatus,
                    TripSummary = $"{b.Trip.Route.Origin.Name} → {b.Trip.Route.Destination.Name} on {b.Trip.DepartureTime:ddd, dd MMM yyyy}",
                    NumberOfSeats = b.NumberOfSeats,
                    CargoWeight = b.CargoWeight,
                    SeatsFare = b.SeatsFare,
                    CargoFare = b.CargoFare,
                    TotalFare = b.TotalFare
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (booking is null)
            {
                return Result<BookingResponseDto>.NotFound("Booking not found.");
            }

            return Result<BookingResponseDto>.Ok(booking, "Booking retrieved successfully.");
        }
    }
}
