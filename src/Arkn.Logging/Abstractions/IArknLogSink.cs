using Arkn.Logging.Models;

namespace Arkn.Logging.Abstractions;

/// <summary>
/// A pluggable log destination. Implement this to send logs anywhere —
/// console, file, database, cloud, etc.
/// </summary>
public interface IArknLogSink
{
    /// <summary>Writes a log entry to this sink.</summary>
    void Write(LogEntry entry);
}
