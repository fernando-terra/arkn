using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Arkn.Http.Auth;
using Arkn.Http.Configuration;
using Arkn.Logging.Models;

namespace Arkn.Http.Builder;

/// <summary>
/// Fluent builder for constructing an HTTP request.
/// Returned by <see cref="Abstractions.IArknHttp.Request"/>.
/// </summary>
public sealed class ArknRequestBuilder : IArknRequestBuilder
{
    private readonly HttpClient _httpClient;
    private readonly string _path;
    private readonly ArknHttpOptions _options;
    private readonly Dictionary<string, string> _headers = new(StringComparer.OrdinalIgnoreCase);

    private TimeSpan? _timeout;
    private object? _body;

    internal ArknRequestBuilder(HttpClient httpClient, string path, ArknHttpOptions options)
    {
        _httpClient = httpClient;
        _path       = path;
        _options    = options;
    }

    /// <inheritdoc />
    public IArknRequestBuilder WithHeader(string name, string value)
    {
        _headers[name] = value;
        return this;
    }

    /// <inheritdoc />
    public IArknRequestBuilder WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    /// <inheritdoc />
    public IArknRequestBuilder WithBody(object body)
    {
        _body = body;
        return this;
    }

    /// <inheritdoc />
    public ArknHttpResponseHandler Get()    => CreateHandler(HttpMethod.Get);

    /// <inheritdoc />
    public ArknHttpResponseHandler Post()   => CreateHandler(HttpMethod.Post);

    /// <inheritdoc />
    public ArknHttpResponseHandler Put()    => CreateHandler(HttpMethod.Put);

    /// <inheritdoc />
    public ArknHttpResponseHandler Patch()  => CreateHandler(HttpMethod.Patch);

    /// <inheritdoc />
    public ArknHttpResponseHandler Delete() => CreateHandler(HttpMethod.Delete);

    // ── Internals ──────────────────────────────────────────────────────────────

    private ArknHttpResponseHandler CreateHandler(HttpMethod method)
    {
        // Capture mutable state at builder time so the lambda is self-contained
        var path         = _path;
        var headers      = new Dictionary<string, string>(_headers, StringComparer.OrdinalIgnoreCase);
        var body         = _body;
        var timeout      = _timeout ?? _options.Timeout;
        var jsonOptions  = _options.JsonOptions;
        var maxRetries   = _options.MaxRetryAttempts;
        var retryDelay   = _options.BaseRetryDelay;
        var httpClient   = _httpClient;
        var interceptors = _options.Interceptors.ToList();
        var debugLogger  = _options.DebugLogger;
        var debugLevel   = _options.DebugLogLevel;

        return new ArknHttpResponseHandler(
            execute: () => ArknRetryPolicy.ExecuteAsync(
                sendAsync: BuildAndSend,
                maxAttempts: maxRetries,
                baseDelay: retryDelay),
            jsonOptions: jsonOptions);

        async Task<HttpResponseMessage> BuildAndSend()
        {
            using var cts = new CancellationTokenSource(timeout);
            var request   = BuildRequest(method, path, headers, body, jsonOptions);

            foreach (var interceptor in interceptors)
                await interceptor.ApplyAsync(request, cts.Token).ConfigureAwait(false);

            if (debugLogger is not null)
                await LogRequestAsync(debugLogger, debugLevel, request, body, jsonOptions).ConfigureAwait(false);

            var sw       = debugLogger is not null ? Stopwatch.StartNew() : null;
            var response = await httpClient.SendAsync(request, cts.Token).ConfigureAwait(false);

            if (debugLogger is not null)
                await LogResponseAsync(debugLogger, debugLevel, response, sw!.ElapsedMilliseconds).ConfigureAwait(false);

            return response;
        }
    }

    private static HttpRequestMessage BuildRequest(
        HttpMethod method,
        string path,
        Dictionary<string, string> headers,
        object? body,
        JsonSerializerOptions jsonOptions)
    {
        var request = new HttpRequestMessage(method, path);

        foreach (var (name, value) in headers)
            request.Headers.TryAddWithoutValidation(name, value);

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, body.GetType(), jsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return request;
    }

    /// <summary>
    /// Replaces named placeholders (<c>{name}</c>, <c>{id}</c>) in <paramref name="path"/>
    /// with <paramref name="args"/> positionally.
    /// </summary>
    internal static string FormatPath(string path, object[] args)
    {
        if (args.Length == 0) return path;
        var index = 0;
        return Regex.Replace(path, @"\{[^}]+\}", _ =>
            index < args.Length
                ? Uri.EscapeDataString(args[index++]?.ToString() ?? string.Empty)
                : string.Empty);
    }

    // ── Debug logging ──────────────────────────────────────────────────────────

    private static readonly HashSet<string> _sensitiveHeaders =
        new(StringComparer.OrdinalIgnoreCase) { "Authorization", "Cookie", "Set-Cookie", "X-Api-Key" };

    private static async Task LogRequestAsync(
        Arkn.Logging.Abstractions.IArknLogger logger,
        ArknLogLevel level,
        HttpRequestMessage request,
        object? body,
        JsonSerializerOptions jsonOptions)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"→ {request.Method.Method} {request.RequestUri}");

        foreach (var (name, values) in request.Headers)
        {
            var value = _sensitiveHeaders.Contains(name)
                ? Sanitize(string.Join(",", values))
                : string.Join(",", values);
            sb.AppendLine($"  {name}: {value}");
        }

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, body.GetType(), jsonOptions);
            sb.AppendLine($"  Body: {json}");
        }

        if (request.Content is not null && body is null)
        {
            var raw = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(raw))
                sb.AppendLine($"  Body: {raw}");
        }

        logger.Debug(sb.ToString().TrimEnd());
    }

    private static async Task LogResponseAsync(
        Arkn.Logging.Abstractions.IArknLogger logger,
        ArknLogLevel level,
        HttpResponseMessage response,
        long elapsedMs)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"← {(int)response.StatusCode} {response.ReasonPhrase} ({elapsedMs}ms)");

        foreach (var (name, values) in response.Headers)
            sb.AppendLine($"  {name}: {string.Join(",", values)}");

        if (response.Content is not null)
        {
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(body))
            {
                // Pretty-print JSON if possible
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var pretty   = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
                    sb.AppendLine($"  Body: {pretty}");
                }
                catch
                {
                    sb.AppendLine($"  Body: {body}");
                }
            }
        }

        // Use the right level based on status
        if ((int)response.StatusCode >= 500)
            logger.Error(sb.ToString().TrimEnd());
        else if ((int)response.StatusCode >= 400)
            logger.Warning(sb.ToString().TrimEnd());
        else
            logger.Debug(sb.ToString().TrimEnd());
    }

    private static string Sanitize(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        // Keep first 8 chars, mask the rest
        var prefix = value.Length > 8 ? value[..8] : value;
        return $"{prefix}***";
    }
}
