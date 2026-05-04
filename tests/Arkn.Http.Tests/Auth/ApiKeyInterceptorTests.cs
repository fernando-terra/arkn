using System.Net;
using Arkn.Http.Auth;
using Arkn.Http.Client;
using Arkn.Http.Configuration;
using Arkn.Http.Tests.Fakes;
using Xunit;

namespace Arkn.Http.Tests.Auth;

public sealed class ApiKeyInterceptorTests
{
    // ── Header placement ───────────────────────────────────────────────────────

    [Fact]
    public async Task ApplyAsync_HeaderPlacement_AddsHeaderToRequest()
    {
        var interceptor = new ApiKeyInterceptor("X-Api-Key", "secret", ApiKeyInterceptor.Placement.Header);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/resource");

        await interceptor.ApplyAsync(request);

        Assert.True(request.Headers.Contains("X-Api-Key"));
        Assert.Equal("secret", request.Headers.GetValues("X-Api-Key").First());
    }

    [Fact]
    public async Task ApplyAsync_DefaultPlacement_UsesHeader()
    {
        // Default ctor placement is Header
        var interceptor = new ApiKeyInterceptor("X-Api-Key", "my-key");
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/resource");

        await interceptor.ApplyAsync(request);

        Assert.True(request.Headers.Contains("X-Api-Key"));
    }

    // ── Query param placement ──────────────────────────────────────────────────

    [Fact]
    public async Task ApplyAsync_QueryParamPlacement_AppendsToUrl_WhenNoExistingQuery()
    {
        var interceptor = new ApiKeyInterceptor("api_key", "abc123", ApiKeyInterceptor.Placement.QueryParam);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/data");

        await interceptor.ApplyAsync(request);

        Assert.NotNull(request.RequestUri);
        Assert.Contains("api_key=abc123", request.RequestUri!.Query);
        Assert.StartsWith("?", request.RequestUri!.Query);
    }

    [Fact]
    public async Task ApplyAsync_QueryParamPlacement_AppendsToUrl_WhenQueryExists()
    {
        var interceptor = new ApiKeyInterceptor("api_key", "abc123", ApiKeyInterceptor.Placement.QueryParam);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/data?page=1");

        await interceptor.ApplyAsync(request);

        Assert.Contains("&api_key=abc123", request.RequestUri!.Query);
    }

    [Fact]
    public async Task ApplyAsync_QueryParamPlacement_EscapesNameAndValue()
    {
        var interceptor = new ApiKeyInterceptor("my key", "val ue", ApiKeyInterceptor.Placement.QueryParam);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/data");

        await interceptor.ApplyAsync(request);

        var query = request.RequestUri!.Query;
        Assert.Contains("my%20key=val%20ue", query);
    }

    // ── Pipeline integration ───────────────────────────────────────────────────

    [Fact]
    public async Task WithApiKey_Header_SentInRequest()
    {
        var handler = FakeHttpMessageHandler.RespondWith(HttpStatusCode.OK, "{}");
        var options = new ArknHttpOptions { Timeout = TimeSpan.FromSeconds(5) };
        options.Interceptors.Add(new ApiKeyInterceptor("X-Api-Key", "pipeline-secret"));

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test") };
        var arknHttp   = new ArknHttp(httpClient, options);

        await arknHttp.Request("/data").Get().AsResult();

        var captured = handler.CapturedRequests[0];
        Assert.True(captured.Headers.Contains("X-Api-Key"));
        Assert.Equal("pipeline-secret", captured.Headers.GetValues("X-Api-Key").First());
    }

    [Fact]
    public async Task WithApiKey_QueryParam_SentInUrl()
    {
        var handler = FakeHttpMessageHandler.RespondWith(HttpStatusCode.OK, "{}");
        var options = new ArknHttpOptions { Timeout = TimeSpan.FromSeconds(5) };
        options.Interceptors.Add(new ApiKeyInterceptor("token", "qp-secret", ApiKeyInterceptor.Placement.QueryParam));

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test") };
        var arknHttp   = new ArknHttp(httpClient, options);

        await arknHttp.Request("/data").Get().AsResult();

        var captured = handler.CapturedRequests[0];
        Assert.Contains("token=qp-secret", captured.RequestUri!.Query);
    }
}
