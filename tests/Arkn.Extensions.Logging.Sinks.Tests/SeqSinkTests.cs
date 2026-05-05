using System.Net;
using Arkn.Extensions.Logging.Seq;
using Arkn.Logging.Models;
using Xunit;

namespace Arkn.Extensions.Logging.Sinks.Tests;

public class SeqSinkTests
{
    private static LogEntry MakeEntry(ArknLogLevel level = ArknLogLevel.Info, string? scope = null) =>
        new(level, "Test message", DateTimeOffset.UtcNow, scope,
            new Dictionary<string, object?> { ["UserId"] = 42 }, null);

    private static (SeqSink sink, List<HttpRequestMessage> requests) BuildWithFakeHttp(
        HttpStatusCode statusCode = HttpStatusCode.Created)
    {
        var requests = new List<HttpRequestMessage>();
        var handler  = new FakeHttpHandler(requests, statusCode);
        var http     = new HttpClient(handler);
        var opts     = new SeqSinkOptions { ServerUrl = "http://seq-test:5341", BatchSize = 2 };
        var sink     = new SeqSink(http, opts);
        return (sink, requests);
    }

    [Fact]
    public void Write_BelowMinimumLevel_ShouldNotBuffer()
    {
        var (sink, requests) = BuildWithFakeHttp();
        var opts = new SeqSinkOptions { MinimumLevel = ArknLogLevel.Warning, BatchSize = 1 };
        var http = new HttpClient(new FakeHttpHandler(requests, HttpStatusCode.Created));
        var filtered = new SeqSink(http, opts);

        filtered.Write(MakeEntry(ArknLogLevel.Debug));

        Assert.Empty(requests); // nothing sent
    }

    [Fact]
    public void Write_AtBatchSize_ShouldFlushImmediately()
    {
        var (sink, requests) = BuildWithFakeHttp(); // BatchSize = 2
        sink.Write(MakeEntry());
        sink.Write(MakeEntry()); // triggers flush

        // Give async fire-and-forget a moment
        Thread.Sleep(100);
        Assert.Single(requests);
    }

    [Fact]
    public void Write_ShouldProduceClefJson()
    {
        var (sink, requests) = BuildWithFakeHttp();
        sink.Write(MakeEntry(scope: "run-abc"));
        sink.Write(MakeEntry()); // flush

        Thread.Sleep(100);
        Assert.Single(requests);

        var body = requests[0].Content!.ReadAsStringAsync().Result;
        Assert.Contains("@t", body);
        Assert.Contains("@mt", body);
        Assert.Contains("@l", body);
        Assert.Contains("Scope", body);
    }

    private sealed class FakeHttpHandler : HttpMessageHandler
    {
        private readonly List<HttpRequestMessage> _requests;
        private readonly HttpStatusCode _status;
        public FakeHttpHandler(List<HttpRequestMessage> requests, HttpStatusCode status)
        { _requests = requests; _status = status; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken ct)
        {
            _requests.Add(request);
            return Task.FromResult(new HttpResponseMessage(_status));
        }
    }
}
