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

    void Trace(string message, IArknLogContext? context = null);
    void Debug(string message, IArknLogContext? context = null);
    void Info(string message, IArknLogContext? context = null);
    void Warning(string message, IArknLogContext? context = null);
    void Error(string message, Exception? exception = null, IArknLogContext? context = null);
    void Fatal(string message, Exception? exception = null, IArknLogContext? context = null);
}
