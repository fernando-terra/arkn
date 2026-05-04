using Arkn.Results;

namespace Arkn.Sample.Api.Features.HttpDemo;

/// <summary>
/// Demonstrates Arkn.Http typed client in a minimal API.
/// All routes return clean responses — no try/catch blocks.
/// </summary>
public static class HttpDemoEndpoints
{
    public static IEndpointRouteBuilder MapHttpDemoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/external/users").WithTags("HttpDemo");

        group.MapGet("/",       ListUsers);
        group.MapGet("/{id}",   GetUser);
        group.MapPost("/",      CreateUser);
        group.MapDelete("/{id}", DeleteUser);

        return app;
    }

    // GET /external/users
    private static async Task<IResult> ListUsers(ExternalUserClient client)
    {
        Result<IReadOnlyList<ExternalUser>> result = await client.ListAsync();

        return result.Match(
            onSuccess: users => Ok(users),
            onFailure: ToHttpResult);
    }

    // GET /external/users/{id}
    private static async Task<IResult> GetUser(int id, ExternalUserClient client)
    {
        Result<ExternalUser> result = await client.GetAsync(id);

        return result.Match(
            onSuccess: user  => Ok(user),
            onFailure: ToHttpResult);
    }

    // POST /external/users
    private static async Task<IResult> CreateUser(CreateExternalUserRequest req, ExternalUserClient client)
    {
        Result<ExternalUser> result = await client.CreateAsync(req);

        return result.Match(
            onSuccess: created => Created($"/external/users/{created.Id}", created),
            onFailure: ToHttpResult);
    }

    // DELETE /external/users/{id}
    private static async Task<IResult> DeleteUser(int id, ExternalUserClient client)
    {
        Result result = await client.DeleteAsync(id);

        return result.Match(
            onSuccess: () => NoContent(),
            onFailure: ToHttpResult);
    }

    // ── Error → HTTP mapping ───────────────────────────────────────────────────

    private static IResult ToHttpResult(Error error) => error.Type switch
    {
        ErrorType.NotFound     => NotFound(new { error.Code, error.Message }),
        ErrorType.Validation   => BadRequest(new { error.Code, error.Message }),
        ErrorType.Unauthorized => Unauthorized(),
        ErrorType.Conflict     => Conflict(new { error.Code, error.Message }),
        _                      => Problem(detail: error.Message, title: error.Code)
    };
}
