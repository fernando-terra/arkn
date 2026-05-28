using System.Net;
using Arkn.Extensions.Logging.Elasticsearch;
using Arkn.Logging.Models;
using Xunit;

namespace Arkn.Extensions.Logging.Sinks.Tests;

public class ElasticsearchSinkTests
{
    private static LogEntry MakeEntry(ArknLogLevel level = ArknLogLevel.Info) =>
        new(level, "ES test message", DateTimeOffset.UtcNow, "scope-1",
            new Dictionary<string, object?> { ["RequestId"] = "abc" }, null);

    private static (ElasticsearchSink sink, List<HttpRequestMessage> requests) BuildWithFakeHttp()
    {
        var requests = new List<HttpRequestMessage>();
        var handler  = new FakeHttpHandler(requests);
        var http     = new HttpClient(handler);
        var opts     = new ElasticsearchSinkOptions
        {
            NodeUrl  = "http://es-test:9200",
            BatchSize = 2,
            IndexPattern = "arkn-{yyyy-MM-dd}",
        };
        return (new ElasticsearchSink(http, opts), requests);
    }

    [Fact]
    public void Write_AtBatchSize_ShouldSendBulkRequest()
    {
        var (sink, requests) = BuildWithFakeHttp();
        sink.Write(MakeEntry());
        sink.Write(MakeEntry());

        Thread.Sleep(100);
        Assert.Single(requests);
        Assert.EndsWith("/_bulk", requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task Write_ShouldProduceNdJsonWithActionAndDocument()
    {
        var (sink, requests) = BuildWithFakeHttp();
        sink.Write(MakeEntry());
        sink.Write(MakeEntry());

        Thread.Sleep(100);
        var body  = await requests[0].Content!.ReadAsStringAsync();
        var lines = body.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Bulk format: alternating action + document lines
        Assert.True(lines.Length >= 2);
        Assert.Contains("_index", lines[0]);
        Assert.Contains("@timestamp", lines[1]);
        Assert.Contains("message", lines[1]);
    }

    [Fact]
    public async Task IndexPattern_ShouldResolveDate()
    {
        var requests = new List<HttpRequestMessage>();
        var handler  = new FakeHttpHandler(requests);
        var opts     = new ElasticsearchSinkOptions { IndexPattern = "logs-{yyyy-MM-dd}", BatchSize = 1 };
        var sink     = new ElasticsearchSink(new HttpClient(handler), opts);

        sink.Write(MakeEntry());
        Thread.Sleep(100);

        var body = await (requests.FirstOrDefault()?.Content?.ReadAsStringAsync() ?? Task.FromResult("")) ?? "";
        Assert.Contains($"logs-{DateTimeOffset.UtcNow:yyyy-MM-dd}", body);
    }

    [Fact]
    public void Write_BelowMinimumLevel_ShouldDrop()
    {
        var requests = new List<HttpRequestMessage>();
        var opts     = new ElasticsearchSinkOptions
        {
            NodeUrl = "http://es:9200", MinimumLevel = ArknLogLevel.Error, BatchSize = 1
        };
        var sink = new ElasticsearchSink(new HttpClient(new FakeHttpHandler(requests)), opts);

        sink.Write(MakeEntry(ArknLogLevel.Debug));
        Thread.Sleep(50);
        Assert.Empty(requests);
    }

    private sealed class FakeHttpHandler : HttpMessageHandler
    {
        private readonly List<HttpRequestMessage> _requests;
        public FakeHttpHandler(List<HttpRequestMessage> r) => _requests = r;
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage req, CancellationToken ct)
        {
            _requests.Add(req);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
