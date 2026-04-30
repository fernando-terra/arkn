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
}
