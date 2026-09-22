namespace BusBookingSystem.Application.Exceptions
{
    public sealed class BookingConflictException(string message) : Exception(message)
    {
    }
}