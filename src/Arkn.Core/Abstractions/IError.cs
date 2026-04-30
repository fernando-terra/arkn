namespace Arkn.Core.Abstractions;

/// <summary>
/// Represents a structured error produced by an operation.
/// </summary>
public interface IError
{
    /// <summary>Gets the error code — a machine-readable short identifier.</summary>
    string Code { get; }

    /// <summary>Gets the human-readable description of the error.</summary>
    string Message { get; }

    /// <summary>Gets the semantic type of the error.</summary>
    ErrorType Type { get; }

    /// <summary>
    /// Gets an optional dictionary of additional metadata about the error.
    /// </summary>
    IReadOnlyDictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Semantic classification of an error, used for HTTP mapping and domain intent.
/// </summary>
public enum ErrorType
{
    /// <summary>Generic, unclassified failure.</summary>
    Failure = 0,

    /// <summary>The requested resource was not found.</summary>
    NotFound = 1,

    /// <summary>One or more validation rules were violated.</summary>
    Validation = 2,

    /// <summary>The operation conflicts with the current state of the resource.</summary>
    Conflict = 3,

    /// <summary>The caller is not authorized to perform this operation.</summary>
    Unauthorized = 4
}
