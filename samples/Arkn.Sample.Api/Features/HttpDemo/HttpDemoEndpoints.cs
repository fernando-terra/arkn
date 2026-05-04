namespace Arkn.Sample.Api.Features.HttpDemo;

public static class HttpDemoEndpoints
{
    public static IEndpointRouteBuilder MapHttpDemoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/external/users").WithTags("HttpDemo");

        group.MapGet("/",
            async (ExternalUserClient client) => (await client.ListAsync())
                .Match(onSuccess: Ok, onFailure: ToHttpResult));

        group.MapGet("/{id:int}",
            async (int id, ExternalUserClient client) => (await client.GetAsync(id))
                .Match(onSuccess: Ok, onFailure: ToHttpResult));

        group.MapPost("/",
            async (CreateExternalUserRequest req, ExternalUserClient client) =>
                (await client.CreateAsync(req))
                    .Match(onSuccess: u => Created($"/external/users/{u.Id}", u), onFailure: ToHttpResult));

        group.MapDelete("/{id:int}",
            async (int id, ExternalUserClient client) => (await client.DeleteAsync(id))
                .Match(onSuccess: () => NoContent(), onFailure: ToHttpResult));

        return app;
    }

    private static IResult ToHttpResult(Error error) => error.Type switch
    {
        ErrorType.NotFound   => NotFound(new   { error.Code, error.Message }),
        ErrorType.Validation => BadRequest(new { error.Code, error.Message }),
        _                    => Problem(error.Message)
    };
}
