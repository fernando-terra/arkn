using Arkn.MCP.Tools;
using System.Text.Json;
using Xunit;

namespace Arkn.MCP.Tests;

public class ListArknTypesTests
{
    [Fact]
    public void ListArknTypes_ShouldDetectErrorGroups()
    {
        var code   = "[ArknErrors]\npublic static partial class UserErrors { }";
        var result = AnalysisTools.ListArknTypes([code]);
        var doc    = JsonDocument.Parse(result);
        var groups = doc.RootElement.GetProperty("errorGroups").EnumerateArray().Select(e => e.GetString()).ToList();
        Assert.Contains("UserErrors", groups);
    }

    [Fact]
    public void ListArknTypes_ShouldDetectJobs()
    {
        var code   = "public sealed class InvoiceProcessorJob : IArknJob { }";
        var result = AnalysisTools.ListArknTypes([code]);
        var doc    = JsonDocument.Parse(result);
        var jobs   = doc.RootElement.GetProperty("jobs").EnumerateArray().ToList();
        Assert.Contains(jobs, j => j.GetProperty("name").GetString() == "InvoiceProcessorJob");
    }

    [Fact]
    public void ListArknTypes_ShouldDetectHttpClients()
    {
        var code   = "public sealed class PaymentClient(IArknHttp http) : ArknHttpClient(http, \"https://api.pay.com\") { }";
        var result = AnalysisTools.ListArknTypes([code]);
        var doc    = JsonDocument.Parse(result);
        var clients = doc.RootElement.GetProperty("httpClients").EnumerateArray().Select(e => e.GetString()).ToList();
        Assert.Contains("PaymentClient", clients);
    }

    [Fact]
    public void ListArknTypes_EmptyFiles_ShouldReturnEmptyCollections()
    {
        var result = AnalysisTools.ListArknTypes([]);
        var doc    = JsonDocument.Parse(result);
        Assert.Equal(0, doc.RootElement.GetProperty("errorGroups").GetArrayLength());
        Assert.Equal(0, doc.RootElement.GetProperty("jobs").GetArrayLength());
    }
}
