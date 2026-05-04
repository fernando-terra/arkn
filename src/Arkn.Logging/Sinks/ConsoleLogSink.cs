using Arkn.Logging.Abstractions;
using Arkn.Logging.Models;

namespace Arkn.Logging.Sinks;

/// <summary>
/// Writes log entries to <see cref="Console"/> using ANSI escape codes for rich colorized output.
/// Falls back to plain output when not running in a TTY or when <see cref="UseAnsi"/> is false.
/// </summary>
public sealed class ConsoleLogSink : IArknLogSink
{
    // ANSI escape codes
    private const string AnsiReset   = "\x1b[0m";
    private const string AnsiGray    = "\x1b[90m";
    private const string AnsiCyan    = "\x1b[36m";
    private const string AnsiGreen   = "\x1b[32m";
    private const string AnsiYellow  = "\x1b[33m";
    private const string AnsiRed     = "\x1b[31m";
    private const string AnsiFatal   = "\x1b[1;35m";  // bold magenta
    private const string AnsiDim     = "\x1b[2m";

    private static readonly Dictionary<ArknLogLevel, string> _ansiColors = new()
    {
        [ArknLogLevel.Trace]   = AnsiGray,
        [ArknLogLevel.Debug]   = AnsiCyan,
        [ArknLogLevel.Info]    = AnsiGreen,
        [ArknLogLevel.Warning] = AnsiYellow,
        [ArknLogLevel.Error]   = AnsiRed,
        [ArknLogLevel.Fatal]   = AnsiFatal,
    };

    private static readonly Dictionary<ArknLogLevel, ConsoleColor> _fallbackColors = new()
    {
        [ArknLogLevel.Trace]   = ConsoleColor.Gray,
        [ArknLogLevel.Debug]   = ConsoleColor.Cyan,
        [ArknLogLevel.Info]    = ConsoleColor.Green,
        [ArknLogLevel.Warning] = ConsoleColor.Yellow,
        [ArknLogLevel.Error]   = ConsoleColor.Red,
        [ArknLogLevel.Fatal]   = ConsoleColor.Magenta,
    };

    private static readonly Dictionary<ArknLogLevel, string> _levelBadge = new()
    {
        [ArknLogLevel.Trace]   = "TRACE  ",
        [ArknLogLevel.Debug]   = "DEBUG  ",
        [ArknLogLevel.Info]    = "INFO   ",
        [ArknLogLevel.Warning] = "WARNING",
        [ArknLogLevel.Error]   = "ERROR  ",
        [ArknLogLevel.Fatal]   = "FATAL  ",
    };

    /// <summary>
    /// Whether to use ANSI escape codes for colorized output.
    /// Defaults to <c>true</c> but is automatically disabled when output is redirected
    /// (i.e., not a real TTY).
    /// </summary>
    public bool UseAnsi { get; }

    /// <summary>
    /// Creates a <see cref="ConsoleLogSink"/> with automatic TTY detection.
    /// ANSI output is enabled unless <see cref="Console.IsOutputRedirected"/> is <c>true</c>.
    /// </summary>
    public ConsoleLogSink() : this(useAnsi: !Console.IsOutputRedirected) { }

    /// <summary>
    /// Creates a <see cref="ConsoleLogSink"/> with explicit ANSI control.
    /// </summary>
    /// <param name="useAnsi">When <c>true</c>, ANSI escape codes are used; otherwise falls back to <see cref="Console.ForegroundColor"/>.</param>
    public ConsoleLogSink(bool useAnsi)
    {
        UseAnsi = useAnsi;
    }

    /// <inheritdoc />
    public void Write(LogEntry entry)
    {
        var badge  = _levelBadge.GetValueOrDefault(entry.Level, entry.Level.ToString().ToUpper().PadRight(7));
        var ts     = entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");

        if (UseAnsi)
        {
            WriteAnsi(entry, ts, badge);
        }
        else
        {
            WriteFallback(entry, ts, badge);
        }
    }

    private static void WriteAnsi(LogEntry entry, string ts, string badge)
    {
        var levelColor = _ansiColors.GetValueOrDefault(entry.Level, AnsiReset);

        // Build the main line
        var sourceHint = string.Empty;
        // Scope rendered with dim styling after the level badge
        var scopePart = entry.Scope is not null
            ? $"{AnsiDim}{entry.Scope}{AnsiReset} » "
            : string.Empty;

        Console.Write($"{ts} {levelColor}[{badge}]{AnsiReset} {scopePart}{entry.Message}");
        Console.WriteLine();

        if (entry.Exception is not null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  Exception: {entry.Exception}");
            Console.ResetColor();
        }

        if (entry.Context is { Count: > 0 })
        {
            foreach (var (key, value) in entry.Context)
                Console.WriteLine($"  {AnsiGray}{key}={value}{AnsiReset}");
        }
    }

    private static void WriteFallback(LogEntry entry, string ts, string badge)
    {
        var original = Console.ForegroundColor;
        Console.ForegroundColor = _fallbackColors.GetValueOrDefault(entry.Level, ConsoleColor.White);

        var scopePart = entry.Scope is not null ? $"{entry.Scope} » " : string.Empty;
        Console.WriteLine($"{ts} [{badge}] {scopePart}{entry.Message}");

        if (entry.Exception is not null)
            Console.WriteLine($"  Exception: {entry.Exception}");

        if (entry.Context is { Count: > 0 })
        {
            foreach (var (key, value) in entry.Context)
                Console.WriteLine($"  {key}={value}");
        }

        Console.ForegroundColor = original;
    }
}
