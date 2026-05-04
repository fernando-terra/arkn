namespace Arkn.Http.Auth;

/// <summary>Token store abstraction. Default implementation is in-memory.</summary>
public interface IArknTokenStore
{
    /// <summary>Returns the cached token, or null if expired/missing.</summary>
    Task<string?> GetAsync(string key);

    /// <summary>Stores a token with an absolute expiry.</summary>
    Task SetAsync(string key, string token, DateTimeOffset expiresAt);

    /// <summary>Removes a token from the store (forces re-auth on next request).</summary>
    Task InvalidateAsync(string key);
}
