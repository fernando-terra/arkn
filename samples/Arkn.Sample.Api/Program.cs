using Arkn.Core.Abstractions;
using Arkn.Results;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ---------------------------------------------------------------------------
// Fake in-memory "database"
// ---------------------------------------------------------------------------
var users = new Dictionary<int, UserDto>
{
    [1] = new(1, "Alice", "alice@example.com"),
    [2] = new(2, "Bob",   "bob@example.com"),
};

// ---------------------------------------------------------------------------
// Endpoints — demonstrating Arkn.Results in a minimal-API context
// ---------------------------------------------------------------------------

app.MapGet("/users/{id:int}", (int id) =>
{
    var result = GetUser(id);

    return result.Match<IResult>(
        user    => Results.Ok(user),
        errors  => MapErrors(errors));
});

app.MapPost("/users", (CreateUserRequest req) =>
{
    var result = CreateUser(req);

    return result.Match<IResult>(
        user   => Results.Created($"/users/{user.Id}", user),
        errors => MapErrors(errors));
});

app.Run();

// ---------------------------------------------------------------------------
// Domain logic — returns Result<T> with no framework dependency
// ---------------------------------------------------------------------------

Result<UserDto> GetUser(int id)
{
    if (!users.TryGetValue(id, out var user))
        return Error.NotFound("USER.NOT_FOUND", $"User with id {id} was not found.");

    return user; // implicit conversion from UserDto → Result<UserDto>
}

Result<UserDto> CreateUser(CreateUserRequest req)
{
    // Collect all validation errors before returning (never short-circuit on validation)
    var validationErrors = new List<IError>();

    if (string.IsNullOrWhiteSpace(req.Name))
        validationErrors.Add(Error.Validation("USER.NAME_REQUIRED", "Name is required."));

    if (string.IsNullOrWhiteSpace(req.Email) || !req.Email.Contains('@'))
        validationErrors.Add(Error.Validation("USER.EMAIL_INVALID", "A valid email address is required."));

    if (validationErrors.Count > 0)
        return Result<UserDto>.Fail(validationErrors);

    if (users.Values.Any(u => u.Email.Equals(req.Email, StringComparison.OrdinalIgnoreCase)))
        return Error.Conflict("USER.EMAIL_CONFLICT", $"Email '{req.Email}' is already registered.");

    var id   = users.Count + 1;
    var user = new UserDto(id, req.Name!, req.Email!);
    users[id] = user;

    return user;
}

// ---------------------------------------------------------------------------
// Error → HTTP mapping helper
// ---------------------------------------------------------------------------
static IResult MapErrors(IReadOnlyList<IError> errors)
{
    var first = errors[0];

    var status = first.Type switch
    {
        ErrorType.NotFound     => StatusCodes.Status404NotFound,
        ErrorType.Validation   => StatusCodes.Status422UnprocessableEntity,
        ErrorType.Conflict     => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        _                      => StatusCodes.Status500InternalServerError
    };

    return Results.Problem(
        statusCode: status,
        title:      first.Type.ToString(),
        detail:     first.Message,
        extensions: new Dictionary<string, object?>
        {
            ["errors"] = errors.Select(e => new { e.Code, e.Message, e.Type })
        });
}

// ---------------------------------------------------------------------------
// DTOs
// ---------------------------------------------------------------------------
record UserDto(int Id, string Name, string Email);
record CreateUserRequest(string? Name, string? Email);
