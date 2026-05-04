using System.Text.Json;
using System.Text.Json.Serialization;
using Arkn.Http.Auth;
using Arkn.Logging.Abstractions;
using Arkn.Logging.Models;

namespace Arkn.Http.Configuration;

/// <summary>
/// Configuration options for an Arkn HTTP client instance.
/// </summary>
public sealed class ArknHttpOptions
{
    /// <summary>
    /// Optional base URL prepended to all relative request paths.
    /// E.g. <c>"https://api.example.com"</c>.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Default request timeout. Overridable per-request with <c>.WithTimeout()</c>.
    /// Default: 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum number of total attempts (initial attempt + retries).
    /// Set to 1 to disable retry (default).
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 1;

    /// <summary>
    /// Base delay for exponential backoff.
    /// Actual delay per attempt: <c>BaseRetryDelay × 2^attempt + jitter</c>.
    /// Default: 200 ms.
    /// </summary>
    public TimeSpan BaseRetryDelay { get; set; } = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// JSON serialization options used for request body serialization and response deserialization.
    /// Default: camelCase property naming, nulls ignored.
    /// </summary>
    public JsonSerializerOptions JsonOptions { get; set; } = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Auth interceptors applied before every request. Add via <c>.WithInterceptor()</c>,
    /// <c>.WithBearerAuth()</c>, or <c>.WithClientCredentials()</c> on the builder.
    /// </summary>
    public List<IArknAuthInterceptor> Interceptors { get; } = new();

    /// <summary>
    /// When <c>true</c>, logs full request and response (including payloads) for every call.
    /// Enable via <c>.WithDebugLogging()</c> on the builder.
    /// </summary>
    public bool DebugLoggingEnabled { get; set; }

    /// <summary>
    /// Severity level used for debug log entries. Default: <see cref="ArknLogLevel.Debug"/>.
    /// </summary>
    public ArknLogLevel DebugLogLevel { get; set; } = ArknLogLevel.Debug;

    /// <summary>
    /// Logger instance used for debug output. Resolved from DI when <see cref="DebugLoggingEnabled"/> is <c>true</c>.
    /// Headers with sensitive content (Authorization, Cookie) are automatically sanitized.
    /// </summary>
    public IArknLogger? DebugLogger { get; set; }
}
