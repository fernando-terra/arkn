using System.Net.Http.Headers;

namespace Arkn.Http.Auth;

/// <summary>
/// Bearer-token interceptor. Fetches a token via <paramref name="tokenFactory"/>,
/// caches it in <see cref="IArknTokenStore"/> under <paramref name="storeKey"/>,
/// and attaches it as <c>Authorization: Bearer &lt;token&gt;</c> on every request.
/// Tokens are cached for 55 minutes by default.
/// </summary>
public sealed class BearerTokenInterceptor : IArknAuthInterceptor
{
    private readonly Func<Task<string>> _tokenFactory;
    private readonly string _storeKey;
    private readonly IArknTokenStore _store;

    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(55);

    /// <param name="tokenFactory">Async delegate that returns a bearer token string.</param>
    /// <param name="storeKey">Cache key (namespace per client).</param>
    /// <param name="store">Token store (typically <see cref="InMemoryTokenStore"/>).</param>
    public BearerTokenInterceptor(
        Func<Task<string>> tokenFactory,
        string storeKey,
        IArknTokenStore store)
    {
        _tokenFactory = tokenFactory;
        _storeKey     = storeKey;
        _store        = store;
    }

    /// <inheritdoc />
    public async Task ApplyAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        var token = await _store.GetAsync(_storeKey).ConfigureAwait(false);

        if (token is null)
        {
            token = await _tokenFactory().ConfigureAwait(false);
            await _store.SetAsync(_storeKey, token, DateTimeOffset.UtcNow.Add(DefaultCacheDuration))
                        .ConfigureAwait(false);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
