namespace Arkn.Core.Abstractions;

/// <summary>
/// Represents the outcome of an operation that may succeed or fail.
/// </summary>
public interface IResult
{
    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    bool IsFailure { get; }

    /// <summary>Gets the collection of errors when the operation fails.</summary>
    IReadOnlyList<IError> Errors { get; }
}

/// <summary>
/// Represents the outcome of an operation that may succeed with a value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public interface IResult<out T> : IResult
{
    /// <summary>
    /// Gets the value when the operation succeeded.
    /// Throws <see cref="InvalidOperationException"/> when accessed on a failed result.
    /// </summary>
    T Value { get; }
}
