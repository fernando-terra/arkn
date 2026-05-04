namespace Arkn.Http.Resilience;

/// <summary>
/// Options that control the automatic 429 wait-and-retry behaviour.
/// </summary>
public sealed class RateLimitOptions
{
    /// <summary>Maximum number of times to wait-and-retry on 429. Default: 3.</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>Fallback wait when Retry-After header is missing. Default: 5 seconds.</summary>
    public TimeSpan FallbackDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>Cap on how long to wait even if Retry-After says longer. Default: 60 seconds.</summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(60);
}

/// <summary>
/// Wraps an execute lambda: when a 429 response arrives, reads the Retry-After header,
/// waits the required amount of time, and retries. Works with both delta-seconds and
/// HTTP-date forms of the Retry-After header.
/// </summary>
internal static class RateLimitHandler
{
    internal static async Task<HttpResponseMessage> ExecuteWithRateLimitAsync(
        Func<Task<HttpResponseMessage>> execute,
        RateLimitOptions opts)
    {
        for (int attempt = 0; attempt <= opts.MaxRetries; attempt++)
        {
            var response = await execute().ConfigureAwait(false);

            if ((int)response.StatusCode != 429 || attempt == opts.MaxRetries)
                return response;

            var delay = ParseRetryAfter(response, opts.FallbackDelay, opts.MaxDelay);
            response.Dispose();
            await Task.Delay(delay).ConfigureAwait(false);
        }

        // unreachable — the loop always returns inside the body
        return await execute().ConfigureAwait(false);
    }

    private static TimeSpan ParseRetryAfter(HttpResponseMessage response, TimeSpan fallback, TimeSpan max)
    {
        if (response.Headers.RetryAfter is { } retryAfter)
        {
            if (retryAfter.Delta.HasValue)
                return retryAfter.Delta.Value > max ? max : retryAfter.Delta.Value;

            if (retryAfter.Date.HasValue)
            {
                var wait = retryAfter.Date.Value - DateTimeOffset.UtcNow;
                if (wait > TimeSpan.Zero)
                    return wait > max ? max : wait;
            }
        }

        return fallback;
    }
}
