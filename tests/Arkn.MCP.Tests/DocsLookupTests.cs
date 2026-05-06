using Arkn.MCP.Tools;

namespace Arkn.MCP.Tests;

public class DocsLookupTests
{
    [Fact]
    public void Lookup_KnownTopic_ShouldReturnContent()
    {
        var result = DocsTools.DocsLookup("result");
        Assert.NotEmpty(result);
        Assert.Contains("Result", result);
    }

    [Fact]
    public void Lookup_ErrorTopic_ShouldReturnErrorDocs()
    {
        var result = DocsTools.DocsLookup("error");
        Assert.Contains("ARK002", result);
    }

    [Fact]
    public void Lookup_JobTopic_ShouldReturnJobDocs()
    {
        var result = DocsTools.DocsLookup("iarknjob");
        Assert.Contains("ExecuteAsync", result);
    }

    [Fact]
    public void Lookup_UnknownTopic_ShouldReturnHelpfulMessage()
    {
        var result = DocsTools.DocsLookup("xyzzy");
        Assert.Contains("No documentation found", result);
    }

    [Fact]
    public void Lookup_KeywordSearch_ShouldMatchPartialTopic()
    {
        var result = DocsTools.DocsLookup("cron");
        Assert.NotEmpty(result);
        Assert.DoesNotContain("No documentation found", result);
    }

    [Fact]
    public void Lookup_EmptyQuery_ShouldReturnHelpfulMessage()
    {
        var result = DocsTools.DocsLookup("");
        Assert.NotEmpty(result);
    }
}
