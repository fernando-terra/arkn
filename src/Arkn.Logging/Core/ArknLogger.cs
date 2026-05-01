using Arkn.Logging.Abstractions;
using Arkn.Logging.Models;

namespace Arkn.Logging.Core;

/// <summary>
/// Default <see cref="IArknLogger"/> implementation.
/// Dispatches every <see cref="LogEntry"/> to all registered <see cref="IArknLogSink"/>s.
/// Thread-safe: sinks are written to sequentially under a lock.
/// </summary>
public sealed class ArknLogger : IArknLogger
{
    private readonly IReadOnlyList<IArknLogSink> _sinks;
    private readonly ArknLogLevel _minimumLevel;
    private readonly object _lock = new();

    public ArknLogger(IEnumerable<IArknLogSink> sinks, ArknLogLevel minimumLevel = ArknLogLevel.Trace)
    {
        _sinks = sinks.ToList().AsReadOnly();
        _minimumLevel = minimumLevel;
    }

    /// <inheritdoc />
    public void Log(ArknLogLevel level, string message, IArknLogContext? context = null, Exception? exception = null)
    {
        if (level < _minimumLevel) return;

        var entry = new LogEntry(
            level,
            message,
            DateTimeOffset.UtcNow,
            context?.Scope,
            context?.Properties,
            exception);

        lock (_lock)
        {
            foreach (var sink in _sinks)
            {
                try { sink.Write(entry); }
                catch { /* sinks must never crash the host */ }
            }
        }
    }

    public void Trace(string message, IArknLogContext? context = null) =>
        Log(ArknLogLevel.Trace, message, context);

    public void Debug(string message, IArknLogContext? context = null) =>
        Log(ArknLogLevel.Debug, message, context);

    public void Info(string message, IArknLogContext? context = null) =>
        Log(ArknLogLevel.Info, message, context);

    public void Warning(string message, IArknLogContext? context = null) =>
        Log(ArknLogLevel.Warning, message, context);

    public void Error(string message, Exception? exception = null, IArknLogContext? context = null) =>
        Log(ArknLogLevel.Error, message, context, exception);

    public void Fatal(string message, Exception? exception = null, IArknLogContext? context = null) =>
        Log(ArknLogLevel.Fatal, message, context, exception);
}
