using Arkn.MCP.Tools;
using Xunit;

namespace Arkn.MCP.Tests;

public class ExplainErrorTests
{
    [Fact]
    public void ExplainError_ValidCode_ShouldReturnExplanation()
    {
        var result = ExplainTools.ExplainError("User.NotFound");

        Assert.Contains("User.NotFound", result);
        Assert.Contains("NotFound", result);
        Assert.Contains("404", result);
    }

    [Fact]
    public void ExplainError_ShouldIncludeCodeExample()
    {
        var result = ExplainTools.ExplainError("Order.Conflict");

        Assert.Contains("```csharp", result);
        Assert.Contains("OrderErrors", result);
    }

    [Fact]
    public void ExplainError_ShouldIncludeHttpStatusMapping()
    {
        var result = ExplainTools.ExplainError("Payment.Unauthorized");

        Assert.Contains("401", result);
    }

    [Fact]
    public void ExplainError_ShouldIncludeUsageExample()
    {
        var result = ExplainTools.ExplainError("Invoice.NotFound");

        Assert.Contains("result.Match", result);
        Assert.Contains("InvoiceErrors", result);
    }

    [Fact]
    public void ExplainError_InvalidFormat_ShouldWarnAboutARK002()
    {
        var result = ExplainTools.ExplainError("usernotfound");

        Assert.Contains("ARK002", result);
        Assert.Contains("PascalCase", result);
    }

    [Fact]
    public void ExplainError_EmptyCode_ShouldReturnError()
    {
        var result = ExplainTools.ExplainError("");
        Assert.StartsWith("Error:", result);
    }

    [Fact]
    public void ExplainError_ValidationReason_ShouldReturn400()
    {
        var result = ExplainTools.ExplainError("User.InvalidEmail");

        Assert.Contains("400", result);
    }

    [Fact]
    public void ExplainError_ShouldIncludeErrorGroupScaffold()
    {
        var result = ExplainTools.ExplainError("Shipment.NotFound");

        Assert.Contains("[ArknErrors]", result);
        Assert.Contains("ShipmentErrors", result);
    }

    [Fact]
    public void ExplainError_ShouldIncludeApiHandlerExample()
    {
        var result = ExplainTools.ExplainError("User.NotFound");

        Assert.Contains("result.Match", result);
        Assert.Contains("Results.NotFound", result);
    }
}
