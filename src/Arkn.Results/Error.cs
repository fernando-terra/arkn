namespace Arkn.Results;

/// <summary>
/// Represents a structured error with a machine-readable code, human-readable message,
/// and an <see cref="ErrorType"/> classification.
/// </summary>
/// <param name="Code">A dot-separated identifier, e.g. <c>"User.NotFound"</c>.</param>
/// <param name="Message">A human-readable description of the failure.</param>
/// <param name="Type">The category of this error.</param>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    /// <summary>The empty, no-error sentinel. Used internally by successful results.</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    // ── Factories ──────────────────────────────────────────────────────────────
    // message is optional — when omitted it defaults to the code value.
    // Use a custom message for user-facing contexts; omit for internal/logging.

    /// <summary>Creates a generic failure error.</summary>
    /// <param name="code">A dot-separated identifier, e.g. <c>"Order.Failed"</c>.</param>
    /// <param name="message">Optional human-readable message; defaults to <paramref name="code"/>.</param>
    /// <returns>A failure-classified <see cref="Error"/>.</returns>
    public static Error Failure(string code, string? message = null) =>
        new(code, message ?? code, ErrorType.Failure);

    /// <summary>Creates a not-found error.</summary>
    /// <param name="code">A dot-separated identifier, e.g. <c>"User.NotFound"</c>.</param>
    /// <param name="message">Optional human-readable message; defaults to <paramref name="code"/>.</param>
    /// <returns>A not-found-classified <see cref="Error"/>.</returns>
    public static Error NotFound(string code, string? message = null) =>
        new(code, message ?? code, ErrorType.NotFound);

    /// <summary>Creates a validation error.</summary>
    /// <param name="code">A dot-separated identifier, e.g. <c>"Email.Invalid"</c>.</param>
    /// <param name="message">Optional human-readable message; defaults to <paramref name="code"/>.</param>
    /// <returns>A validation-classified <see cref="Error"/>.</returns>
    public static Error Validation(string code, string? message = null) =>
        new(code, message ?? code, ErrorType.Validation);

    /// <summary>Creates a conflict error.</summary>
    /// <param name="code">A dot-separated identifier, e.g. <c>"User.EmailAlreadyRegistered"</c>.</param>
    /// <param name="message">Optional human-readable message; defaults to <paramref name="code"/>.</param>
    /// <returns>A conflict-classified <see cref="Error"/>.</returns>
    public static Error Conflict(string code, string? message = null) =>
        new(code, message ?? code, ErrorType.Conflict);

    /// <summary>Creates an unauthorized error.</summary>
    /// <param name="code">A dot-separated identifier, e.g. <c>"Auth.TokenExpired"</c>.</param>
    /// <param name="message">Optional human-readable message; defaults to <paramref name="code"/>.</param>
    /// <returns>An unauthorized-classified <see cref="Error"/>.</returns>
    public static Error Unauthorized(string code, string? message = null) =>
        new(code, message ?? code, ErrorType.Unauthorized);

    /// <summary>Creates a forbidden error.</summary>
    /// <param name="code">A dot-separated identifier, e.g. <c>"User.Forbidden"</c>.</param>
    /// <param name="message">Optional human-readable message; defaults to <paramref name="code"/>.</param>
    /// <returns>A forbidden-classified <see cref="Error"/>.</returns>
    public static Error Forbidden(string code, string? message = null) =>
        new(code, message ?? code, ErrorType.Forbidden);

    /// <summary>Returns a human-readable representation of this error.</summary>
    public override string ToString() => $"[{Type}] {Code}: {Message}";
}
