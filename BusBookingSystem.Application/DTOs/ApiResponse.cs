

namespace BusBookingSystem.Application.DTOs
{
    public class ApiResponse<TData>
    {
        public bool Success { set; get; }
        public string? Message { set; get; }
        public TData? Data { set; get; }
        public object? Errors { set; get; }
        public DateTime Timestamp { set; get; } = DateTime.UtcNow;


        public static ApiResponse<TData> Create(bool success, string message, TData? data = default, object? errors = null)
        {
            return new ApiResponse<TData>
            {
                Success = success,
                Message = message,
                Data = data,
                Errors = errors
            };
        }

        public static ApiResponse<TData> Ok(string message, TData? data) =>
            Create(true, message, data);

        public static ApiResponse<TData> Created(string message, TData? data) =>
            Create(true, message, data);

        public static ApiResponse<TData> NoContent(string message) =>
            Create(true, message);

        public static ApiResponse<TData> NotFound(string message) =>
            Create(false, message);

        public static ApiResponse<TData> BadRequest(string message, object? errors = null) =>
            Create(false, message, errors: errors);

        public static ApiResponse<TData> Conflict(string message, object? errors = null) =>
            Create(false, message, errors: errors);

        public static ApiResponse<TData> Error(int statusCode, string message, object? errors = null) =>
            Create(false, message, errors: errors);
    }
}
