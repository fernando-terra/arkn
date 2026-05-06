using Arkn.MCP.Tools;
using System.Text.Json;

namespace Arkn.MCP.Tests;

public class ValidatePatternTests
{
    private static List<JsonElement> Validate(string code)
    {
        var json = ValidateTools.ValidatePattern(code);
        return JsonSerializer.Deserialize<List<JsonElement>>(json)!;
    }

    [Fact]
    public void ARK002_InvalidCode_ShouldDetectViolation()
    {
        var code = """Error.NotFound("usernotfound", "msg");""";
        var violations = Validate(code);
        Assert.Contains(violations, v => v.GetProperty("Rule").GetString() == "ARK002");
    }

    [Fact]
    public void ARK002_ValidCode_ShouldNotDetect()
    {
        var code = """Error.NotFound("User.NotFound", "msg");""";
        var violations = Validate(code);
        Assert.DoesNotContain(violations, v => v.GetProperty("Rule").GetString() == "ARK002");
    }

    [Fact]
    public void ARK004_WrongReturnType_ShouldDetect()
    {
        var code = "public Task ExecuteAsync(ArknJobContext ctx) { }";
        var violations = Validate(code);
        Assert.Contains(violations, v => v.GetProperty("Rule").GetString() == "ARK004");
    }

    [Fact]
    public void ARK004_CorrectReturnType_ShouldNotDetect()
    {
        var code = "public Task<Result> ExecuteAsync(ArknJobContext ctx) { }";
        var violations = Validate(code);
        Assert.DoesNotContain(violations, v => v.GetProperty("Rule").GetString() == "ARK004");
    }

    [Fact]
    public void ARK005_NewHttpClient_ShouldDetect()
    {
        var code = "var client = new HttpClient();";
        var violations = Validate(code);
        Assert.Contains(violations, v => v.GetProperty("Rule").GetString() == "ARK005");
    }

    [Fact]
    public void ARK007_ThrowNew_ShouldDetect()
    {
        var code = "throw new InvalidOperationException(\"oops\");";
        var violations = Validate(code);
        Assert.Contains(violations, v => v.GetProperty("Rule").GetString() == "ARK007");
    }

    [Fact]
    public void EmptyCode_ShouldReturnEmptyArray()
    {
        var violations = Validate("");
        Assert.Empty(violations);
    }

    [Fact]
    public void ARK006_ConsoleWriteLine_ShouldDetect()
    {
        var code = "Console.WriteLine(\"hello\");";
        var violations = Validate(code);
        Assert.Contains(violations, v => v.GetProperty("Rule").GetString() == "ARK006");
    }
}
