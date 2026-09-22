namespace BusBookingSystem.Application.Common
{
    public sealed class Result<TData>
    {
        public ResultStatus Status { get; init; }
        public string? Message { get; init; }
        public TData? Data { get; init; }
        public object? Errors { get; init; }

        public bool Success => Status == ResultStatus.Success;

        public static Result<TData> Ok(TData? data, string? message = null) =>
            new()
            {
                Status = ResultStatus.Success,
                Message = message,
                Data = data
            };

        public static Result<TData> Invalid(string message, object? errors = null) =>
            new()
            {
                Status = ResultStatus.ValidationError,
                Message = message,
                Errors = errors
            };

        public static Result<TData> NotFound(string message) =>
            new()
            {
                Status = ResultStatus.NotFound,
                Message = message
            };

        public static Result<TData> Conflict(string message, object? errors = null) =>
            new()
            {
                Status = ResultStatus.Conflict,
                Message = message,
                Errors = errors
            };

        public static Result<TData> Unexpected(string message, object? errors = null) =>
            new()
            {
                Status = ResultStatus.Unexpected,
                Message = message,
                Errors = errors
            };
    }
}
