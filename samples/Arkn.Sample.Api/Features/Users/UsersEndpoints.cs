using Arkn.Results;

namespace Arkn.Sample.Api.Features.Users;

public static class UsersEndpoints
{
    // In-memory store for demo purposes
    private static readonly List<UserDto> _users =
    [
        new(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Alice", "alice@example.com"),
        new(Guid.Parse("11111111-0000-0000-0000-000000000002"), "Bob",   "bob@example.com"),
    ];

    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users").WithTags("Users");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create);

        return app;
    }

    // GET /users
    private static IResult GetAll() =>
        Ok(_users);

    // GET /users/{id}
    private static IResult GetById(Guid id)
    {
        Result<UserDto> result = _users.FirstOrDefault(u => u.Id == id) is { } user
            ? user
            : Error.NotFound("User.NotFound", $"User with id '{id}' was not found.");

        return result.Match(
            onSuccess: dto => Ok(dto),
            onFailure: error => error.Type switch
            {
                ErrorType.NotFound => NotFound(new { error.Code, error.Message }),
                _ => Problem(error.Message)
            });
    }

    // POST /users
    private static IResult Create(UserDto dto)
    {
        Result<UserDto> result = ValidateAndCreate(dto);

        return result.Match(
            onSuccess: created => Created($"/users/{created.Id}", created),
            onFailure: error => BadRequest(new { error.Code, error.Message, error.Type }));
    }

    private static Result<UserDto> ValidateAndCreate(UserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Error.Validation("User.NameRequired", "Name is required.");

        if (!dto.Email.Contains('@'))
            return Error.Validation("User.InvalidEmail", "Email is invalid.");

        if (_users.Any(u => u.Email == dto.Email))
            return Error.Conflict("User.EmailConflict", "A user with this email already exists.");

        var created = dto with { Id = Guid.NewGuid() };
        _users.Add(created);
        return created;
    }
}
