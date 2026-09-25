namespace BusBookingSystem.Application.Common
{
    public sealed class Result<TData>
    {
        public ResultStatus Status { get; init; }
        public string? Message { get; init; }
        public TData? Data { get; init; }
        public object? Errors { get; init; }

        public bool Success { get; set; }

        public static Result<TData> Ok(TData? data, string? message = null) =>
            new()
            {
                Success = true,
                Status = ResultStatus.Success,
                Message = message,
                Data = data
            };

        public static Result<TData> Created(TData? data, string? message = null) =>
            new()
            {
                Success = true,
                Status = ResultStatus.Created,
                Message = message,
                Data = data
            };

        public static Result<TData> NoContent(TData? data, string? message = null) =>
            new()
            {
                Success = true,
                Status = ResultStatus.NoContent,
                Message = message,
            };

        public static Result<TData> BadRequest(string message, object? errors = null) =>
            new()
            {
                Success = false,
                Status = ResultStatus.BadRequest,
                Message = message,
                Errors = errors
            };

        public static Result<TData> NotFound(string message) =>
            new()
            {
                Success = false,
                Status = ResultStatus.NotFound,
                Message = message
            };

        public static Result<TData> Conflict(string message, object? errors = null) =>
            new()
            {
                Success = false,
                Status = ResultStatus.Conflict,
                Message = message,
                Errors = errors
            };

        public static Result<TData> UnAuthorized(string message, object? errors = null) =>
            new()
            {
                Success = false,
                Status = ResultStatus.UnAuthorized,
                Message = message,
                Errors = errors
            };

        public static Result<TData> Forbidden(string message, object? errors = null) =>
            new()
            {
                Success = false,
                Status = ResultStatus.Forbidden,
                Message = message,
                Errors = errors
            };

        public static Result<TData> Unexpected(string message, object? errors = null) =>
            new()
            {
                Success = false,
                Status = ResultStatus.Unexpected,
                Message = message,
                Errors = errors
            };
    }
}
