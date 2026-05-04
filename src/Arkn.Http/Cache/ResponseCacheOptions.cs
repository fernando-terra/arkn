namespace Arkn.Http.Cache;

/// <summary>
/// Options controlling which responses are cached and for how long.
/// </summary>
public sealed class ResponseCacheOptions
{
    /// <summary>Default TTL for cached responses. Default: 5 minutes.</summary>
    public TimeSpan DefaultTtl { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>HTTP methods to cache. Default: GET only.</summary>
    public HashSet<string> CacheMethods { get; set; } = ["GET"];

    /// <summary>Response status codes to cache. Default: 200 only.</summary>
    public HashSet<int> CacheStatusCodes { get; set; } = [200];
}
