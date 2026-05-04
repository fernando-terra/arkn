using System.Text;
using System.Text.Json;
using Arkn.Logging.Abstractions;
using Arkn.Logging.Models;

namespace Arkn.Logging.Sinks;

/// <summary>
/// Options for configuring the <see cref="FileSink"/> with daily rotation and optional JSON output.
/// </summary>
public record FileSinkOptions
{
    /// <summary>Directory where log files are written. Defaults to <c>logs</c>.</summary>
    public string Directory { get; init; } = "logs";

    /// <summary>
    /// File name pattern. Use <c>{date}</c> as a placeholder for the current date (yyyy-MM-dd).
    /// Defaults to <c>app-{date}.log</c>.
    /// </summary>
    public string FileNamePattern { get; init; } = "app-{date}.log";

    /// <summary>
    /// When <c>true</c>, each log entry is written as a JSON line (JSON Lines format).
    /// Defaults to <c>false</c> (plain text).
    /// </summary>
    public bool UseJsonFormat { get; init; } = false;

    /// <summary>
    /// Maximum file size in bytes before the file is closed and a new one is opened.
    /// Defaults to 10 MB.
    /// </summary>
    public long MaxFileSizeBytes { get; init; } = 10 * 1024 * 1024;
}

/// <summary>
/// Appends log entries to a file. Supports daily rotation and JSON Lines output format.
/// Thread-safe.
/// </summary>
public sealed class FileSink : IArknLogSink, IDisposable
{
    private readonly object _lock = new();

    // Options-based fields
    private readonly FileSinkOptions? _options;

    // Legacy single-file fields
    private readonly StreamWriter? _legacyWriter;

    // Rotation state
    private string? _currentDate;
    private StreamWriter? _rotatingWriter;

    /// <summary>
    /// Creates a <see cref="FileSink"/> that appends to a single fixed file path.
    /// Backward-compatible constructor; no rotation occurs.
    /// </summary>
    /// <param name="filePath">Path to the log file. Created if it does not exist.</param>
    public FileSink(string filePath)
    {
        System.IO.Directory.CreateDirectory(
            Path.GetDirectoryName(Path.GetFullPath(filePath)) ?? ".");

        _legacyWriter = new StreamWriter(filePath, append: true, encoding: Encoding.UTF8)
        {
            AutoFlush = true,
        };
    }

    /// <summary>
    /// Creates a <see cref="FileSink"/> with daily rotation and optional JSON format.
    /// </summary>
    /// <param name="options">Rotation and format options.</param>
    public FileSink(FileSinkOptions options)
    {
        _options = options;
        System.IO.Directory.CreateDirectory(options.Directory);
    }

    /// <inheritdoc />
    public void Write(LogEntry entry)
    {
        if (_legacyWriter is not null)
        {
            lock (_lock)
            {
                WritePlainText(_legacyWriter, entry);
            }
            return;
        }

        // Options-based rotating path
        lock (_lock)
        {
            EnsureRotatedWriter(entry.Timestamp);
            var writer = _rotatingWriter!;

            if (_options!.UseJsonFormat)
                WriteJson(writer, entry);
            else
                WritePlainText(writer, entry);
        }
    }

    private void EnsureRotatedWriter(DateTimeOffset timestamp)
    {
        var today = timestamp.UtcDateTime.ToString("yyyy-MM-dd");

        if (_rotatingWriter is not null && _currentDate == today)
        {
            // Check size limit
            try
            {
                var filePath = BuildFilePath(today);
                if (new FileInfo(filePath).Length < _options!.MaxFileSizeBytes)
                    return;
            }
            catch
            {
                return; // If we can't stat the file, keep writing to current writer
            }
        }

        // Date changed or size exceeded — rotate
        _rotatingWriter?.Dispose();
        _currentDate = today;

        var path = BuildFilePath(today);
        System.IO.Directory.CreateDirectory(_options!.Directory);
        _rotatingWriter = new StreamWriter(path, append: true, encoding: Encoding.UTF8)
        {
            AutoFlush = true,
        };
    }

    private string BuildFilePath(string date)
    {
        var fileName = _options!.FileNamePattern.Replace("{date}", date);
        return Path.Combine(_options.Directory, fileName);
    }

    private static void WritePlainText(StreamWriter writer, LogEntry entry)
    {
        var scope = entry.Scope is not null ? $"[{entry.Scope}] " : string.Empty;
        writer.WriteLine($"{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{entry.Level,-7}] {scope}{entry.Message}");

        if (entry.Exception is not null)
            writer.WriteLine($"  Exception: {entry.Exception}");

        if (entry.Context is { Count: > 0 })
        {
            foreach (var (key, value) in entry.Context)
                writer.WriteLine($"  {key} = {value}");
        }
    }

    private static void WriteJson(StreamWriter writer, LogEntry entry)
    {
        // Build a minimal JSON line to avoid System.Text.Json serializer overhead with anonymous types
        var contextObj = entry.Context is { Count: > 0 }
            ? entry.Context
            : null;

        var doc = new
        {
            timestamp = entry.Timestamp.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            level = entry.Level.ToString(),
            message = entry.Message,
            scope = entry.Scope,
            context = contextObj,
            exception = entry.Exception?.ToString(),
        };

        writer.WriteLine(JsonSerializer.Serialize(doc, JsonOptions));
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    /// <inheritdoc />
    public void Dispose()
    {
        _legacyWriter?.Dispose();
        _rotatingWriter?.Dispose();
    }
}
