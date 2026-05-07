using Arkn.MCP.Tools;
using Xunit;

namespace Arkn.MCP.Tests;

public class MigrateExceptionCatchTests
{
    [Fact]
    public void MigrateException_Rethrow_ShouldReturnResultFailure()
    {
        var catchBlock = "catch (InvalidOperationException ex) { throw; }";
        var result     = MigrateTools.MigrateException(catchBlock, "Order.InvalidOperation");

        Assert.Contains("Result.Failure", result);
        Assert.Contains("Order.InvalidOperation", result);
    }

    [Fact]
    public void MigrateException_EmptyCatch_ShouldSurfaceFailure()
    {
        var catchBlock = "catch (Exception ex) { }";
        var result     = MigrateTools.MigrateException(catchBlock);

        Assert.Contains("Result.Failure", result);
        Assert.Contains("ex.Message", result);
    }

    [Fact]
    public void MigrateException_LogOnlyCatch_ShouldPreserveLog()
    {
        var catchBlock = "catch (Exception ex) { _logger.LogError(ex, \"failed\"); }";
        var result     = MigrateTools.MigrateException(catchBlock);

        Assert.Contains("_logger.LogError", result);
        Assert.Contains("Result.Failure", result);
    }

    [Fact]
    public void MigrateException_ShouldSelectCorrectErrorType_ForArgumentNull()
    {
        var catchBlock = "catch (ArgumentNullException ex) { throw; }";
        var result     = MigrateTools.MigrateException(catchBlock);

        Assert.Contains("Error.Validation", result);
    }

    [Fact]
    public void MigrateException_ShouldSelectCorrectErrorType_ForUnauthorized()
    {
        var catchBlock = "catch (UnauthorizedAccessException ex) { throw; }";
        var result     = MigrateTools.MigrateException(catchBlock);

        Assert.Contains("Error.Unauthorized", result);
    }

    [Fact]
    public void MigrateException_ShouldSelectCorrectErrorType_ForKeyNotFound()
    {
        var catchBlock = "catch (KeyNotFoundException ex) { throw; }";
        var result     = MigrateTools.MigrateException(catchBlock);

        Assert.Contains("Error.NotFound", result);
    }

    [Fact]
    public void MigrateException_CustomErrorCode_ShouldUseProvidedCode()
    {
        var catchBlock = "catch (Exception ex) { throw; }";
        var result     = MigrateTools.MigrateException(catchBlock, "Payment.ProcessingFailed");

        Assert.Contains("\"Payment.ProcessingFailed\"", result);
    }

    [Fact]
    public void MigrateException_ShouldIncludeExplanationComment()
    {
        var catchBlock = "catch (ArgumentNullException ex) { throw; }";
        var result     = MigrateTools.MigrateException(catchBlock);

        Assert.Contains("Why this ErrorType", result);
        Assert.Contains("ARK002", result);
    }

    [Fact]
    public void MigrateException_EmptyCatchBlock_ShouldReturnError()
    {
        var result = MigrateTools.MigrateException("   ");
        Assert.StartsWith("Error:", result);
    }

    [Fact]
    public void MigrateException_ShouldIncludeBeforeAfterSummary()
    {
        var catchBlock = "catch (Exception ex) { }";
        var result     = MigrateTools.MigrateException(catchBlock);

        Assert.Contains("Before", result);
        Assert.Contains("After", result);
    }
}
