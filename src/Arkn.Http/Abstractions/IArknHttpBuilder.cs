using System.Security.Cryptography.X509Certificates;
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

    // ── mTLS ─────────────────────────────────────────────────────────────────

    /// <summary>Attaches an already-loaded certificate to every TLS handshake (mTLS).</summary>
    IArknHttpBuilder WithClientCertificate(X509Certificate2 certificate);

    /// <summary>Loads a client certificate from a PFX/PKCS#12 file.</summary>
    /// <param name="pfxPath">Path to the .pfx or .p12 file.</param>
    /// <param name="password">Password protecting the file, or <c>null</c> if unprotected.</param>
    IArknHttpBuilder WithClientCertificate(string pfxPath, string? password = null);

    /// <summary>
    /// Loads a client certificate from PEM-encoded certificate and private key files (.pem / .crt / .key).
    /// Use this overload to distinguish from the PFX overload when both paths are provided.
    /// </summary>
    IArknHttpBuilder WithClientCertificatePem(string certPemPath, string keyPemPath);

    /// <summary>Loads a client certificate from the OS certificate store by thumbprint.</summary>
    IArknHttpBuilder WithClientCertificate(StoreName storeName, StoreLocation location, string thumbprint);
}
