using Arkn.Http.Abstractions;
using Arkn.Http.Builder;

namespace Arkn.Http.Client;

/// <summary>
/// Base class for strongly-typed HTTP clients that return <see cref="Arkn.Results.Result{T}"/>.
/// </summary>
/// <remarks>
/// Inherit from this class and inject <see cref="IArknHttp"/> to create a clean, typed client:
/// <code>
/// public class UserClient : ArknHttpClient
/// {
///     public UserClient(IArknHttp http) : base(http, "https://api.example.com") { }
///
///     public Task&lt;Result&lt;User&gt;&gt; GetAsync(int id) =&gt;
///         Request($"/users/{id}").Get().As&lt;User&gt;();
/// }
/// </code>
/// </remarks>
public abstract class ArknHttpClient
{
    private readonly IArknHttp _http;
    private readonly string _baseUrl;

    /// <param name="http">The Arkn HTTP abstraction injected from DI.</param>
    /// <param name="baseUrl">
    /// Base URL for all requests made by this typed client.
    /// Overrides the base URL configured at the <see cref="IArknHttp"/> level.
    /// </param>
    protected ArknHttpClient(IArknHttp http, string baseUrl)
    {
        _http    = http;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    /// <summary>
    /// Starts building a request relative to this client's base URL.
    /// Named placeholders (<c>{id}</c>, <c>{name}</c>) in <paramref name="path"/> are
    /// replaced positionally by <paramref name="args"/>.
    /// </summary>
    protected IArknRequestBuilder Request(string path, params object[] args)
    {
        var formatted = ArknRequestBuilder.FormatPath(path, args);
        var fullPath  = BuildFullPath(formatted);
        return _http.Request(fullPath);
    }

    private string BuildFullPath(string path)
    {
        if (string.IsNullOrEmpty(_baseUrl)) return path;
        if (Uri.IsWellFormedUriString(path, UriKind.Absolute)) return path;

        var rel = path.TrimStart('/');
        return $"{_baseUrl}/{rel}";
    }
}
