using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Arkn.Logging.Abstractions;
using Arkn.Logging.Models;

namespace Arkn.Extensions.Logging.Seq;

/// <summary>
/// Arkn.Logging sink that forwards <see cref="LogEntry"/> records to Seq
/// using the CLEF (Compact Log Event Format) HTTP endpoint <c>/api/events/raw</c>.
/// Batches entries and flushes on a timer or when the batch is full.
/// Thread-safe.
/// </summary>
public sealed class SeqSink : IArknLogSink, IDisposable
{
    private readonly HttpClient        _http;
    private readonly SeqSinkOptions    _options;
    private readonly List<LogEntry>    _buffer = [];
    private readonly object _lock = new object();
    private readonly Timer             _timer;
    private readonly string            _endpoint;

    public SeqSink(SeqSinkOptions options) : this(new HttpClient(), options) { }

    public SeqSink(HttpClient http, SeqSinkOptions options)
    {
        _http    = http;
        _options = options;
        _http.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        _endpoint = $"{options.ServerUrl.TrimEnd('/')}/api/events/raw?clef";

        _timer = new Timer(
            _ => _ = FlushAsync(),
            null,
            TimeSpan.FromSeconds(options.FlushIntervalSeconds),
            TimeSpan.FromSeconds(options.FlushIntervalSeconds));
    }

    /// <inheritdoc />
    public void Write(LogEntry entry)
    {
        if (entry.Level < _options.MinimumLevel) return;

        List<LogEntry>? toFlush = null;

        lock (_lock)
        {
            _buffer.Add(entry);
            if (_buffer.Count >= _options.BatchSize)
            {
                toFlush = new List<LogEntry>(_buffer);
                _buffer.Clear();
            }
        }

        if (toFlush is not null)
            _ = SendAsync(toFlush);
    }

    private async Task FlushAsync()
    {
        List<LogEntry> toFlush;
        lock (_lock)
        {
            if (_buffer.Count == 0) return;
            toFlush = new List<LogEntry>(_buffer);
            _buffer.Clear();
        }
        await SendAsync(toFlush);
    }

    private async Task SendAsync(List<LogEntry> entries)
    {
        if (entries.Count == 0) return;

        try
        {
            // Build CLEF (JSON Lines) payload
            var sb = new StringBuilder();
            foreach (var entry in entries)
                sb.AppendLine(ToClef(entry));

            var content = new StringContent(sb.ToString(), Encoding.UTF8, "application/vnd.serilog.clef");

            if (_options.ApiKey is not null)
                _http.DefaultRequestHeaders.Remove("X-Seq-ApiKey"); // idempotent
            var req = new HttpRequestMessage(HttpMethod.Post, _endpoint)
            {
                Content = content
            };
            if (_options.ApiKey is not null)
                req.Headers.Add("X-Seq-ApiKey", _options.ApiKey);

            using var response = await _http.SendAsync(req);
            // Seq returns 201 on success — ignore non-fatal errors silently
        }
        catch
        {
            // Sink must never crash the host
        }
    }

    private static string ToClef(LogEntry entry)
    {
        // CLEF fields: @t=timestamp, @mt=message template, @l=level, @x=exception, rest=properties
        var obj = new Dictionary<string, object?>
        {
            ["@t"]  = entry.Timestamp.ToString("O"),
            ["@mt"] = entry.Message,
            ["@l"]  = MapLevel(entry.Level),
        };

        if (entry.Exception is not null)
            obj["@x"] = entry.Exception.ToString();

        if (entry.Scope is not null)
            obj["Scope"] = entry.Scope;

        if (entry.Context is not null)
            foreach (var (k, v) in entry.Context)
                obj[k] = v;

        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        });
    }

    private static string MapLevel(ArknLogLevel level) => level switch
    {
        ArknLogLevel.Trace   => "Verbose",
        ArknLogLevel.Debug   => "Debug",
        ArknLogLevel.Info    => "Information",
        ArknLogLevel.Warning => "Warning",
        ArknLogLevel.Error   => "Error",
        ArknLogLevel.Fatal   => "Fatal",
        _                    => "Information",
    };

    public void Dispose()
    {
        _timer.Dispose();
        FlushAsync().GetAwaiter().GetResult();
        _http.Dispose();
    }
}
