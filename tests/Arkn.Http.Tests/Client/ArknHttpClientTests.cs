using System.Net;
using Arkn.Http.Abstractions;
using Arkn.Http.Client;
using Arkn.Http.Configuration;
using Arkn.Http.Tests.Fakes;
using Arkn.Results;
using Xunit;

namespace Arkn.Http.Tests.Client;

public sealed class ArknHttpClientTests
{
    // ── Typed client resolves base URL ─────────────────────────────────────────

    [Fact]
    public async Task TypedClient_PrependBaseUrl_SendsCorrectRequest()
    {
        var handler = FakeHttpMessageHandler.RespondWith(HttpStatusCode.OK, """{"id":5,"name":"Eve"}""");
        var client  = BuildUserClient(handler);

        var result = await client.GetAsync(5);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Id);

        var captured = handler.CapturedRequests[0];
        Assert.Equal("/users/5", captured.RequestUri!.PathAndQuery);
    }

    [Fact]
    public async Task TypedClient_ListAll_ReturnsCollection()
    {
        var json    = """[{"id":1,"name":"Alice"},{"id":2,"name":"Bob"}]""";
        var handler = FakeHttpMessageHandler.RespondWith(HttpStatusCode.OK, json);
        var client  = BuildUserClient(handler);

        var result = await client.ListAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task TypedClient_Create_PostsBodyAndReturnsCreated()
    {
        var json    = """{"id":3,"name":"Carol"}""";
        var handler = FakeHttpMessageHandler.RespondWith(HttpStatusCode.Created, json);
        var client  = BuildUserClient(handler);

        var result = await client.CreateAsync(new CreateUserRequest("Carol"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Carol", result.Value.Name);

        var captured = handler.CapturedRequests[0];
        Assert.Equal(HttpMethod.Post, captured.Method);
    }

    [Fact]
    public async Task TypedClient_Delete_ReturnsSuccess()
    {
        var handler = FakeHttpMessageHandler.RespondWith(HttpStatusCode.NoContent);
        var client  = BuildUserClient(handler);

        var result = await client.DeleteAsync(3);

        Assert.True(result.IsSuccess);
    }

    // ── Retry ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Retry_RecoverFrom500OnSecondAttempt_ReturnsSuccess()
    {
        var handler = FakeHttpMessageHandler.RespondSequence(
            (HttpStatusCode.InternalServerError, "{}"),
            (HttpStatusCode.OK, """{"id":1,"name":"Alice"}"""));

        var options = new ArknHttpOptions
        {
            MaxRetryAttempts = 2,
            BaseRetryDelay   = TimeSpan.FromMilliseconds(1), // fast for tests
            Timeout          = TimeSpan.FromSeconds(5),
        };

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test") };
        IArknHttp arknHttp = new ArknHttp(httpClient, options);
        var client = new UserClient(arknHttp);

        var result = await client.GetAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, handler.CapturedRequests.Count); // 1 fail + 1 success
    }

    [Fact]
    public async Task Retry_AllAttemptsFailWith500_ReturnsServerError()
    {
        var handler = FakeHttpMessageHandler.RespondSequence(
            (HttpStatusCode.InternalServerError, "{}"),
            (HttpStatusCode.InternalServerError, "{}"),
            (HttpStatusCode.InternalServerError, "{}"));

        var options = new ArknHttpOptions
        {
            MaxRetryAttempts = 3,
            BaseRetryDelay   = TimeSpan.FromMilliseconds(1),
            Timeout          = TimeSpan.FromSeconds(5),
        };

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test") };
        IArknHttp arknHttp = new ArknHttp(httpClient, options);
        var client = new UserClient(arknHttp);

        var result = await client.GetAsync(1);

        Assert.True(result.IsFailure);
        Assert.StartsWith("Http.ServerError", result.Error.Code);
        Assert.Equal(3, handler.CapturedRequests.Count);
    }

    [Fact]
    public async Task Retry_400Response_IsNotRetried()
    {
        var handler = FakeHttpMessageHandler.RespondSequence(
            (HttpStatusCode.BadRequest, "{}"),
            (HttpStatusCode.OK, """{"id":1,"name":"Alice"}""")); // should never reach this

        var options = new ArknHttpOptions
        {
            MaxRetryAttempts = 3,
            BaseRetryDelay   = TimeSpan.FromMilliseconds(1),
            Timeout          = TimeSpan.FromSeconds(5),
        };

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test") };
        IArknHttp arknHttp = new ArknHttp(httpClient, options);
        var client = new UserClient(arknHttp);

        var result = await client.GetAsync(1);

        Assert.True(result.IsFailure);
        Assert.Equal("Http.BadRequest", result.Error.Code);
        Assert.Equal(1, handler.CapturedRequests.Count); // only 1 attempt
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static UserClient BuildUserClient(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test") };
        var options    = new ArknHttpOptions { Timeout = TimeSpan.FromSeconds(5) };
        IArknHttp arknHttp = new ArknHttp(httpClient, options);
        return new UserClient(arknHttp);
    }
}

// ── Test typed client ──────────────────────────────────────────────────────────

public sealed class UserClient : ArknHttpClient
{
    public UserClient(IArknHttp http) : base(http, "https://api.test") { }

    public Task<Result<UserDto>> GetAsync(int id) =>
        Request($"/users/{id}").Get().As<UserDto>();

    public Task<Result<IReadOnlyList<UserDto>>> ListAsync() =>
        Request("/users").Get().As<IReadOnlyList<UserDto>>();

    public Task<Result<UserDto>> CreateAsync(CreateUserRequest req) =>
        Request("/users").WithBody(req).Post().As<UserDto>();

    public Task<Result> DeleteAsync(int id) =>
        Request($"/users/{id}").Delete().AsResult();
}

public sealed record UserDto(int Id, string Name);
public sealed record CreateUserRequest(string Name);
