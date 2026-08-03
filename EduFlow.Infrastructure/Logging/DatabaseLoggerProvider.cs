using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace EduFlow.Infrastructure.Logging;

/// <summary>
/// Feeds every application log line into <see cref="AppLog"/> rows via a bounded channel, so a
/// burst of logging can never block the request that produced it (writes are dropped, not
/// queued indefinitely, once the channel is full). <see cref="DatabaseLogFlusherService"/> drains
/// the channel in the background and batches the actual inserts into <see cref="LoggingDbContext"/>.
/// </summary>
public sealed class DatabaseLoggerProvider(Channel<AppLog> channel) : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, DatabaseLogger> _loggers = new();

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new DatabaseLogger(name, channel.Writer));

    public void Dispose() => _loggers.Clear();
}

internal sealed class DatabaseLogger(string categoryName, ChannelWriter<AppLog> writer) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) =>
        logLevel >= LogLevel.Information
        && !categoryName.StartsWith("Microsoft.", StringComparison.Ordinal)
        && !categoryName.StartsWith("System.", StringComparison.Ordinal);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var entry = new AppLog
        {
            Timestamp = DateTime.UtcNow,
            Level = logLevel.ToString(),
            Category = categoryName,
            Message = formatter(state, exception),
            Exception = exception?.ToString()
        };

        // Best-effort: if the channel is full we drop the line rather than back-pressure the app.
        writer.TryWrite(entry);
    }
}
