using Arkn.MCP.Tools;
using Xunit;

namespace Arkn.MCP.Tests;

public class MigrateExceptionTests
{
    [Fact]
    public void MigrateExceptionToResult_ShouldAddUsingArknResults()
    {
        var code   = "public void Process() { throw new InvalidOperationException(\"bad\"); }";
        var result = MigrateTools.MigrateExceptionToResult(code);
        Assert.Contains("using Arkn.Results", result);
    }

    [Fact]
    public void MigrateExceptionToResult_ShouldReplaceThrowWithResultFailure()
    {
        var code   = "public void Process() { throw new InvalidOperationException(\"bad state\"); }";
        var result = MigrateTools.MigrateExceptionToResult(code);
        Assert.Contains("Result.Failure", result);
        // Check only the generated code section — not the changes-applied comments at the bottom
        var codeSection = result.Split("// ── Changes applied")[0];
        Assert.DoesNotContain("throw new InvalidOperationException", codeSection);
    }

    [Fact]
    public void MigrateExceptionToResult_EmptyCode_ShouldReturnError()
    {
        var result = MigrateTools.MigrateExceptionToResult("");
        Assert.StartsWith("Error:", result);
    }

    [Fact]
    public void MigrateHttpClientToArkn_ShouldGenerateTypedClient()
    {
        var code   = "public class PaymentService { private readonly HttpClient _http; public async Task<string> GetAsync() { return await _http.GetAsync(\"https://api.pay.com/payments\"); } }";
        var result = MigrateTools.MigrateHttpClientToArkn(code);
        Assert.Contains("ArknHttpClient", result);
        Assert.Contains("IArknHttp", result);
    }

    [Fact]
    public void MigrateHttpClientToArkn_ShouldTransformGetAsync()
    {
        var code   = "class UserClient { void Test() { client.GetAsync(\"/users\"); } }";
        var result = MigrateTools.MigrateHttpClientToArkn(code);
        Assert.Contains("GetAs<", result);
    }

    [Fact]
    public void MigrateHttpClientToArkn_ShouldIncludeDiRegistration()
    {
        var code   = "class PayClient { HttpClient http; }";
        var result = MigrateTools.MigrateHttpClientToArkn(code);
        Assert.Contains("AddArknHttp", result);
    }
}
