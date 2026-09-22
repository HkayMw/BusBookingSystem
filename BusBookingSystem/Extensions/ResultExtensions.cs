using BusBookingSystem.Application.Common;
using BusBookingSystem.API.Models;
using Microsoft.AspNetCore.Http;

namespace BusBookingSystem.API.Extensions
{
    public static class ResultExtensions
    {
        public static (int StatusCode, ApiResponse<TData> Body) ToApiResponse<TData>(this Result<TData> result)
        {
            var statusCode = result.Status switch
            {
                ResultStatus.Success => StatusCodes.Status200OK,
                ResultStatus.ValidationError => StatusCodes.Status400BadRequest,
                ResultStatus.NotFound => StatusCodes.Status404NotFound,
                ResultStatus.Conflict => StatusCodes.Status409Conflict,
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
