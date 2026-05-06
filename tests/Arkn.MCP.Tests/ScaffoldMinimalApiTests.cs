using Arkn.MCP.Tools;
using Xunit;

namespace Arkn.MCP.Tests;

public class ScaffoldMinimalApiTests
{
    [Fact]
    public void ScaffoldMinimalApi_Get_ShouldGenerateGetEndpoint()
    {
        var result = AnalysisTools.ScaffoldMinimalApi("Payment", "get");
        Assert.Contains("MapGet", result);
        Assert.Contains("{id:guid}", result);
    }

    [Fact]
    public void ScaffoldMinimalApi_Create_ShouldGeneratePostEndpoint()
    {
        var result = AnalysisTools.ScaffoldMinimalApi("Payment", "create");
        Assert.Contains("MapPost", result);
        Assert.Contains("Results.Created", result);
    }

    [Fact]
    public void ScaffoldMinimalApi_Delete_ShouldGenerateDeleteEndpoint()
    {
        var result = AnalysisTools.ScaffoldMinimalApi("Payment", "delete");
        Assert.Contains("MapDelete", result);
        Assert.Contains("Results.NoContent", result);
    }

    [Fact]
    public void ScaffoldMinimalApi_AllOps_ShouldContainResultMatch()
    {
        var result = AnalysisTools.ScaffoldMinimalApi("User", "get,create,update,delete");
        Assert.Contains("result.Match", result);
        Assert.Contains("ErrorType.NotFound", result);
    }

    [Fact]
    public void ScaffoldMinimalApi_EmptyResource_ShouldReturnError()
    {
        var result = AnalysisTools.ScaffoldMinimalApi("", "get");
        Assert.StartsWith("Error:", result);
    }
}
