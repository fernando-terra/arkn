namespace Arkn.Results;

/// <summary>
/// Extension methods for working with <see cref="Result"/> and <see cref="Result{T}"/> in async contexts.
/// </summary>
public static class ResultExtensions
{
    // ── Async Map ─────────────────────────────────────────────────────────────

    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }

    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> mapper)
    {
        var result = await resultTask;
        return result.IsSuccess
            ? Result.Success(await mapper(result.Value))
            : Result.Failure<TOut>(result.Error);
    }

    // ── Async Bind ────────────────────────────────────────────────────────────

    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Result<TOut>> binder)
    {
        var result = await resultTask;
        return result.Bind(binder);
    }

    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> binder)
    {
        var result = await resultTask;
        return await result.BindAsync(binder);
    }

    // ── Async Match ───────────────────────────────────────────────────────────

    public static async Task<TOut> MatchAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        var result = await resultTask;
        return result.Match(onSuccess, onFailure);
    }

    public static async Task<TOut> MatchAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> onSuccess,
        Func<Error, Task<TOut>> onFailure)
    {
        var result = await resultTask;
        return result.IsSuccess
            ? await onSuccess(result.Value)
            : await onFailure(result.Error);
    }

    // ── Ensure ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a failure with <paramref name="error"/> if the predicate is false;
    /// otherwise propagates the result unchanged.
    /// </summary>
    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Error error) =>
        result.IsSuccess && !predicate(result.Value) ? Result.Failure<T>(error) : result;
}
