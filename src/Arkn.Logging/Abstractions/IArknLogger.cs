using Arkn.Logging.Models;

namespace Arkn.Logging.Abstractions;

/// <summary>
/// The primary Arkn logging interface.
/// Implementations dispatch <see cref="LogEntry"/> records to registered <see cref="IArknLogSink"/>s.
/// </summary>
public interface IArknLogger
{
    /// <summary>Logs an entry at the specified level.</summary>
    void Log(ArknLogLevel level, string message, IArknLogContext? context = null, Exception? exception = null);

    /// <summary>Logs a trace-level message.</summary>
    void Trace(string message, IArknLogContext? context = null);
    /// <summary>Logs a debug-level message.</summary>
    void Debug(string message, IArknLogContext? context = null);
    /// <summary>Logs an informational message.</summary>
    void Info(string message, IArknLogContext? context = null);
    /// <summary>Logs a warning message.</summary>
    void Warning(string message, IArknLogContext? context = null);
    /// <summary>Logs an error message with an optional exception.</summary>
    void Error(string message, Exception? exception = null, IArknLogContext? context = null);
    /// <summary>Logs a fatal error message with an optional exception.</summary>
    void Fatal(string message, Exception? exception = null, IArknLogContext? context = null);
}
