using System.Net;
using System.Net.Http.Headers;
using Arkn.Http.Auth;
using Arkn.Http.Client;
using Arkn.Http.Configuration;
using Arkn.Http.Tests.Fakes;
using Xunit;

namespace Arkn.Http.Tests.Auth;

public sealed class InMemoryTokenStoreTests
{
    [Fact]
    public async Task GetAsync_ReturnsNull_WhenKeyNotSet()
    {
        var store = new InMemoryTokenStore();
        var token = await store.GetAsync("missing");
        Assert.Null(token);
    }

    [Fact]
    public async Task SetAndGet_ReturnsCachedToken_WhenNotExpired()
    {
        var store = new InMemoryTokenStore();
        await store.SetAsync("key", "my-token", DateTimeOffset.UtcNow.AddMinutes(60));

        var token = await store.GetAsync("key");
        Assert.Equal("my-token", token);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenTokenExpiredWithBuffer()
    {
        var store = new InMemoryTokenStore();
        // Store a token that expires in 20 seconds — within the 30-second buffer
        await store.SetAsync("key", "expiring-token", DateTimeOffset.UtcNow.AddSeconds(20));

        var token = await store.GetAsync("key");
        Assert.Null(token); // should be treated as expired due to buffer
    }

    [Fact]
    public async Task InvalidateAsync_RemovesToken()
    {
        var store = new InMemoryTokenStore();
        await store.SetAsync("key", "token", DateTimeOffset.UtcNow.AddMinutes(60));

        await store.InvalidateAsync("key");

        var token = await store.GetAsync("key");
        Assert.Null(token);
    }

    [Fact]
    public async Task SetAsync_Overwrites_PreviousToken()
    {
        var store = new InMemoryTokenStore();
        await store.SetAsync("key", "old-token", DateTimeOffset.UtcNow.AddMinutes(60));
        await store.SetAsync("key", "new-token", DateTimeOffset.UtcNow.AddMinutes(60));

        var token = await store.GetAsync("key");
        Assert.Equal("new-token", token);
    }
}

public sealed class BearerTokenInterceptorTests
{
    [Fact]
    public async Task ApplyAsync_AttachesBearerTokenToRequest()
    {
        var store       = new InMemoryTokenStore();
        var interceptor = new BearerTokenInterceptor(
            tokenFactory: () => Task.FromResult("test-token"),
            storeKey: "test",
            store: store);

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/resource");
        await interceptor.ApplyAsync(request);

        Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
        Assert.Equal("test-token", request.Headers.Authorization!.Parameter);
    }

    [Fact]
    public async Task ApplyAsync_UsesCache_OnSecondCall()
    {
        var store    = new InMemoryTokenStore();
        var callCount = 0;

        var interceptor = new BearerTokenInterceptor(
            tokenFactory: () => { callCount++; return Task.FromResult("cached-token"); },
            storeKey: "test",
            store: store);

        using var req1 = new HttpRequestMessage(HttpMethod.Get, "https://api.test/1");
        using var req2 = new HttpRequestMessage(HttpMethod.Get, "https://api.test/2");

        await interceptor.ApplyAsync(req1);
        await interceptor.ApplyAsync(req2);

        Assert.Equal(1, callCount); // factory called only once
        Assert.Equal("cached-token", req2.Headers.Authorization!.Parameter);
    }

    [Fact]
    public async Task ApplyAsync_RefetchesToken_AfterInvalidation()
    {
        var store    = new InMemoryTokenStore();
        var callCount = 0;

        var interceptor = new BearerTokenInterceptor(
            tokenFactory: () => { callCount++; return Task.FromResult($"token-{callCount}"); },
            storeKey: "test",
            store: store);

        using var req1 = new HttpRequestMessage(HttpMethod.Get, "https://api.test/1");
        await interceptor.ApplyAsync(req1);

        await store.InvalidateAsync("test");

        using var req2 = new HttpRequestMessage(HttpMethod.Get, "https://api.test/2");
        await interceptor.ApplyAsync(req2);

        Assert.Equal(2, callCount);
        Assert.Equal("token-2", req2.Headers.Authorization!.Parameter);
    }
}

public sealed class InterceptorPipelineTests
{
    [Fact]
    public async Task Request_WithBearerInterceptor_AttachesAuthHeader()
    {
        var handler = FakeHttpMessageHandler.RespondWith(HttpStatusCode.OK, """{"id":1,"name":"Alice"}""");
        var store   = new InMemoryTokenStore();

        var interceptor = new BearerTokenInterceptor(
            tokenFactory: () => Task.FromResult("pipeline-token"),
            storeKey: "test",
            store: store);

        var options = new ArknHttpOptions
        {
            Timeout = TimeSpan.FromSeconds(5),
        };
        options.Interceptors.Add(interceptor);

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test") };
        var arknHttp   = new ArknHttp(httpClient, options);

        var result = await arknHttp.Request("/users/1").Get().As<UserDto>();

        Assert.True(result.IsSuccess);

        var captured = handler.CapturedRequests[0];
        Assert.True(captured.Headers.Contains("Authorization"));
        Assert.Equal("Bearer pipeline-token",
            captured.Headers.Authorization?.ToString());
    }

    [Fact]
    public async Task Request_MultipleInterceptors_AllApplied()
    {
        var handler = FakeHttpMessageHandler.RespondWith(HttpStatusCode.OK, """{"id":2,"name":"Bob"}""");

        var interceptor1 = new HeaderInterceptor("X-Header-1", "value1");
        var interceptor2 = new HeaderInterceptor("X-Header-2", "value2");

        var options = new ArknHttpOptions { Timeout = TimeSpan.FromSeconds(5) };
        options.Interceptors.Add(interceptor1);
        options.Interceptors.Add(interceptor2);

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test") };
        var arknHttp   = new ArknHttp(httpClient, options);

        await arknHttp.Request("/users/2").Get().As<UserDto>();

        var captured = handler.CapturedRequests[0];
        Assert.True(captured.Headers.Contains("X-Header-1"));
        Assert.True(captured.Headers.Contains("X-Header-2"));
    }

    private sealed record UserDto(int Id, string Name);

    // Simple test interceptor that adds a fixed header
    private sealed class HeaderInterceptor : IArknAuthInterceptor
    {
        private readonly string _name;
        private readonly string _value;
        public HeaderInterceptor(string name, string value) { _name = name; _value = value; }
        public Task ApplyAsync(HttpRequestMessage request, CancellationToken ct = default)
        {
            request.Headers.TryAddWithoutValidation(_name, _value);
            return Task.CompletedTask;
        }
    }
}
