using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Arkn.Logging.Abstractions;
using Arkn.Logging.Models;

namespace Arkn.Extensions.Logging.Elasticsearch;

/// <summary>
/// Arkn.Logging sink that forwards <see cref="LogEntry"/> records to Elasticsearch
/// via the Bulk API (<c>/_bulk</c>). No NEST or official SDK required.
/// Batches entries and flushes on a timer or when the batch is full.
/// </summary>
public sealed class ElasticsearchSink : IArknLogSink, IDisposable
{
    private readonly HttpClient                 _http;
    private readonly ElasticsearchSinkOptions   _options;
    private readonly List<LogEntry>             _buffer = [];
    private readonly Lock                       _lock   = new();
    private readonly Timer                      _timer;

    public ElasticsearchSink(ElasticsearchSinkOptions options) : this(new HttpClient(), options) { }

    public ElasticsearchSink(HttpClient http, ElasticsearchSinkOptions options)
    {
        _http    = http;
        _options = options;
        _http.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

        ConfigureAuth();

        _timer = new Timer(
            _ => _ = FlushAsync(),
            null,
            TimeSpan.FromSeconds(options.FlushIntervalSeconds),
            TimeSpan.FromSeconds(options.FlushIntervalSeconds));
    }

    private void ConfigureAuth()
    {
        if (_options.ApiKey is not null)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("ApiKey", _options.ApiKey);
        }
        else if (_options.Username is not null && _options.Password is not null)
        {
            var creds = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_options.Username}:{_options.Password}"));
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", creds);
        }
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
            var sb = new StringBuilder();
            foreach (var entry in entries)
            {
                var index = ResolveIndex(entry.Timestamp);
                // Bulk action line
                sb.AppendLine(JsonSerializer.Serialize(new { index = new { _index = index } }));
                // Document line
                sb.AppendLine(ToDocument(entry));
            }

            var url     = $"{_options.NodeUrl.TrimEnd('/')}/_bulk";
            var content = new StringContent(sb.ToString(), Encoding.UTF8, "application/x-ndjson");
            using var response = await _http.PostAsync(url, content);
            // Elasticsearch Bulk API returns 200 with per-item results — ignore on failure
        }
        catch
        {
            // Sink must never crash the host
        }
    }

    private string ResolveIndex(DateTimeOffset ts)
    {
        return _options.IndexPattern
            .Replace("{yyyy-MM-dd}", ts.UtcDateTime.ToString("yyyy-MM-dd"))
            .Replace("{yyyy-MM}",    ts.UtcDateTime.ToString("yyyy-MM"))
            .Replace("{yyyy}",       ts.UtcDateTime.ToString("yyyy"));
    }

    private static string ToDocument(LogEntry entry)
    {
        var doc = new Dictionary<string, object?>
        {
            ["@timestamp"] = entry.Timestamp.ToString("O"),
            ["level"]      = entry.Level.ToString(),
            ["message"]    = entry.Message,
        };

        if (entry.Scope     is not null) doc["scope"]     = entry.Scope;
        if (entry.Exception is not null) doc["exception"] = entry.Exception.ToString();

        if (entry.Context is not null)
            foreach (var (k, v) in entry.Context)
                doc[k] = v;

        return JsonSerializer.Serialize(doc, new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        });
    }

    public void Dispose()
    {
        _timer.Dispose();
        FlushAsync().GetAwaiter().GetResult();
        _http.Dispose();
    }
}
