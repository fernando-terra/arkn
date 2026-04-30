using Arkn.Http.Builder;

namespace Arkn.Http.Abstractions;

/// <summary>
/// Core abstraction for the Arkn HTTP client.
/// Use this interface to build and execute HTTP requests that return <see cref="Arkn.Results.Result{T}"/>.
/// </summary>
public interface IArknHttp
{
    /// <summary>
    /// Starts building an HTTP request for the given path.
    /// Named placeholders (<c>{id}</c>, <c>{name}</c>) are replaced positionally by <paramref name="args"/>.
    /// </summary>
    /// <param name="path">Relative or absolute path, e.g. <c>"/users/{id}"</c>.</param>
    /// <param name="args">Values to substitute into the path placeholders, in order.</param>
    IArknRequestBuilder Request(string path, params object[] args);
}
