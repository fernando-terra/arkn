using System.Net;
using System.Net.Http.Headers;
using Arkn.Http.Resilience;
using Xunit;

namespace Arkn.Http.Tests.Resilience;

public sealed class RateLimitHandlerTests
{
    // ── Retry-After: delta seconds ─────────────────────────────────────────────

    [Fact]
    public async Task ExecuteWithRateLimitAsync_On429_WaitsAndRetries_WithDeltaSeconds()
    {
        var callCount = 0;
        var delays    = new List<TimeSpan>();
        var opts      = new RateLimitOptions
        {
            MaxRetries    = 3,
            FallbackDelay = TimeSpan.FromSeconds(1),
            MaxDelay      = TimeSpan.FromSeconds(60),
        };

        // Replace Task.Delay by overriding via a thin wrapper is not possible directly,
        // so we use near-zero Retry-After to avoid slowing tests.
        Task<HttpResponseMessage> Execute()
        {
            callCount++;
            if (callCount == 1)
            {
                var r = new HttpResponseMessage((HttpStatusCode)429);
                r.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromMilliseconds(1));
                return Task.FromResult(r);
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }

        var result = await RateLimitHandler.ExecuteWithRateLimitAsync(Execute, opts);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(2, callCount); // one 429, then one 200
    }

    [Fact]
    public async Task ExecuteWithRateLimitAsync_On429_WaitsAndRetries_WithDateHeader()
    {
        var callCount = 0;
        var opts      = new RateLimitOptions
        {
            MaxRetries    = 3,
            FallbackDelay = TimeSpan.FromSeconds(1),
            MaxDelay      = TimeSpan.FromSeconds(60),
        };

        Task<HttpResponseMessage> Execute()
        {
            callCount++;
            if (callCount == 1)
            {
                var r = new HttpResponseMessage((HttpStatusCode)429);
                // Retry-After as HTTP date just 1 ms in the future
                r.Headers.RetryAfter = new RetryConditionHeaderValue(DateTimeOffset.UtcNow.AddMilliseconds(1));
                return Task.FromResult(r);
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }

        var result = await RateLimitHandler.ExecuteWithRateLimitAsync(Execute, opts);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task ExecuteWithRateLimitAsync_FallbackDelay_UsedWhenNoRetryAfterHeader()
    {
        var callCount = 0;
        // Use a zero-ish fallback so the test doesn't hang
        var opts = new RateLimitOptions
        {
            MaxRetries    = 2,
            FallbackDelay = TimeSpan.FromMilliseconds(1),
            MaxDelay      = TimeSpan.FromSeconds(60),
        };

        Task<HttpResponseMessage> Execute()
        {
            callCount++;
            if (callCount < 2)
                return Task.FromResult(new HttpResponseMessage((HttpStatusCode)429));
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }

        var result = await RateLimitHandler.ExecuteWithRateLimitAsync(Execute, opts);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task ExecuteWithRateLimitAsync_RespectsMaxRetries_ReturnsLast429()
    {
        var callCount = 0;
        var opts = new RateLimitOptions
        {
            MaxRetries    = 2,
            FallbackDelay = TimeSpan.FromMilliseconds(1),
            MaxDelay      = TimeSpan.FromSeconds(60),
        };

        Task<HttpResponseMessage> Execute()
        {
            callCount++;
            var r = new HttpResponseMessage((HttpStatusCode)429);
            r.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromMilliseconds(1));
            return Task.FromResult(r);
        }

        var result = await RateLimitHandler.ExecuteWithRateLimitAsync(Execute, opts);

        // MaxRetries = 2 → 1 initial + 2 retries = 3 total calls, last response returned as-is
        Assert.Equal((HttpStatusCode)429, result.StatusCode);
        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task ExecuteWithRateLimitAsync_NonRateLimitResponse_ReturnedImmediately()
    {
        var callCount = 0;
        var opts = new RateLimitOptions { MaxRetries = 5, FallbackDelay = TimeSpan.FromSeconds(1) };

        Task<HttpResponseMessage> Execute()
        {
            callCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }

        var result = await RateLimitHandler.ExecuteWithRateLimitAsync(Execute, opts);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(1, callCount); // no retries for non-429
    }

    [Fact]
    public async Task ExecuteWithRateLimitAsync_MaxDelay_CapsRetryAfterDelta()
    {
        // Even if Retry-After says 120s, MaxDelay of 1ms should cap it
        var callCount = 0;
        var opts = new RateLimitOptions
        {
            MaxRetries    = 1,
            FallbackDelay = TimeSpan.FromSeconds(1),
            MaxDelay      = TimeSpan.FromMilliseconds(1), // tiny cap
        };

        Task<HttpResponseMessage> Execute()
        {
            callCount++;
            if (callCount == 1)
            {
                var r = new HttpResponseMessage((HttpStatusCode)429);
                r.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(120));
                return Task.FromResult(r);
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }

        // Should complete quickly because MaxDelay caps the wait to 1ms
        var result = await RateLimitHandler.ExecuteWithRateLimitAsync(Execute, opts);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(2, callCount);
    }
}
