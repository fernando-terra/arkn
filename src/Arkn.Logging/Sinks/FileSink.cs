using Arkn.Logging.Abstractions;
using Arkn.Logging.Models;

namespace Arkn.Logging.Sinks;

/// <summary>
/// Appends log entries to a plain-text file.
/// Thread-safe. No rotation — suitable for basic production use.
/// </summary>
public sealed class FileSink : IArknLogSink, IDisposable
{
    private readonly StreamWriter _writer;
    private readonly object _lock = new();

    /// <param name="filePath">Path to the log file. Created if it does not exist.</param>
    public FileSink(string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath)) ?? ".");
        _writer = new StreamWriter(filePath, append: true, encoding: System.Text.Encoding.UTF8)
        {
            AutoFlush = true,
        };
    }

    /// <inheritdoc />
    public void Write(LogEntry entry)
    {
        lock (_lock)
        {
            var scope = entry.Scope is not null ? $"[{entry.Scope}] " : string.Empty;
            _writer.WriteLine($"{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{entry.Level,-7}] {scope}{entry.Message}");

            if (entry.Exception is not null)
                _writer.WriteLine($"  Exception: {entry.Exception}");

            if (entry.Context is { Count: > 0 })
            {
                foreach (var (key, value) in entry.Context)
                    _writer.WriteLine($"  {key} = {value}");
            }
        }
    }

    public void Dispose() => _writer.Dispose();
}
