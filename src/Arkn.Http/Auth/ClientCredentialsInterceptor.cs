using System.Net.Http.Headers;
using System.Text.Json;

namespace Arkn.Http.Auth;

/// <summary>
/// Options for the OAuth2 Client Credentials flow.
/// </summary>
public sealed class ClientCredentialsOptions
{
    /// <summary>Token endpoint URL (e.g. <c>https://auth.example.com/oauth/token</c>).</summary>
    public string TokenUrl { get; set; } = string.Empty;

    /// <summary>OAuth2 client_id.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>OAuth2 client_secret.</summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Optional scope(s) to request.</summary>
    public string Scope { get; set; } = string.Empty;

    /// <summary>Cache key used in <see cref="IArknTokenStore"/>. Override when you have multiple CC clients.</summary>
    public string StoreKey { get; set; } = "client_credentials";
}

/// <summary>
/// OAuth2 Client Credentials interceptor. Fetches an access token from <see cref="ClientCredentialsOptions.TokenUrl"/>,
/// caches it via <see cref="IArknTokenStore"/>, and attaches it as <c>Authorization: Bearer &lt;token&gt;</c>.
/// No external dependencies — uses <see cref="HttpClient"/> directly.
/// </summary>
public sealed class ClientCredentialsInterceptor : IArknAuthInterceptor
{
    private readonly ClientCredentialsOptions _options;
    private readonly IArknTokenStore _store;
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <param name="options">Client credentials configuration.</param>
    /// <param name="store">Token cache.</param>
    /// <param name="httpClient">Optional <see cref="HttpClient"/>; a new shared instance is created when null.</param>
    public ClientCredentialsInterceptor(
        ClientCredentialsOptions options,
        IArknTokenStore store,
        HttpClient? httpClient = null)
    {
        _options    = options;
        _store      = store;
        _httpClient = httpClient ?? new HttpClient();
    }

    /// <inheritdoc />
    public async Task ApplyAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        var token = await _store.GetAsync(_options.StoreKey).ConfigureAwait(false);

        if (token is null)
            token = await FetchTokenAsync(cancellationToken).ConfigureAwait(false);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<string> FetchTokenAsync(CancellationToken ct)
    {
        var formFields = new Dictionary<string, string>
        {
            ["grant_type"]    = "client_credentials",
            ["client_id"]     = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
        };

        if (!string.IsNullOrEmpty(_options.Scope))
            formFields["scope"] = _options.Scope;

        using var content  = new FormUrlEncodedContent(formFields);
        using var response = await _httpClient.PostAsync(_options.TokenUrl, content, ct).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var body     = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        var tokenDoc = JsonSerializer.Deserialize<TokenResponse>(body, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize OAuth2 token response.");

        if (string.IsNullOrEmpty(tokenDoc.AccessToken))
            throw new InvalidOperationException("OAuth2 token response did not contain an access_token.");

        // Calculate expiry; default to 55 min if expires_in is absent
        var expiresIn  = tokenDoc.ExpiresIn > 0 ? tokenDoc.ExpiresIn : 3300;
        var expiresAt  = DateTimeOffset.UtcNow.AddSeconds(expiresIn);

        await _store.SetAsync(_options.StoreKey, tokenDoc.AccessToken, expiresAt).ConfigureAwait(false);

        return tokenDoc.AccessToken;
    }

    private sealed class TokenResponse
    {
        public string? AccessToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}
