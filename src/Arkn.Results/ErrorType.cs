namespace Arkn.Results;

/// <summary>Classifies the kind of failure a <see cref="Error"/> represents.</summary>
public enum ErrorType
{
    /// <summary>A generic, unclassified failure.</summary>
    Failure = 0,

    /// <summary>The requested resource was not found.</summary>
    NotFound = 1,

    /// <summary>One or more input values are invalid.</summary>
    Validation = 2,

    /// <summary>A conflict with the current state of the resource.</summary>
    Conflict = 3,

    /// <summary>The caller is not authorized to perform this operation.</summary>
    Unauthorized = 4,

    /// <summary>The operation is forbidden for the current caller.</summary>
    Forbidden = 5,
}
