using Arkn.MCP.Tools;
using Xunit;

namespace Arkn.MCP.Tests;

public class ReviewPatternTests
{
    [Fact]
    public void ReviewPattern_CleanCode_ShouldReturnNoViolations()
    {
        var code   = "var result = GetUser(id); result.Match(onSuccess: u => u, onFailure: e => null);";
        var result = ReviewTools.ReviewPattern(code);

        Assert.Contains("No Arkn pattern violations", result);
    }

    [Fact]
    public void ReviewPattern_WithViolation_ShouldReturnMarkdownHeader()
    {
        var code   = "var client = new HttpClient();";
        var result = ReviewTools.ReviewPattern(code, "PaymentService.cs");

        Assert.Contains("# Code Review — PaymentService.cs", result);
        Assert.Contains("ARK005", result);
    }

    [Fact]
    public void ReviewPattern_ShouldIncludeScore()
    {
        var code   = "var client = new HttpClient(); throw new Exception(\"err\");";
        var result = ReviewTools.ReviewPattern(code);

        Assert.Contains("Score:", result);
        Assert.Contains("/100", result);
    }

    [Fact]
    public void ReviewPattern_ShouldIncludeGrade()
    {
        var code = """
            var client = new HttpClient();
            throw new Exception("e");
            catch(Exception ex) { }
            """;
        var result = ReviewTools.ReviewPattern(code);

        // Grade must appear (A/B/C/D/F)
        Assert.Matches(@"Grade [ABCDF]", result);
    }

    [Fact]
    public void ReviewPattern_ShouldIncludeRecommendations()
    {
        var code   = "throw new Exception(\"error\");";
        var result = ReviewTools.ReviewPattern(code);

        Assert.Contains("Recommended next steps", result);
    }

    [Fact]
    public void ReviewPattern_ARK002Violation_ShouldIncludeLineNumber()
    {
        var code   = "Error.NotFound(\"usernotfound\", \"msg\");";
        var result = ReviewTools.ReviewPattern(code);

        Assert.Contains("Line", result);
        Assert.Contains("ARK002", result);
    }

    [Fact]
    public void ReviewPattern_EmptyCode_ShouldReturnError()
    {
        var result = ReviewTools.ReviewPattern("");
        Assert.StartsWith("Error:", result);
    }

    [Fact]
    public void ReviewPattern_NoFileName_ShouldUseDefaultHeader()
    {
        var code   = "var x = 1;";
        var result = ReviewTools.ReviewPattern(code);

        Assert.Contains("# Code Review", result);
    }

    [Fact]
    public void ReviewPattern_ARK004_ShouldShowErrorSeverity()
    {
        var code   = "public Task ExecuteAsync(ArknJobContext ctx) { }";
        var result = ReviewTools.ReviewPattern(code);

        Assert.Contains("❌", result);
        Assert.Contains("ARK004", result);
    }
}
