using Arkn.MCP.Tools;

namespace Arkn.MCP.Tests;

public class ScaffoldErrorsTests
{
    [Fact]
    public void ScaffoldErrors_ShouldContainDomainName()
    {
        var result = ScaffoldTools.ScaffoldErrors("User");
        Assert.Contains("UserErrors", result);
    }

    [Fact]
    public void ScaffoldErrors_ShouldContainArknErrorsAttribute()
    {
        var result = ScaffoldTools.ScaffoldErrors("Payment");
        Assert.Contains("[ArknErrors]", result);
    }

    [Fact]
    public void ScaffoldErrors_ShouldContainAllFourStandardErrors()
    {
        var result = ScaffoldTools.ScaffoldErrors("Invoice");
        Assert.Contains("NotFound", result);
        Assert.Contains("Invalid", result);
        Assert.Contains("Conflict", result);
        Assert.Contains("Unauthorized", result);
    }

    [Fact]
    public void ScaffoldErrors_ShouldCapitalizeDomainName()
    {
        var result = ScaffoldTools.ScaffoldErrors("user");
        Assert.Contains("UserErrors", result);
    }

    [Fact]
    public void ScaffoldErrors_EmptyDomain_ShouldReturnError()
    {
        var result = ScaffoldTools.ScaffoldErrors("");
        Assert.StartsWith("Error:", result);
    }

    [Fact]
    public void ScaffoldJob_ShouldContainJobName()
    {
        var result = ScaffoldTools.ScaffoldJob("InvoiceProcessor", "0 2 * * *");
        Assert.Contains("InvoiceProcessorJob", result);
    }

    [Fact]
    public void ScaffoldJob_ShouldContainCronExpression()
    {
        var result = ScaffoldTools.ScaffoldJob("Report", "0 8 * * 1", "Weekly report");
        Assert.Contains("0 8 * * 1", result);
    }

    [Fact]
    public void ScaffoldJob_ShouldReturnTaskResult()
    {
        var result = ScaffoldTools.ScaffoldJob("Sample", "* * * * *");
        Assert.Contains("Task<Result>", result);
    }

    [Fact]
    public void ScaffoldHttpClient_ShouldContainClientName()
    {
        var result = ScaffoldTools.ScaffoldHttpClient("Payment", "https://api.pay.com", "GetPayment,CreatePayment");
        Assert.Contains("PaymentClient", result);
    }

    [Fact]
    public void ScaffoldHttpClient_ShouldContainBaseUrl()
    {
        var result = ScaffoldTools.ScaffoldHttpClient("User", "https://api.users.com");
        Assert.Contains("https://api.users.com", result);
    }
}
