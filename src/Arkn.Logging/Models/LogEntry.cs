namespace Arkn.Logging.Models;

/// <summary>
/// An immutable, strongly-typed log record.
/// </summary>
/// <param name="Level">Severity level.</param>
/// <param name="Message">Human-readable message.</param>
/// <param name="Timestamp">When this entry was created (UTC).</param>
/// <param name="Scope">Optional named scope, e.g. a Job RunId.</param>
/// <param name="Context">Structured key-value pairs attached to this entry.</param>
/// <param name="Exception">Exception associated with this entry, if any.</param>
public sealed record LogEntry(
    ArknLogLevel Level,
    string Message,
    DateTimeOffset Timestamp,
    string? Scope,
    IReadOnlyDictionary<string, object?>? Context,
    Exception? Exception)
{
    /// <summary>Creates a minimal log entry with only level, message and timestamp.</summary>
    public static LogEntry Create(ArknLogLevel level, string message) =>
        new(level, message, DateTimeOffset.UtcNow, null, null, null);
}
