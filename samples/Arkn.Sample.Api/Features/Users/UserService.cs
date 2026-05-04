namespace Arkn.Sample.Api.Features.Users;

/// <summary>
/// Domain service — all methods return Result, never throw.
/// Errors are defined in UserErrors, not inlined here.
/// </summary>
public sealed class UserService
{
    private readonly List<UserDto> _store =
    [
        new(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Alice", "alice@example.com"),
        new(Guid.Parse("11111111-0000-0000-0000-000000000002"), "Bob",   "bob@example.com"),
    ];

    public Result<IReadOnlyList<UserDto>> GetAll() =>
        Result.Success<IReadOnlyList<UserDto>>(_store.AsReadOnly());

    public Result<UserDto> GetById(Guid id)
    {
        var user = _store.FirstOrDefault(u => u.Id == id);
        return user is null ? UserErrors.NotFound(id) : Result.Success(user);
    }

    public Result<UserDto> Create(CreateUserRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))   return UserErrors.NameRequired;
        if (!req.Email.Contains('@'))               return UserErrors.InvalidEmail;
        if (_store.Any(u => u.Email == req.Email))  return UserErrors.EmailAlreadyRegistered;

        var user = new UserDto(Guid.NewGuid(), req.Name, req.Email);
        _store.Add(user);
        return Result.Success(user);
    }

    public Result Delete(Guid id)
    {
        var user = _store.FirstOrDefault(u => u.Id == id);
        if (user is null) return UserErrors.NotFound(id);

        _store.Remove(user);
        return Result.Success();
    }
}
