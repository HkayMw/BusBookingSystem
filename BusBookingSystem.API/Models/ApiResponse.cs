namespace BusBookingSystem.API.Models
{
    public sealed class ApiResponse<TData>
    {
        public bool Success { get; init; }
        public int StatusCode { get; init; }
        public string? Message { get; init; }
        public TData? Data { get; init; }
        public object? Errors { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }
}
