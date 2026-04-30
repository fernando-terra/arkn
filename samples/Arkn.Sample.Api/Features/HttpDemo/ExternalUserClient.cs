using Arkn.Http.Abstractions;
using Arkn.Http.Client;
using Arkn.Results;

namespace Arkn.Sample.Api.Features.HttpDemo;

// ── DTOs ───────────────────────────────────────────────────────────────────────

public sealed record ExternalUser(int Id, string Name, string Email);
public sealed record CreateExternalUserRequest(string Name, string Email);

// ── Typed client ───────────────────────────────────────────────────────────────

/// <summary>
/// Example typed HTTP client using Arkn.Http.
/// All methods return Result&lt;T&gt; — no exceptions leak to the caller.
/// </summary>
public sealed class ExternalUserClient : ArknHttpClient
{
    // The base URL is injected here; the DI registration configures the HttpClient.
    public ExternalUserClient(IArknHttp http)
        : base(http, "https://jsonplaceholder.typicode.com") { }

    /// <summary>Fetches a single user by id. Returns NotFound on 404.</summary>
    public Task<Result<ExternalUser>> GetAsync(int id) =>
        Request("/users/{id}", id).Get().As<ExternalUser>();

    /// <summary>Lists all users.</summary>
    public Task<Result<IReadOnlyList<ExternalUser>>> ListAsync() =>
        Request("/users").Get().As<IReadOnlyList<ExternalUser>>();

    /// <summary>Creates a user (JSONPlaceholder echoes the body back).</summary>
    public Task<Result<ExternalUser>> CreateAsync(CreateExternalUserRequest req) =>
        Request("/users").WithBody(req).Post().As<ExternalUser>();

    /// <summary>Deletes a user (JSONPlaceholder always returns 200 for this).</summary>
    public Task<Result> DeleteAsync(int id) =>
        Request("/users/{id}", id).Delete().AsResult();
}
