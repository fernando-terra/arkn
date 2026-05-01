namespace Arkn.Logging.Abstractions;

/// <summary>
/// Structured key-value context attached to log entries.
/// Typically represents the ambient context of an operation (e.g., a Job run).
/// </summary>
public interface IArknLogContext
{
    /// <summary>Optional named scope (e.g., a Job RunId). Used to isolate log streams.</summary>
    string? Scope { get; }

    /// <summary>Returns all key-value pairs in this context.</summary>
    IReadOnlyDictionary<string, object?> Properties { get; }

    /// <summary>Returns a new context with an additional key-value pair.</summary>
    IArknLogContext With(string key, object? value);
}
