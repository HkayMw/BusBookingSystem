

using BusBookingSystem.Application.Common;
using BusBookingSystem.Application.DTOs;

namespace BusBookingSystem.Application.Interfaces.Services
{
    public interface IBookingService
    {
        Task<Result<BookingResponseDto>> CreateBookingAsync(Guid userId, BookingRequestDto requestDto);
    }
}
