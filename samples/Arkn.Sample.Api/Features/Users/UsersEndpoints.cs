namespace Arkn.Sample.Api.Features.Users;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users").WithTags("Users");

        group.MapGet("/",           (UserService svc) => svc.GetAll()
            .Match(onSuccess: Ok, onFailure: ToHttpResult));

        group.MapGet("/{id:guid}", (Guid id, UserService svc) => svc.GetById(id)
            .Match(onSuccess: Ok, onFailure: ToHttpResult));

        group.MapPost("/",          (CreateUserRequest req, UserService svc) => svc.Create(req)
            .Match(onSuccess: user => Created($"/users/{user.Id}", user), onFailure: ToHttpResult));

        group.MapDelete("/{id:guid}", (Guid id, UserService svc) => svc.Delete(id)
            .Match(onSuccess: () => NoContent(), onFailure: ToHttpResult));

        return app;
    }

    // ── Error → HTTP mapping ── single place, consistent across all endpoints ──

    private static IResult ToHttpResult(Error error) => error.Type switch
    {
        ErrorType.NotFound   => NotFound(new   { error.Code, error.Message }),
        ErrorType.Validation => BadRequest(new { error.Code, error.Message }),
        ErrorType.Conflict   => Conflict(new   { error.Code, error.Message }),
        _                    => Problem(error.Message)
    };
}
