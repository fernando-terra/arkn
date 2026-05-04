using System.Net;
using System.Text;
using Arkn.Http.Cache;
using Arkn.Http.Client;
using Arkn.Http.Configuration;
using Arkn.Http.Tests.Fakes;
using Xunit;

namespace Arkn.Http.Tests.Cache;

public sealed class InMemoryResponseCacheTests
{
    // ── Basic get/set/invalidate ───────────────────────────────────────────────

    [Fact]
    public void TryGet_ReturnsFalse_WhenKeyNotSet()
    {
        var cache = new InMemoryResponseCache();
        Assert.False(cache.TryGet("missing", out _));
    }

    [Fact]
    public void TryGet_ReturnsTrue_WhenEntryIsNotExpired()
    {
        var cache = new InMemoryResponseCache();
        var body  = Encoding.UTF8.GetBytes("{\"id\":1}");
        cache.Set("key", body, "application/json", TimeSpan.FromMinutes(5));

        Assert.True(cache.TryGet("key", out var result));
        Assert.Equal(body, result.Body);
        Assert.Equal("application/json", result.ContentType);
    }

    [Fact]
    public void TryGet_ReturnsFalse_WhenEntryIsExpired()
    {
        var cache = new InMemoryResponseCache();
        var body  = Encoding.UTF8.GetBytes("{}");
        // TTL of -1 second → already expired
        cache.Set("key", body, "application/json", TimeSpan.FromSeconds(-1));

        Assert.False(cache.TryGet("key", out _));
    }

    [Fact]
    public void Invalidate_RemovesEntry()
    {
        var cache = new InMemoryResponseCache();
        var body  = Encoding.UTF8.GetBytes("{}");
        cache.Set("key", body, null, TimeSpan.FromMinutes(1));

        cache.Invalidate("key");

        Assert.False(cache.TryGet("key", out _));
    }

    [Fact]
    public void BuildKey_ProducesDeterministicKey()
    {
        var uri  = new Uri("https://api.test/users/1");
        var key1 = InMemoryResponseCache.BuildKey(HttpMethod.Get, uri);
        var key2 = InMemoryResponseCache.BuildKey(HttpMethod.Get, uri);
        Assert.Equal(key1, key2);
    }

    [Fact]
    public void BuildKey_DifferentiatesMethodAndUri()
    {
        var uri  = new Uri("https://api.test/users");
        var get  = InMemoryResponseCache.BuildKey(HttpMethod.Get, uri);
        var post = InMemoryResponseCache.BuildKey(HttpMethod.Post, uri);
        Assert.NotEqual(get, post);
    }
}

public sealed class ResponseCachePipelineTests
{
    // ── Cache hit ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CacheHit_ReturnsBodyWithoutSendingRequest()
    {
        var handler = FakeHttpMessageHandler.RespondWith(HttpStatusCode.OK, """{"id":1,"name":"Alice"}""");
        var http    = BuildArknHttp(handler, cacheOpts: new ResponseCacheOptions());

        // First request — cache miss, sends request
        var r1 = await http.Request("/users/1").Get().As<UserDto>();
        Assert.True(r1.IsSuccess);
        Assert.Equal(1, handler.CapturedRequests.Count);

        // Second request — cache hit, no new HTTP call
        var r2 = await http.Request("/users/1").Get().As<UserDto>();
        Assert.True(r2.IsSuccess);
        Assert.Equal(1, handler.CapturedRequests.Count); // still 1
        Assert.Equal("Alice", r2.Value.Name);
    }

    // ── Cache miss ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CacheMiss_SendsRequest_AndCachesResponse()
    {
        var handler = FakeHttpMessageHandler.RespondWith(HttpStatusCode.OK, """{"id":2,"name":"Bob"}""");
        var http    = BuildArknHttp(handler, cacheOpts: new ResponseCacheOptions());

        var result = await http.Request("/users/2").Get().As<UserDto>();

        Assert.True(result.IsSuccess);
        Assert.Equal(1, handler.CapturedRequests.Count);
        Assert.Equal("Bob", result.Value.Name);
    }

    // ── Expired entry ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ExpiredEntry_SendsFreshRequest()
    {
        var handler = FakeHttpMessageHandler.RespondSequence(
            (HttpStatusCode.OK, """{"id":3,"name":"First"}"""),
            (HttpStatusCode.OK, """{"id":3,"name":"Second"}"""));

        var cacheOpts = new ResponseCacheOptions
        {
            DefaultTtl = TimeSpan.FromMilliseconds(50), // very short TTL
        };
        var http = BuildArknHttp(handler, cacheOpts: cacheOpts);

        var r1 = await http.Request("/users/3").Get().As<UserDto>();
        Assert.Equal("First", r1.Value.Name);
        Assert.Equal(1, handler.CapturedRequests.Count);

        await Task.Delay(100); // let the cache expire

        var r2 = await http.Request("/users/3").Get().As<UserDto>();
        Assert.Equal("Second", r2.Value.Name);
        Assert.Equal(2, handler.CapturedRequests.Count); // fresh request sent
    }

    // ── Non-GET requests are not cached ───────────────────────────────────────

    [Fact]
    public async Task PostRequests_AreNotCached()
    {
        var handler = FakeHttpMessageHandler.RespondWith(HttpStatusCode.Created, """{"id":99,"name":"New"}""");
        var http    = BuildArknHttp(handler, cacheOpts: new ResponseCacheOptions());

        await http.Request("/users").WithBody(new { name = "New" }).Post().As<UserDto>();
        await http.Request("/users").WithBody(new { name = "New" }).Post().As<UserDto>();

        // Both requests should hit the server (POST not in CacheMethods)
        Assert.Equal(2, handler.CapturedRequests.Count);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static Arkn.Http.Abstractions.IArknHttp BuildArknHttp(
        FakeHttpMessageHandler handler,
        ResponseCacheOptions? cacheOpts = null)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test") };
        var options    = new ArknHttpOptions { Timeout = TimeSpan.FromSeconds(5) };

        if (cacheOpts is not null)
        {
            options.ResponseCacheOptions = cacheOpts;
            options.ResponseCache        = new InMemoryResponseCache();
        }

        return new ArknHttp(httpClient, options);
    }

    private sealed record UserDto(int Id, string Name);
}
