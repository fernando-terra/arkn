using System.Net;
using Arkn.Results;

namespace Arkn.Http.Errors;

/// <summary>
/// Standard <see cref="Error"/> codes for HTTP failures.
/// Maps HTTP status codes to typed <see cref="ErrorType"/> values.
/// </summary>
public static class HttpErrors
{
    /// <summary>Request timed out.</summary>
    public static readonly Error Timeout =
        Error.Failure("Http.Timeout", "Request timed out.");

    /// <summary>
    /// Maps an HTTP status code to a structured <see cref="Error"/>.
    /// </summary>
    /// <param name="statusCode">The HTTP status code returned by the server.</param>
    /// <param name="detail">Optional detail extracted from the response body.</param>
    public static Error FromStatusCode(HttpStatusCode statusCode, string? detail = null)
    {
        var code = (int)statusCode;
        return code switch
        {
            400 => Error.Validation("Http.BadRequest",    detail ?? "Bad request."),
            401 => Error.Unauthorized("Http.Unauthorized", detail ?? "Authentication required."),
            403 => Error.Unauthorized("Http.Forbidden",   detail ?? "Access forbidden."),
            404 => Error.NotFound("Http.NotFound",        detail ?? "Resource not found."),
            409 => Error.Conflict("Http.Conflict",        detail ?? "Conflict with the current state of the resource."),
            422 => Error.Validation("Http.UnprocessableEntity", detail ?? "Unprocessable entity."),
            429 => Error.Failure("Http.RateLimited",      detail ?? "Too many requests."),
            >= 500 => Error.Failure($"Http.ServerError.{code}", detail ?? $"Server error ({code})."),
            _ => Error.Failure($"Http.Error.{code}",     detail ?? $"HTTP error ({code}).")
        };
    }
}
