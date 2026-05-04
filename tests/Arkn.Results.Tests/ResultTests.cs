using Arkn.Results;

namespace Arkn.Results.Tests;

public class ResultTests
{
    // ── Result (no value) ─────────────────────────────────────────────────────

    [Fact]
    public void Result_Success_IsSuccessTrue()
    {
        var result = Result.Success();
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Result_Failure_IsFailureTrue()
    {
        var error = Error.NotFound("X", "msg");
        var result = Result.Failure(error);
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Result_Success_WithError_ShouldThrow()
    {
        var ex = Assert.ThrowsAny<Exception>(() =>
        {
            _ = typeof(Result)
                .GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)[0]
                .Invoke([true, Error.NotFound("X", "Y")]);
        });
        // Reflection wraps exceptions in TargetInvocationException
        var inner = ex.InnerException ?? ex;
        Assert.IsType<InvalidOperationException>(inner);
    }

    // ── Result<T> ─────────────────────────────────────────────────────────────

    [Fact]
    public void ResultT_Success_ShouldCarryValue()
    {
        var result = Result.Success(42);
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ResultT_Failure_AccessingValue_ShouldThrow()
    {
        var result = Result.Failure<int>(Error.NotFound("X", "msg"));
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void ResultT_Map_OnSuccess_ShouldTransformValue()
    {
        var result = Result.Success(5).Map(x => x * 2);
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value);
    }

    [Fact]
    public void ResultT_Map_OnFailure_ShouldPropagateError()
    {
        var error = Error.NotFound("X", "msg");
        var result = Result.Failure<int>(error).Map(x => x * 2);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void ResultT_Bind_OnSuccess_ShouldChain()
    {
        var result = Result.Success(5)
            .Bind(x => x > 0 ? Result.Success(x.ToString()) : Result.Failure<string>(Error.Validation("V", "msg")));
        Assert.True(result.IsSuccess);
        Assert.Equal("5", result.Value);
    }

    [Fact]
    public void ResultT_Bind_OnFailure_ShouldPropagateError()
    {
        var error = Error.NotFound("X", "msg");
        var result = Result.Failure<int>(error)
            .Bind(x => Result.Success(x.ToString()));
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void ResultT_Match_OnSuccess_ShouldCallOnSuccess()
    {
        var result = Result.Success(42);
        var output = result.Match(v => $"ok:{v}", e => $"err:{e.Code}");
        Assert.Equal("ok:42", output);
    }

    [Fact]
    public void ResultT_Match_OnFailure_ShouldCallOnFailure()
    {
        var error = Error.NotFound("User.NotFound", "msg");
        var result = Result.Failure<int>(error);
        var output = result.Match(v => $"ok:{v}", e => $"err:{e.Code}");
        Assert.Equal("err:User.NotFound", output);
    }

    [Fact]
    public void ResultT_ImplicitConversion_FromValue_ShouldSucceed()
    {
        Result<string> result = "hello";
        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void ResultT_ImplicitConversion_FromError_ShouldFail()
    {
        Result<string> result = Error.NotFound("X", "msg");
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ResultT_MultipleErrors_ShouldCarryAll()
    {
        var errors = new[]
        {
            Error.Validation("A", "err1"),
            Error.Validation("B", "err2"),
        };
        var result = Result.Failure<string>(errors);
        Assert.True(result.IsFailure);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void ResultT_Ensure_FailsWhenPredicateFalse()
    {
        var result = Result.Success(3)
            .Ensure(x => x > 10, Error.Validation("V", "must be > 10"));
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ResultT_Ensure_PassesWhenPredicateTrue()
    {
        var result = Result.Success(15)
            .Ensure(x => x > 10, Error.Validation("V", "must be > 10"));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ResultT_Tap_ShouldExecuteSideEffectOnSuccess()
    {
        var called = false;
        Result.Success(1).Tap(_ => called = true);
        Assert.True(called);
    }

    [Fact]
    public void ResultT_Tap_ShouldNotExecuteOnFailure()
    {
        var called = false;
        Result.Failure<int>(Error.NotFound("X", "msg")).Tap(_ => called = true);
        Assert.False(called);
    }

    // ── Async extensions ──────────────────────────────────────────────────────

    [Fact]
    public async Task ResultT_BindAsync_ShouldChain()
    {
        var result = await Task.FromResult(Result.Success(5))
            .BindAsync(x => Task.FromResult(Result.Success(x * 2)));
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value);
    }

    [Fact]
    public async Task ResultT_MatchAsync_ShouldCallCorrectBranch()
    {
        var output = await Task.FromResult(Result.Success(7))
            .MatchAsync(
                onSuccess: v => Task.FromResult($"ok:{v}"),
                onFailure: e => Task.FromResult($"err:{e.Code}"));
        Assert.Equal("ok:7", output);
    }
}
