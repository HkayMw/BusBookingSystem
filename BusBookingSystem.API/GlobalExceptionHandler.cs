using BusBookingSystem.API.Models;
using BusBookingSystem.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace BusBookingSystem.API
{
    public class GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Unhandled exception occurred");

            var statusCode = exception switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                ResourceNotFoundException => StatusCodes.Status404NotFound,
                BookingConflictException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            httpContext.Response.StatusCode = statusCode;

            var message = statusCode switch
            {
                StatusCodes.Status400BadRequest => exception.Message,
                StatusCodes.Status404NotFound => exception.Message,
                StatusCodes.Status409Conflict => exception.Message,
                _ => "An unexpected error occurred."
            };

            await httpContext.Response.WriteAsJsonAsync(
                new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = statusCode,
                    Message = message
                },
                cancellationToken);

            return true;
        }
    }
}