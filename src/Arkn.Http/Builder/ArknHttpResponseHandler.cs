using System.Text.Json;
using Arkn.Http.Errors;
using Arkn.Results;

namespace Arkn.Http.Builder;

/// <summary>
/// Handles the HTTP response from an executed request:
/// deserializes the body and maps success/failure to <see cref="Result{T}"/> or <see cref="Result"/>.
/// </summary>
/// <remarks>
/// Returned by the HTTP verb methods on <see cref="IArknRequestBuilder"/> (<c>.Get()</c>, <c>.Post()</c>, etc.).
/// Await <see cref="As{T}"/> or <see cref="AsResult"/> to execute the request.
/// </remarks>
public sealed class ArknHttpResponseHandler
{
    private readonly Func<Task<HttpResponseMessage>> _execute;
    private readonly JsonSerializerOptions _jsonOptions;

    internal ArknHttpResponseHandler(
        Func<Task<HttpResponseMessage>> execute,
        JsonSerializerOptions jsonOptions)
    {
        _execute     = execute;
        _jsonOptions = jsonOptions;
    }

    /// <summary>
    /// Executes the request and deserializes the response body to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Expected response type.</typeparam>
    /// <returns>
    /// <see cref="Result{T}.Success"/> with the deserialized value,
    /// or <see cref="Result{T}.Failure"/> with a mapped <see cref="Error"/>.
    /// </returns>
    public async Task<Result<T>> As<T>()
    {
        try
        {
            using var response = await _execute().ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                return Result.Failure<T>(HttpErrors.FromStatusCode(response.StatusCode, TrimDetail(body)));

            if (string.IsNullOrWhiteSpace(body))
                return Result.Failure<T>(Error.Failure("Http.EmptyResponse", "Response body was empty."));

            var value = JsonSerializer.Deserialize<T>(body, _jsonOptions);
            if (value is null)
                return Result.Failure<T>(Error.Failure("Http.DeserializationError", "Failed to deserialize response body."));

            return Result.Success(value);
        }
        catch (TaskCanceledException)
        {
            return Result.Failure<T>(HttpErrors.Timeout);
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<T>(Error.Failure("Http.RequestFailed", ex.Message));
        }
        catch (JsonException ex)
        {
            return Result.Failure<T>(Error.Failure("Http.DeserializationError", $"JSON error: {ex.Message}"));
        }
    }

    /// <summary>
    /// Executes the request and returns a value-less <see cref="Result"/>.
    /// Useful for operations like DELETE or PUT where no response body is expected.
    /// </summary>
    public async Task<Result> AsResult()
    {
        try
        {
            using var response = await _execute().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return Result.Failure(HttpErrors.FromStatusCode(response.StatusCode, TrimDetail(body)));
            }

            return Result.Success();
        }
        catch (TaskCanceledException)
        {
            return Result.Failure(HttpErrors.Timeout);
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure(Error.Failure("Http.RequestFailed", ex.Message));
        }
    }

    // Trim long response bodies used as error details to avoid noise
    private static string? TrimDetail(string? body) =>
        string.IsNullOrWhiteSpace(body) ? null
        : body.Length > 500 ? body[..500]
        : body;
}
