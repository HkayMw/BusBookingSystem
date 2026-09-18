

using BusBookingSystem.Application.DTOs;

namespace BusBookingSystem.Application.Interfaces.Services
{
    public interface IBookingService
    {
        Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto requestDto);
    }
}
