using Arkn.MCP.Tools;
using System.Text.Json;
using Xunit;

namespace Arkn.MCP.Tests;

public class ProjectHealthTests
{
    [Fact]
    public void ProjectHealth_CleanCode_ShouldScore100()
    {
        var code   = "public Task<Result> DoSomethingAsync() => Task.FromResult(Result.Success());";
        var result = AnalysisTools.ProjectHealth([code]);
        var doc    = JsonDocument.Parse(result);
        Assert.Equal(100, doc.RootElement.GetProperty("score").GetInt32());
        Assert.Equal(0, doc.RootElement.GetProperty("totalViolations").GetInt32());
    }

    [Fact]
    public void ProjectHealth_WithViolations_ShouldReduceScore()
    {
        var code   = "throw new Exception(\"bad\"); throw new Exception(\"bad\"); throw new Exception(\"bad\");";
        var result = AnalysisTools.ProjectHealth([code]);
        var doc    = JsonDocument.Parse(result);
        Assert.True(doc.RootElement.GetProperty("score").GetInt32() < 100);
        Assert.True(doc.RootElement.GetProperty("totalViolations").GetInt32() > 0);
    }

    [Fact]
    public void ProjectHealth_EmptyFiles_ShouldReturn100()
    {
        var result = AnalysisTools.ProjectHealth([]);
        var doc    = JsonDocument.Parse(result);
        Assert.Equal(100, doc.RootElement.GetProperty("score").GetInt32());
    }

    [Fact]
    public void ProjectHealth_VerdictField_ShouldBePresent()
    {
        var result = AnalysisTools.ProjectHealth(["// clean"]);
        var doc    = JsonDocument.Parse(result);
        Assert.True(doc.RootElement.TryGetProperty("verdict", out _));
    }
}
