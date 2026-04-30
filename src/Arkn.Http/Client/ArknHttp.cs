using Arkn.Http.Abstractions;
using Arkn.Http.Builder;
using Arkn.Http.Configuration;

namespace Arkn.Http.Client;

/// <summary>
/// Default implementation of <see cref="IArknHttp"/>.
/// Wraps a <see cref="System.Net.Http.HttpClient"/> and applies <see cref="ArknHttpOptions"/> to every request.
/// </summary>
public sealed class ArknHttp : IArknHttp
{
    private readonly HttpClient _httpClient;
    private readonly ArknHttpOptions _options;

    /// <param name="httpClient">The underlying HTTP client. Typically provided by <see cref="IHttpClientFactory"/>.</param>
    /// <param name="options">Configuration: timeout, retry, JSON, base URL.</param>
    public ArknHttp(HttpClient httpClient, ArknHttpOptions options)
    {
        _httpClient = httpClient;
        _options    = options;
    }

    /// <inheritdoc />
    public IArknRequestBuilder Request(string path, params object[] args)
    {
        var resolvedPath = ArknRequestBuilder.FormatPath(path, args);

        // Prepend base URL when path is relative and a base URL is configured
        if (!string.IsNullOrEmpty(_options.BaseUrl) && !Uri.IsWellFormedUriString(resolvedPath, UriKind.Absolute))
        {
            var baseUrl = _options.BaseUrl.TrimEnd('/');
            var rel     = resolvedPath.TrimStart('/');
            resolvedPath = $"{baseUrl}/{rel}";
        }

        return new ArknRequestBuilder(_httpClient, resolvedPath, _options);
    }
}
