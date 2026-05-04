using Arkn.Results;

namespace ArknApi.Features.Users;

/// <summary>
/// All errors for the User domain in one place.
/// 
/// Pattern: define errors as static members here — never inline Error.* calls in services.
/// This makes errors discoverable, reusable, and easy to document.
/// </summary>
public static class UserErrors
{
    // ── Not Found ─────────────────────────────────────────────────────────────

    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User '{id}' was not found.");

    // ── Validation ────────────────────────────────────────────────────────────

    public static readonly Error NameRequired =
        Error.Validation("User.NameRequired", "Name is required.");

    public static readonly Error InvalidEmail =
        Error.Validation("User.InvalidEmail", "Email format is invalid.");

    // ── Conflict ──────────────────────────────────────────────────────────────

    public static readonly Error EmailAlreadyRegistered =
        Error.Conflict("User.EmailAlreadyRegistered", "This email is already in use.");
}
