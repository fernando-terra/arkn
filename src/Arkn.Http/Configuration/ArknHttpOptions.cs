using System.Text.Json;
using System.Text.Json.Serialization;
using Arkn.Http.Auth;

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
}
