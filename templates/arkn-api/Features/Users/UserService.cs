using Arkn.Results;

namespace ArknApi.Features.Users;

public record User(Guid Id, string Name, string Email);
public record CreateUserRequest(string Name, string Email);

public class UserService
{
    private readonly List<User> _store =
    [
        new(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Alice", "alice@example.com"),
    ];

    public Result<User> GetById(Guid id)
    {
        var user = _store.FirstOrDefault(u => u.Id == id);
        return user is null ? UserErrors.NotFound(id) : Result.Success(user);
    }

    public Result<User> Create(CreateUserRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))  return UserErrors.NameRequired;
        if (!req.Email.Contains('@'))              return UserErrors.InvalidEmail;
        if (_store.Any(u => u.Email == req.Email)) return UserErrors.EmailAlreadyRegistered;

        var user = new User(Guid.NewGuid(), req.Name, req.Email);
        _store.Add(user);
        return Result.Success(user);
    }
}
