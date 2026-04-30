namespace Arkn.Http.Configuration;

/// <summary>
/// Native exponential-backoff retry policy. No Polly. No external dependencies.
/// </summary>
/// <remarks>
/// Retry triggers:
/// <list type="bullet">
///   <item><see cref="HttpRequestException"/> — network-level failures.</item>
///   <item><see cref="TaskCanceledException"/> — request timeouts.</item>
///   <item>HTTP 5xx — server-side errors.</item>
/// </list>
/// Never retried: HTTP 4xx (client errors — fix the request, not the retry count).
/// </remarks>
public static class ArknRetryPolicy
{
    private static readonly Random Jitter = new();

    /// <summary>
    /// Executes <paramref name="sendAsync"/> with retry logic applied.
    /// </summary>
    /// <param name="sendAsync">
    /// A factory that builds and sends a fresh <see cref="HttpRequestMessage"/> each attempt.
    /// Must create a new message per call — <see cref="HttpRequestMessage"/> cannot be reused.
    /// </param>
    /// <param name="maxAttempts">Total attempts including the initial one. Must be ≥ 1.</param>
    /// <param name="baseDelay">Base delay for exponential backoff.</param>
    /// <param name="ct">Cancellation token.</param>
    public static async Task<HttpResponseMessage> ExecuteAsync(
        Func<Task<HttpResponseMessage>> sendAsync,
        int maxAttempts,
        TimeSpan baseDelay,
        CancellationToken ct = default)
    {
        if (maxAttempts < 1) maxAttempts = 1;

        Exception? lastException = null;
        HttpResponseMessage? lastResponse = null;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var response = await sendAsync().ConfigureAwait(false);

                // 5xx → retryable if attempts remain
                if ((int)response.StatusCode >= 500 && attempt < maxAttempts - 1)
                {
                    response.Dispose();
                    await DelayAsync(baseDelay, attempt, ct).ConfigureAwait(false);
                    continue;
                }

                // 4xx or success → return immediately, no retry
                return response;
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                // Timeout (not user cancellation)
                lastException = ex;
            }

            if (attempt < maxAttempts - 1)
                await DelayAsync(baseDelay, attempt, ct).ConfigureAwait(false);
        }

        // All attempts exhausted — re-throw the last exception
        if (lastException is not null)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(lastException).Throw();

        // Shouldn't reach here, but compiler needs a return
        return lastResponse!;
    }

    /// <summary>
    /// Computes delay for a given attempt using exponential backoff + jitter.
    /// Formula: <c>baseDelay × 2^attempt + Random(0..100ms)</c>.
    /// </summary>
    private static Task DelayAsync(TimeSpan baseDelay, int attempt, CancellationToken ct)
    {
        var exponential = baseDelay.TotalMilliseconds * Math.Pow(2, attempt);
        var jitter      = Jitter.NextDouble() * 100;
        var totalMs     = Math.Min(exponential + jitter, 30_000); // cap at 30s
        return Task.Delay(TimeSpan.FromMilliseconds(totalMs), ct);
    }
}
