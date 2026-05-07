using Arkn.MCP.Tools;
using Xunit;

namespace Arkn.MCP.Tests;

public class RefactorToResultTests
{
    [Fact]
    public void RefactorToResult_ShouldGenerateErrorGroupClass()
    {
        var code   = "public class OrderService { public void Cancel() { throw new InvalidOperationException(\"already cancelled\"); } }";
        var result = RefactorTools.RefactorToResult(code, "Order");

        Assert.Contains("[ArknErrors]", result);
        Assert.Contains("OrderErrors", result);
    }

    [Fact]
    public void RefactorToResult_ShouldIncludeArknResultsUsing()
    {
        var code   = "public void Process() { throw new ArgumentNullException(\"id\"); }";
        var result = RefactorTools.RefactorToResult(code);

        Assert.Contains("using Arkn.Results;", result);
    }

    [Fact]
    public void RefactorToResult_ShouldReplaceThrowWithErrorGroupCall()
    {
        var code   = "public void Cancel() { throw new InvalidOperationException(\"already cancelled\"); }";
        var result = RefactorTools.RefactorToResult(code, "Order");

        // The refactored method section should contain the ErrorGroup call, not the original throw
        // Get only the refactored code section — stop before the changes-applied comments
        var step2    = result.Split("STEP 2")[^1];
        var codeOnly = step2.Split("// ── Changes applied")[0];
        Assert.Contains("OrderErrors.", codeOnly);
        Assert.DoesNotContain("throw new InvalidOperationException", codeOnly);
    }

    [Fact]
    public void RefactorToResult_ShouldInferDomainFromClassName()
    {
        var code   = "public class PaymentService { public void Process() { } }";
        var result = RefactorTools.RefactorToResult(code); // no explicit domain

        Assert.Contains("PaymentErrors", result);
    }

    [Fact]
    public void RefactorToResult_EmptyCode_ShouldReturnError()
    {
        var result = RefactorTools.RefactorToResult("   ");
        Assert.StartsWith("Error:", result);
    }

    [Fact]
    public void RefactorToResult_CodeWithNoThrows_ShouldStillGenerateErrorGroup()
    {
        var code   = "public class UserService { public Result<User> GetUser(Guid id) { return Result.Success(new User()); } }";
        var result = RefactorTools.RefactorToResult(code, "User");

        Assert.Contains("[ArknErrors]", result);
        Assert.Contains("UserErrors", result);
    }

    [Fact]
    public void ClassifyException_ArgumentNullException_ShouldReturnValidation()
    {
        var (_, errorType) = RefactorTools.ClassifyException("ArgumentNullException");
        Assert.Equal("Validation", errorType);
    }

    [Fact]
    public void ClassifyException_KeyNotFoundException_ShouldReturnNotFound()
    {
        var (_, errorType) = RefactorTools.ClassifyException("KeyNotFoundException");
        Assert.Equal("NotFound", errorType);
    }

    [Fact]
    public void ClassifyException_UnauthorizedAccessException_ShouldReturnUnauthorized()
    {
        var (_, errorType) = RefactorTools.ClassifyException("UnauthorizedAccessException");
        Assert.Equal("Unauthorized", errorType);
    }
}
