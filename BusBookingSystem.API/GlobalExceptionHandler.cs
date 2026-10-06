using BusBookingSystem.API.Models;
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

            // Expected failures are returned as Result<T> by services, so anything reaching here is a server error.
            // Never echo exception.Message: it can contain internal details. The full exception is logged above.
            const int statusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(
                new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = statusCode,
                    Message = "An unexpected error occurred."
                },
                cancellationToken);

            return true;
        }
    }
}