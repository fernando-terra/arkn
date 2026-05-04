using System.Collections.Concurrent;

namespace Arkn.Http.Auth;

/// <summary>
/// Thread-safe in-memory token store. Tokens are auto-expired with a 30-second buffer
/// to avoid using a token that is about to expire.
/// </summary>
public sealed class InMemoryTokenStore : IArknTokenStore
{
    private record CachedToken(string Value, DateTimeOffset ExpiresAt);

    private readonly ConcurrentDictionary<string, CachedToken> _cache = new();
    private static readonly TimeSpan ExpiryBuffer = TimeSpan.FromSeconds(30);

    /// <inheritdoc />
    public Task<string?> GetAsync(string key)
    {
        if (_cache.TryGetValue(key, out var cached) &&
            cached.ExpiresAt - ExpiryBuffer > DateTimeOffset.UtcNow)
        {
            return Task.FromResult<string?>(cached.Value);
        }

        return Task.FromResult<string?>(null);
    }

    /// <inheritdoc />
    public Task SetAsync(string key, string token, DateTimeOffset expiresAt)
    {
        _cache[key] = new CachedToken(token, expiresAt);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task InvalidateAsync(string key)
    {
        _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
