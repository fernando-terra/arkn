using Arkn.Logging.Abstractions;
using Arkn.Logging.Models;

namespace Arkn.Logging.Sinks;

/// <summary>
/// Writes log entries to <see cref="Console"/>.
/// Intended for development and debugging.
/// Colors entries by severity level.
/// </summary>
public sealed class ConsoleLogSink : IArknLogSink
{
    private static readonly Dictionary<ArknLogLevel, ConsoleColor> _colors = new()
    {
        [ArknLogLevel.Trace]   = ConsoleColor.Gray,
        [ArknLogLevel.Debug]   = ConsoleColor.Cyan,
        [ArknLogLevel.Info]    = ConsoleColor.Green,
        [ArknLogLevel.Warning] = ConsoleColor.Yellow,
        [ArknLogLevel.Error]   = ConsoleColor.Red,
        [ArknLogLevel.Fatal]   = ConsoleColor.Magenta,
    };

    /// <inheritdoc />
    public void Write(LogEntry entry)
    {
        var original = Console.ForegroundColor;
        Console.ForegroundColor = _colors.GetValueOrDefault(entry.Level, ConsoleColor.White);

        var scope = entry.Scope is not null ? $"[{entry.Scope}] " : string.Empty;
        Console.WriteLine($"{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{entry.Level,-7}] {scope}{entry.Message}");

        if (entry.Exception is not null)
            Console.WriteLine($"  Exception: {entry.Exception}");

        if (entry.Context is { Count: > 0 })
        {
            foreach (var (key, value) in entry.Context)
                Console.WriteLine($"  {key} = {value}");
        }

        Console.ForegroundColor = original;
    }
}
