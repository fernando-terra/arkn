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

    /// <summary>Creates a generic failure error.</summary>
    public static Error Failure(string code, string message) =>
        new(code, message, ErrorType.Failure);

    /// <summary>Creates a not-found error.</summary>
    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    /// <summary>Creates a validation error.</summary>
    public static Error Validation(string code, string message) =>
        new(code, message, ErrorType.Validation);

    /// <summary>Creates a conflict error.</summary>
    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);

    /// <summary>Creates an unauthorized error.</summary>
    public static Error Unauthorized(string code, string message) =>
        new(code, message, ErrorType.Unauthorized);

    /// <summary>Creates a forbidden error.</summary>
    public static Error Forbidden(string code, string message) =>
        new(code, message, ErrorType.Forbidden);

    public override string ToString() => $"[{Type}] {Code}: {Message}";
}
