using System.Text.Json;
using System.Text.Json.Serialization;
using Arkn.Http.Auth;
using Arkn.Http.Cache;
using Arkn.Http.Resilience;
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
    /// When non-null, debug logging is enabled with the specified options.
    /// Set via <c>.WithDebugLogging()</c> on the builder.
    /// </summary>
    public DebugLoggingOptions? DebugOptions { get; set; }

    /// <summary>
    /// Logger resolved from DI when <see cref="DebugOptions"/> is set.
    /// Headers with sensitive content (Authorization, Cookie) are sanitized automatically.
    /// </summary>
    public IArknLogger? DebugLogger { get; set; }

    /// <summary>When set, handles 429 responses by waiting and retrying automatically.</summary>
    public RateLimitOptions? RateLimitOptions { get; set; }

    /// <summary>When set, caches responses in memory according to these options.</summary>
    public ResponseCacheOptions? ResponseCacheOptions { get; set; }

    /// <summary>The in-memory cache instance created alongside <see cref="ResponseCacheOptions"/>.</summary>
    internal InMemoryResponseCache? ResponseCache { get; set; }
}
