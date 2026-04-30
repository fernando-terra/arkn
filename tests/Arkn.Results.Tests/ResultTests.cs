using Arkn.Core.Abstractions;
using Arkn.Results;

namespace Arkn.Results.Tests;

public class ResultTests
{
    // -------------------------------------------------------------------------
    // Result (void) — Ok
    // -------------------------------------------------------------------------

    [Fact]
    public void Result_Ok_IsSuccess()
    {
        var result = Result.Ok();
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Empty(result.Errors);
    }

    // -------------------------------------------------------------------------
    // Result (void) — Fail
    // -------------------------------------------------------------------------

    [Fact]
    public void Result_Fail_SingleError_IsFailure()
    {
        var error = Error.Failure("ERR", "Failed.");
        var result = Result.Fail(error);
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public void Result_Fail_MultipleErrors_ContainsAll()
    {
        var errors = new[] { Error.Validation("V1", "E1."), Error.Validation("V2", "E2.") };
        var result = Result.Fail(errors);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void Result_Fail_EmptyCollection_Throws()
    {
        Assert.Throws<ArgumentException>(() => Result.Fail(Array.Empty<IError>()));
    }

    [Fact]
    public void Result_Fail_NullError_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Result.Fail((IError)null!));
    }

    // -------------------------------------------------------------------------
    // Result (void) — FirstError sentinel
    // -------------------------------------------------------------------------

    [Fact]
    public void Result_Ok_FirstError_ReturnsNone()
    {
        var result = Result.Ok();
        Assert.Equal(Error.None, result.FirstError);
    }

    // -------------------------------------------------------------------------
    // Result (void) — Bind
    // -------------------------------------------------------------------------

    [Fact]
    public void Result_Bind_OnSuccess_InvokesNext()
    {
        var invoked = false;
        var result = Result.Ok().Bind(() => { invoked = true; return Result.Ok(); });
        Assert.True(invoked);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Result_Bind_OnFailure_SkipsNext()
    {
        var invoked = false;
        var error = Error.Failure("E", "Err.");
        var result = Result.Fail(error).Bind(() => { invoked = true; return Result.Ok(); });
        Assert.False(invoked);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Result_Bind_ToTyped_OnSuccess_ReturnsTypedResult()
    {
        var result = Result.Ok().Bind(() => Result.Ok(42));
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Result_Bind_ToTyped_OnFailure_PropagatesErrors()
    {
        var error = Error.Failure("E", "Err.");
        var result = Result.Fail(error).Bind(() => Result.Ok(42));
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    // -------------------------------------------------------------------------
    // Result (void) — Match
    // -------------------------------------------------------------------------

    [Fact]
    public void Result_Match_OnSuccess_CallsSuccessBranch()
    {
        var result = Result.Ok().Match(() => "ok", _ => "fail");
        Assert.Equal("ok", result);
    }

    [Fact]
    public void Result_Match_OnFailure_CallsFailureBranch()
    {
        var result = Result.Fail(Error.Failure("E", "Err.")).Match(() => "ok", _ => "fail");
        Assert.Equal("fail", result);
    }

    // -------------------------------------------------------------------------
    // ToString
    // -------------------------------------------------------------------------

    [Fact]
    public void Result_Ok_ToString_ContainsSuccess()
        => Assert.Contains("Success", Result.Ok().ToString());

    [Fact]
    public void Result_Fail_ToString_ContainsFailure()
        => Assert.Contains("Failure", Result.Fail(Error.Failure("E", "Err.")).ToString());
}
