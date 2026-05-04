using System.Collections.Concurrent;

namespace Arkn.Http.Cache;

/// <summary>
/// Thread-safe in-memory HTTP response cache backed by a <see cref="ConcurrentDictionary"/>.
/// Cache keys are formed from the HTTP method and the full request URI.
/// Entries expire lazily: they are checked on every read and evicted if past their TTL.
/// </summary>
internal sealed class InMemoryResponseCache
{
    private sealed record CachedEntry(byte[] Body, string? ContentType, DateTimeOffset ExpiresAt);

    private readonly ConcurrentDictionary<string, CachedEntry> _cache = new();

    /// <summary>
    /// Attempts to retrieve a non-expired cached response for <paramref name="key"/>.
    /// </summary>
    /// <returns><c>true</c> if a fresh cached entry was found; otherwise <c>false</c>.</returns>
    public bool TryGet(string key, out (byte[] Body, string? ContentType) result)
    {
        if (_cache.TryGetValue(key, out var entry) && entry.ExpiresAt > DateTimeOffset.UtcNow)
        {
            result = (entry.Body, entry.ContentType);
            return true;
        }

        // Evict stale entry
        if (entry is not null)
            _cache.TryRemove(key, out _);

        result = default;
        return false;
    }

    /// <summary>Stores a response body in the cache with the given TTL.</summary>
    public void Set(string key, byte[] body, string? contentType, TimeSpan ttl)
    {
        var entry = new CachedEntry(body, contentType, DateTimeOffset.UtcNow.Add(ttl));
        _cache[key] = entry;
    }

    /// <summary>Removes the cached entry for <paramref name="key"/>, if any.</summary>
    public void Invalidate(string key) => _cache.TryRemove(key, out _);

    /// <summary>Builds a deterministic cache key from an HTTP method and request URI.</summary>
    internal static string BuildKey(HttpMethod method, Uri? uri) =>
        $"{method.Method}:{uri}";
}
