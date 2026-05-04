using Arkn.Http.Auth;
using Arkn.Http.Cache;
using Arkn.Http.Configuration;
using Arkn.Http.Resilience;

namespace Arkn.Http.Abstractions;

/// <summary>
/// Fluent builder returned by <c>AddArknHttp&lt;TClient&gt;()</c> for configuring
/// retry and timeout policies on a typed HTTP client.
/// </summary>
public interface IArknHttpBuilder
{
    /// <summary>
    /// Enables exponential-backoff retry with jitter.
    /// Retries on <see cref="HttpRequestException"/>, timeouts, and 5xx responses.
    /// Does NOT retry on 4xx (client errors).
    /// </summary>
    /// <param name="maxAttempts">Total number of attempts (initial + retries).</param>
    /// <param name="baseDelay">Base delay for exponential backoff. Actual delay = baseDelay × 2^attempt + jitter.</param>
    IArknHttpBuilder WithRetry(int maxAttempts, TimeSpan baseDelay);

    /// <summary>
    /// Sets a default request timeout for all requests made by this client.
    /// Can be overridden per-request with <c>.WithTimeout()</c>.
    /// </summary>
    IArknHttpBuilder WithTimeout(TimeSpan timeout);

    /// <summary>Adds a custom auth interceptor that runs before every request.</summary>
    IArknHttpBuilder WithInterceptor(IArknAuthInterceptor interceptor);

    /// <summary>
    /// Adds a bearer-token interceptor backed by <paramref name="tokenFactory"/>.
    /// Tokens are cached in an <see cref="InMemoryTokenStore"/> for 55 minutes.
    /// </summary>
    IArknHttpBuilder WithBearerAuth(Func<Task<string>> tokenFactory);

    /// <summary>
    /// Adds a bearer-token interceptor backed by a service-provider-aware <paramref name="tokenFactory"/>.
    /// Tokens are cached in an <see cref="InMemoryTokenStore"/> for 55 minutes.
    /// </summary>
    IArknHttpBuilder WithBearerAuth(Func<IServiceProvider, Task<string>> tokenFactory);

    /// <summary>
    /// Adds an OAuth2 Client Credentials interceptor. Fetches and caches tokens automatically.
    /// </summary>
    IArknHttpBuilder WithClientCredentials(Action<ClientCredentialsOptions> configure);

    /// <summary>
    /// Enables debug logging with default options (<see cref="DebugLoggingOptions.Development"/>).
    /// Uses the <c>IArknLogger</c> registered in DI — flows to all configured sinks including AppInsights.
    /// </summary>
    IArknHttpBuilder WithDebugLogging();

    /// <summary>
    /// Enables debug logging with fine-grained control over levels and body capture.
    /// </summary>
    /// <example>
    /// // Production: full tracing in AppInsights
    /// .WithDebugLogging(DebugLoggingOptions.Production)
    ///
    /// // Failures only — 2xx are silent
    /// .WithDebugLogging(DebugLoggingOptions.FailuresOnly)
    ///
    /// // Custom
    /// .WithDebugLogging(opts => {
    ///     opts.SuccessLevel     = ArknLogLevel.Info;
    ///     opts.LogResponseBody  = false;
    /// })
    /// </example>
    IArknHttpBuilder WithDebugLogging(Action<DebugLoggingOptions> configure);

    /// <summary>Enables debug logging with a pre-built <see cref="DebugLoggingOptions"/> instance.</summary>
    IArknHttpBuilder WithDebugLogging(DebugLoggingOptions options);

    /// <summary>Adds a static API key to every request via header (default) or query param.</summary>
    IArknHttpBuilder WithApiKey(string headerName, string value);

    /// <summary>Adds an API key using the specified placement strategy.</summary>
    IArknHttpBuilder WithApiKey(string name, string value, ApiKeyInterceptor.Placement placement);

    /// <summary>
    /// Handles 429 Too Many Requests automatically by reading Retry-After and waiting.
    /// </summary>
    IArknHttpBuilder WithRateLimitHandling(Action<RateLimitOptions>? configure = null);

    /// <summary>Enables in-memory response caching for GET requests.</summary>
    IArknHttpBuilder WithResponseCaching(Action<ResponseCacheOptions>? configure = null);
}
