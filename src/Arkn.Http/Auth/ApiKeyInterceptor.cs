namespace Arkn.Http.Auth;

/// <summary>
/// Attaches a static API key to every request via header or query parameter.
/// </summary>
public sealed class ApiKeyInterceptor : IArknAuthInterceptor
{
    public enum Placement { Header, QueryParam }

    private readonly string _name;
    private readonly string _value;
    private readonly Placement _placement;

    public ApiKeyInterceptor(string name, string value, Placement placement = Placement.Header)
    {
        _name      = name;
        _value     = value;
        _placement = placement;
    }

    public Task ApplyAsync(HttpRequestMessage request, CancellationToken ct = default)
    {
        if (_placement == Placement.Header)
        {
            request.Headers.TryAddWithoutValidation(_name, _value);
        }
        else
        {
            var uri       = request.RequestUri!;
            // Use OriginalString to safely work with both absolute and relative URIs
            // (Uri.Query throws InvalidOperationException on relative URIs).
            var uriStr    = uri.IsAbsoluteUri ? uri.AbsoluteUri : uri.OriginalString;
            var hasQuery  = uriStr.Contains('?');
            var sep       = hasQuery ? "&" : "?";
            var newStr    = $"{uriStr}{sep}{Uri.EscapeDataString(_name)}={Uri.EscapeDataString(_value)}";
            request.RequestUri = uri.IsAbsoluteUri
                ? new Uri(newStr)
                : new Uri(newStr, UriKind.Relative);
        }

        return Task.CompletedTask;
    }
}
