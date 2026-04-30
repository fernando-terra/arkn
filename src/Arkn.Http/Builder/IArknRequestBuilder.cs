namespace Arkn.Http.Builder;

/// <summary>
/// Fluent interface for building an HTTP request before execution.
/// </summary>
public interface IArknRequestBuilder
{
    /// <summary>Adds a request header.</summary>
    IArknRequestBuilder WithHeader(string name, string value);

    /// <summary>Overrides the default request timeout for this request only.</summary>
    IArknRequestBuilder WithTimeout(TimeSpan timeout);

    /// <summary>Sets the request body. Serialized to JSON using the configured <c>JsonSerializerOptions</c>.</summary>
    IArknRequestBuilder WithBody(object body);

    /// <summary>Sends an HTTP GET request.</summary>
    ArknHttpResponseHandler Get();

    /// <summary>Sends an HTTP POST request.</summary>
    ArknHttpResponseHandler Post();

    /// <summary>Sends an HTTP PUT request.</summary>
    ArknHttpResponseHandler Put();

    /// <summary>Sends an HTTP PATCH request.</summary>
    ArknHttpResponseHandler Patch();

    /// <summary>Sends an HTTP DELETE request.</summary>
    ArknHttpResponseHandler Delete();
}
