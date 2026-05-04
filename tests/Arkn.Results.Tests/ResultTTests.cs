using Arkn.Results;

namespace Arkn.Results.Tests;

public class ResultTTests
{
    // -------------------------------------------------------------------------
    // Ok
    // -------------------------------------------------------------------------

    [Fact]
    public void ResultT_Ok_IsSuccessWithValue()
    {
        var result = Result<int>.Ok(42);
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ResultT_Ok_NullValue_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Result<string>.Ok(null!));
    }

    // -------------------------------------------------------------------------
    // Fail
    // -------------------------------------------------------------------------

    [Fact]
    public void ResultT_Fail_SingleError_IsFailure()
    {
        var error = Error.NotFound("USER_NF", "User not found.");
        var result = Result<int>.Fail(error);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public void ResultT_Fail_MultipleErrors_ContainsAll()
    {
        var errors = new[] { Error.Validation("V1", "E1."), Error.Validation("V2", "E2.") };
        var result = Result<int>.Fail(errors);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void ResultT_Fail_AccessValue_Throws()
    {
        var result = Result<int>.Fail(Error.Failure("E", "Err."));
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    // -------------------------------------------------------------------------
    // Map
    // -------------------------------------------------------------------------

    [Fact]
    public void ResultT_Map_OnSuccess_TransformsValue()
    {
        var result = Result<int>.Ok(5).Map(v => v * 2);
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value);
    }

    [Fact]
    public void ResultT_Map_OnFailure_PropagatesErrors()
    {
        var error = Error.Failure("E", "Err.");
        var result = Result<int>.Fail(error).Map(v => v * 2);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public void ResultT_Map_NullMapper_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Result<int>.Ok(1).Map<string>(null!));
    }

    // -------------------------------------------------------------------------
    // Bind (typed -> typed)
    // -------------------------------------------------------------------------

    [Fact]
    public void ResultT_Bind_ToTyped_OnSuccess_ReturnsNextResult()
    {
        var result = Result<int>.Ok(3).Bind(v => Result<string>.Ok($"value:{v}"));
        Assert.True(result.IsSuccess);
        Assert.Equal("value:3", result.Value);
    }

    [Fact]
    public void ResultT_Bind_ToTyped_OnFailure_ShortCircuits()
    {
        var error = Error.Failure("E", "Err.");
        var called = false;
        var result = Result<int>.Fail(error).Bind(v => { called = true; return Result<string>.Ok("x"); });
        Assert.False(called);
        Assert.True(result.IsFailure);
    }

    // -------------------------------------------------------------------------
    // Bind (typed -> void)
    // -------------------------------------------------------------------------

    [Fact]
    public void ResultT_Bind_ToVoid_OnSuccess_InvokesNext()
    {
        var called = false;
        var result = Result<int>.Ok(1).Bind(v => { called = true; return Result.Ok(); });
        Assert.True(called);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ResultT_Bind_ToVoid_OnFailure_ShortCircuits()
    {
        var error = Error.Failure("E", "Err.");
        var called = false;
        var result = Result<int>.Fail(error).Bind(v => { called = true; return Result.Ok(); });
        Assert.False(called);
        Assert.True(result.IsFailure);
    }

    // -------------------------------------------------------------------------
    // Match
    // -------------------------------------------------------------------------

    [Fact]
    public void ResultT_Match_OnSuccess_CallsSuccessBranch()
    {
        var msg = Result<int>.Ok(7).Match(v => $"got {v}", _ => "fail");
        Assert.Equal("got 7", msg);
    }

    [Fact]
    public void ResultT_Match_OnFailure_CallsFailureBranch()
    {
        var msg = Result<int>.Fail(Error.Failure("E", "Err.")).Match(v => "ok", _ => "fail");
        Assert.Equal("fail", msg);
    }

    // -------------------------------------------------------------------------
    // Implicit conversions
    // -------------------------------------------------------------------------

    [Fact]
    public void ResultT_ImplicitFromValue_IsSuccess()
    {
        Result<int> result = 99;
        Assert.True(result.IsSuccess);
        Assert.Equal(99, result.Value);
    }

    [Fact]
    public void ResultT_ImplicitFromError_IsFailure()
    {
        Result<int> result = Error.NotFound("NF", "Not found.");
        Assert.True(result.IsFailure);
    }

    // -------------------------------------------------------------------------
    // Chained pipeline
    // -------------------------------------------------------------------------

    [Fact]
    public void ResultT_Chain_Map_Bind_Match_ReturnsCorrectValue()
    {
        var output = Result<int>.Ok(10)
            .Map(v => v + 5)                                        // 15
            .Bind(v => Result<string>.Ok(v.ToString()))             // "15"
            .Match(v => $"Result: {v}", _ => "Error");

        Assert.Equal("Result: 15", output);
    }

    [Fact]
    public void ResultT_Chain_FailEarly_SkipsRemainingSteps()
    {
        var mapCalled = false;

        var output = Result<int>.Fail(Error.Failure("E", "Err."))
            .Map(v => { mapCalled = true; return v + 5; })
            .Match(v => "ok", _ => "fail");

        Assert.False(mapCalled);
        Assert.Equal("fail", output);
    }

    // -------------------------------------------------------------------------
    // ToString
    // -------------------------------------------------------------------------

    [Fact]
    public void ResultT_Ok_ToString_ContainsSuccessAndValue()
    {
        var str = Result<int>.Ok(42).ToString();
        Assert.Contains("Success", str);
        Assert.Contains("42", str);
    }

    [Fact]
    public void ResultT_Fail_ToString_ContainsFailure()
    {
        var str = Result<int>.Fail(Error.Failure("E", "Err.")).ToString();
        Assert.Contains("Failure", str);
    }

    // -------------------------------------------------------------------------
    // Multiple validation errors
    // -------------------------------------------------------------------------

    [Fact]
    public void ResultT_MultipleValidationErrors_AllPreserved()
    {
        var errors = new Error[]
        {
            Error.Validation("VAL.NAME", "Name is required."),
            Error.Validation("VAL.EMAIL", "Invalid email format."),
            Error.Validation("VAL.AGE", "Age must be positive.")
        };

        var result = Result<object>.Fail(errors);

        Assert.True(result.IsFailure);
        Assert.Equal(3, result.Errors.Count);
        Assert.All(result.Errors, e => Assert.Equal(ErrorType.Validation, e.Type));
    }
}
