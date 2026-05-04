namespace Arkn.Sample.Api.Features.Users;

/// <summary>
/// All errors for the User domain — single source of truth.
/// Pattern: never inline Error.* calls inside services or endpoints.
/// </summary>
public static class UserErrors
{
    // ── Dynamic errors (take runtime context) ─────────────────────────────────

    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User '{id}' was not found.");

    // ── Static errors (no runtime context needed) ─────────────────────────────

    public static readonly Error NameRequired =
        Error.Validation("User.NameRequired", "Name is required.");

    public static readonly Error InvalidEmail =
        Error.Validation("User.InvalidEmail"); // message defaults to code

    public static readonly Error EmailAlreadyRegistered =
        Error.Conflict("User.EmailAlreadyRegistered", "A user with this email already exists.");
}
