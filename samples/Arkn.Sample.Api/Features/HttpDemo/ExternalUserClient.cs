using Arkn.Http.Abstractions;
using Arkn.Http.Client;

namespace Arkn.Sample.Api.Features.HttpDemo;

public sealed record ExternalUser(int Id, string Name, string Email);
public sealed record CreateExternalUserRequest(string Name, string Email);

/// <summary>
/// Typed HTTP client using Arkn.Http shorthand methods.
/// Auth and debug logging are configured in Program.cs — not here.
/// </summary>
public sealed class ExternalUserClient(IArknHttp http)
    : ArknHttpClient(http, "https://jsonplaceholder.typicode.com")
{
    public Task<Result<ExternalUser>>              GetAsync(int id)  => GetAs<ExternalUser>("/users/{id}", id);
    public Task<Result<IReadOnlyList<ExternalUser>>> ListAsync()     => GetAs<IReadOnlyList<ExternalUser>>("/users");
    public Task<Result<ExternalUser>>              CreateAsync(CreateExternalUserRequest req) => PostAs<ExternalUser>("/users", req);
    public Task<Result>                            DeleteAsync(int id) => Delete("/users/{id}", id);
}
