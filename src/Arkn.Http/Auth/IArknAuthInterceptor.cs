namespace Arkn.Http.Auth;

/// <summary>
/// Pluggable authentication interceptor. Called before every request to attach credentials.
/// </summary>
public interface IArknAuthInterceptor
{
    /// <summary>Attaches authentication headers to the outgoing request.</summary>
    Task ApplyAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
}
