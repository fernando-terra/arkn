using Arkn.Results;

namespace Arkn.Results.Tests;

public class ErrorTests
{
    [Fact]
    public void Error_NotFound_ShouldHaveCorrectType()
    {
        var error = Error.NotFound("User.NotFound", "User not found");
        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.Equal("User.NotFound", error.Code);
        Assert.Equal("User not found", error.Message);
    }

    [Fact]
    public void Error_Validation_ShouldHaveCorrectType()
    {
        var error = Error.Validation("User.InvalidEmail", "Email is invalid");
        Assert.Equal(ErrorType.Validation, error.Type);
    }

    [Fact]
    public void Error_Conflict_ShouldHaveCorrectType()
    {
        var error = Error.Conflict("User.AlreadyExists", "User already exists");
        Assert.Equal(ErrorType.Conflict, error.Type);
    }

    [Fact]
    public void Error_Unauthorized_ShouldHaveCorrectType()
    {
        var error = Error.Unauthorized("Auth.NotAuthenticated", "Not authenticated");
        Assert.Equal(ErrorType.Unauthorized, error.Type);
    }

    [Fact]
    public void Error_None_ShouldBeEmptyFailure()
    {
        Assert.Equal(string.Empty, Error.None.Code);
        Assert.Equal(ErrorType.Failure, Error.None.Type);
    }

    [Fact]
    public void Error_ToString_ShouldIncludeTypeCodeAndMessage()
    {
        var error = Error.NotFound("X.Y", "msg");
        var str = error.ToString();
        Assert.Contains("NotFound", str);
        Assert.Contains("X.Y", str);
        Assert.Contains("msg", str);
    }
}
