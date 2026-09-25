using BusBookingSystem.Application.Common;
using BusBookingSystem.API.Models;

namespace BusBookingSystem.API.Extensions
{
    public static class ResultExtensions
    {
        public static (int StatusCode, ApiResponse<TData> Body) ToApiResponse<TData>(this Result<TData> result)
        {
            var statusCode = result.Status switch
            {
                ResultStatus.Success => StatusCodes.Status200OK,
                ResultStatus.Created => StatusCodes.Status201Created,
                ResultStatus.NoContent => StatusCodes.Status204NoContent,
                ResultStatus.BadRequest => StatusCodes.Status400BadRequest,
                ResultStatus.NotFound => StatusCodes.Status404NotFound,
                ResultStatus.Conflict => StatusCodes.Status409Conflict,
                ResultStatus.UnAuthorized => StatusCodes.Status401Unauthorized,
                ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            return (statusCode, new ApiResponse<TData>
            {
                Success = result.Success,
                StatusCode = statusCode,
                Message = result.Message,
                Data = result.Data,
                Errors = result.Errors
            });
        }
    }
}
