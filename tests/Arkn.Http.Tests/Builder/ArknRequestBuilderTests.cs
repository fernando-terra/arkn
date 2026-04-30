using System.Net;
using Arkn.Http.Builder;
using Arkn.Http.Client;
using Arkn.Http.Configuration;
using Arkn.Http.Tests.Fakes;
using Xunit;

namespace Arkn.Http.Tests.Builder;

public sealed class ArknRequestBuilderTests
{
    // ── FormatPath ─────────────────────────────────────────────────────────────

    [Fact]
    public void FormatPath_NoArgs_ReturnsSamePath()
    {
        var result = ArknRequestBuilder.FormatPath("/users", []);
        Assert.Equal("/users", result);
    }

    [Fact]
    public void FormatPath_SingleArg_ReplacesPlaceholder()
    {
        var result = ArknRequestBuilder.FormatPath("/users/{id}", [42]);
        Assert.Equal("/users/42", result);
    }

    [Fact]
    public void FormatPath_MultipleArgs_ReplacesAllPlaceholders()
    {
        var result = ArknRequestBuilder.FormatPath("/orgs/{org}/users/{id}", ["acme", 7]);
        Assert.Equal("/orgs/acme/users/7", result);
    }

    [Fact]
    public void FormatPath_SpecialChars_AreUriEncoded()
    {
        var result = ArknRequestBuilder.FormatPath("/search/{query}", ["hello world"]);
        Assert.Equal("/search/hello%20world", result);
    }

    // ── GET / success ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Get_SuccessResponse_ReturnsSuccessResult()
    {
        var handler = FakeHttpMessageHandler.RespondWith(HttpStatusCode.OK, """{"id":1,"name":"Alice"}""");
        var http    = BuildArknHttp(handler);

        var result = await http.Request("/users/1").Get().As<UserDto>();

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("Alice", result.Value.Name);
    }

    [Fact]
    public async Task Get_404Response_ReturnsNotFoundError()
    {
        var handler = FakeHttpMessageHandler.RespondWith(HttpStatusCode.NotFound, "User not found");
        var http    = BuildArknHttp(handler);

        var result = await http.Request("/users/99").Get().As<UserDto>();

        Assert.True(result.IsFailure);
        Assert.Equal("Http.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Get_401Response_ReturnsUnauthorizedError()
    {
        var handler = FakeHttpMessageHandler.RespondWith(HttpStatusCode.Unauthorized);
        var http    = BuildArknHttp(handler);

        var result = await http.Request("/protected").Get().As<UserDto>();

        Assert.True(result.IsFailure);
        Assert.Equal("Http.Unauthorized", result.Error.Code);
    }

    [Fact]
    public async Task Get_500Response_ReturnsServerError()
    {
        var handler = FakeHttpMessageHandler.RespondWith(HttpStatusCode.InternalServerError, "{}");
        var http    = BuildArknHttp(handler);

        var result = await http.Request("/crash").Get().As<UserDto>();

        Assert.True(result.IsFailure);
        Assert.StartsWith("Http.ServerError", result.Error.Code);
    }

    // ── POST with body ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Post_WithBody_SendsJsonAndReturnsResult()
    {
        var handler = FakeHttpMessageHandler.RespondWith(HttpStatusCode.Created, """{"id":2,"name":"Bob"}""");
        var http    = BuildArknHttp(handler);

        var result = await http
            .Request("/users")
            .WithBody(new { name = "Bob" })
            .Post()
            .As<UserDto>();

        Assert.True(result.IsSuccess);
        Assert.Equal("Bob", result.Value.Name);

        var captured = handler.CapturedRequests[0];
        Assert.Equal(HttpMethod.Post, captured.Method);
        Assert.NotNull(captured.Content);
    }

    // ── DELETE / AsResult ──────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_SuccessResponse_ReturnsSuccessResult()
    {
        var handler = FakeHttpMessageHandler.RespondWith(HttpStatusCode.NoContent);
        var http    = BuildArknHttp(handler);

        var result = await http.Request("/users/1").Delete().AsResult();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Delete_404Response_ReturnsFailureResult()
    {
        var handler = FakeHttpMessageHandler.RespondWith(HttpStatusCode.NotFound);
        var http    = BuildArknHttp(handler);

        var result = await http.Request("/users/999").Delete().AsResult();

        Assert.True(result.IsFailure);
        Assert.Equal("Http.NotFound", result.Error.Code);
    }

    // ── Headers ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task WithHeader_AddsHeaderToRequest()
    {
        var handler = FakeHttpMessageHandler.RespondWith(HttpStatusCode.OK, """{"id":1,"name":"Alice"}""");
        var http    = BuildArknHttp(handler);

        await http
            .Request("/users/1")
            .WithHeader("Authorization", "Bearer tok")
            .Get()
            .As<UserDto>();

        var captured = handler.CapturedRequests[0];
        Assert.True(captured.Headers.Contains("Authorization"));
    }

    // ── Network error ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Get_NetworkError_ReturnsFailureResult()
    {
        var handler = FakeHttpMessageHandler.ThrowsNetworkError("Connection refused");
        var http    = BuildArknHttp(handler);

        var result = await http.Request("/users").Get().As<UserDto>();

        Assert.True(result.IsFailure);
        Assert.Equal("Http.RequestFailed", result.Error.Code);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static Arkn.Http.Abstractions.IArknHttp BuildArknHttp(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test") };
        var options    = new ArknHttpOptions { Timeout = TimeSpan.FromSeconds(5) };
        return new ArknHttp(httpClient, options);
    }

    private sealed record UserDto(int Id, string Name);
}
