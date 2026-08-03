namespace EduFlow.Infrastructure.Logging;

/// <summary>
/// A general application log line (as opposed to <see cref="AuditLog"/>, which tracks data changes).
/// Written by <see cref="DatabaseLoggerProvider"/> through a batching background flusher so the
/// logging pipeline never blocks the request that triggered it.
/// </summary>
public sealed class AppLog
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public required string Level { get; set; }
    public required string Category { get; set; }
    public required string Message { get; set; }
    public string? Exception { get; set; }
}
