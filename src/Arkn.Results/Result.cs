namespace Arkn.Results;

/// <summary>
/// Represents the outcome of an operation that may succeed or fail.
/// Carries no value — use <see cref="Result{T}"/> when a value is expected on success.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot carry an error.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failed result must carry an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>Gets whether this result represents a success.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets whether this result represents a failure.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The error associated with this result.
    /// <see cref="Error.None"/> when <see cref="IsSuccess"/> is <c>true</c>.
    /// </summary>
    public Error Error { get; }

    // ── Factories ──────────────────────────────────────────────────────────────

    /// <summary>Creates a successful result.</summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>Shorthand for <see cref="Success()"/>.</summary>
    public static Result Ok() => Success();

    /// <summary>Creates a failed result.</summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>Creates a successful result carrying <paramref name="value"/>.</summary>
    public static Result<T> Success<T>(T value) => new(value, true, Error.None);

    /// <summary>Creates a failed result of type <typeparamref name="T"/>.</summary>
    public static Result<T> Failure<T>(Error error) => new(default, false, error);

    /// <summary>Creates a failed result of type <typeparamref name="T"/> with multiple errors.</summary>
    public static Result<T> Failure<T>(IEnumerable<Error> errors)
    {
        var list = errors.ToArray();
        if (list.Length == 0) throw new ArgumentException("At least one error is required.", nameof(errors));
        return new(default, false, list[0], list);
    }

    // ── Implicit conversions ───────────────────────────────────────────────────

    public static implicit operator Result(Error error) => Failure(error);

    // ── Match ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Executes <paramref name="onSuccess"/> or <paramref name="onFailure"/> based on state.
    /// </summary>
    public TOut Match<TOut>(Func<TOut> onSuccess, Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Error);

    public override string ToString() =>
        IsSuccess ? "Success" : $"Failure({Error})";
}

/// <summary>
/// Represents the outcome of an operation that may succeed with a value of type
/// <typeparamref name="T"/>, or fail with one or more <see cref="Error"/>s.
/// </summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;
    private readonly IReadOnlyList<Error>? _errors;

    internal Result(T? value, bool isSuccess, Error error, IReadOnlyList<Error>? errors = null)
        : base(isSuccess, error)
    {
        _value = value;
        _errors = errors;
    }

    /// <summary>
    /// The value carried by this result.
    /// Throws <see cref="InvalidOperationException"/> if accessed on a failure.
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    /// <summary>
    /// All errors associated with this result.
    /// For single-error results, contains exactly one element.
    /// </summary>
    public IReadOnlyList<Error> Errors =>
        _errors ?? (IsFailure ? [Error] : []);

    /// <summary>The first error. Equivalent to <see cref="Result.Error"/> for single-error results.</summary>
    public Error FirstError => IsFailure
        ? (_errors?[0] ?? Error)
        : throw new InvalidOperationException("Cannot access error of a successful result.");

    // ── Functional operations ──────────────────────────────────────────────────

    /// <summary>
    /// Projects the value using <paramref name="mapper"/> if successful;
    /// propagates the error otherwise.
    /// </summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return IsSuccess ? Result.Success(mapper(Value)) : Result.Failure<TOut>(Error);
    }

    /// <summary>
    /// Chains into another result-returning operation if successful;
    /// propagates the error otherwise.
    /// </summary>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> binder) =>
        IsSuccess ? binder(Value) : Result.Failure<TOut>(Error);

    /// <summary>
    /// Chains into a non-value result-returning operation if successful;
    /// propagates the error otherwise.
    /// </summary>
    public Result Bind(Func<T, Result> binder) =>
        IsSuccess ? binder(Value) : Result.Failure(Error);

    /// <summary>
    /// Async version of <see cref="Bind{TOut}"/>.
    /// </summary>
    public async Task<Result<TOut>> BindAsync<TOut>(Func<T, Task<Result<TOut>>> binder) =>
        IsSuccess ? await binder(Value) : Result.Failure<TOut>(Error);

    /// <summary>
    /// Executes <paramref name="onSuccess"/> or <paramref name="onFailure"/> based on state.
    /// </summary>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Error);

    /// <summary>
    /// Executes a side-effect <paramref name="action"/> if successful.
    /// Returns this result unchanged.
    /// </summary>
    public Result<T> Tap(Action<T> action)
    {
        if (IsSuccess) action(Value);
        return this;
    }

    // ── Convenience factories ─────────────────────────────────────────────────

    /// <summary>Shorthand for <see cref="Result.Success{T}(T)"/>.</summary>
    public static Result<T> Ok(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Result.Success(value);
    }

    /// <summary>Shorthand for <see cref="Result.Failure{T}(Error)"/>.</summary>
    public static Result<T> Fail(Error error) => Result.Failure<T>(error);

    /// <summary>Shorthand for <see cref="Result.Failure{T}(IEnumerable{Error})"/>.</summary>
    public static Result<T> Fail(IEnumerable<Error> errors) => Result.Failure<T>(errors);

    // ── Implicit conversions ───────────────────────────────────────────────────

    public static implicit operator Result<T>(T value) => Result.Success(value);
    public static implicit operator Result<T>(Error error) => Result.Failure<T>(error);

    public override string ToString() =>
        IsSuccess ? $"Success({_value})" : $"Failure({Error})";
}
