using System.Net;
using System.Text;

namespace Arkn.Http.Tests.Fakes;

/// <summary>
/// Native mock <see cref="HttpMessageHandler"/> — no external mocking library required.
/// Configure the response before calling, then assert on captured requests.
/// </summary>
public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private Func<HttpRequestMessage, HttpResponseMessage> _handler;

    /// <summary>All requests captured in order of arrival.</summary>
    public List<HttpRequestMessage> CapturedRequests { get; } = new();

    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        _handler = handler;
    }

    /// <summary>Responds with a fixed status code and JSON body.</summary>
    public static FakeHttpMessageHandler RespondWith(HttpStatusCode status, string body = "")
    {
        return new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
    }

    /// <summary>Throws <see cref="HttpRequestException"/> on every request.</summary>
    public static FakeHttpMessageHandler ThrowsNetworkError(string message = "Network error")
    {
        return new FakeHttpMessageHandler(_ => throw new HttpRequestException(message));
    }

    /// <summary>Responds differently per call — useful for retry testing.</summary>
    public static FakeHttpMessageHandler RespondSequence(
        params (HttpStatusCode status, string body)[] responses)
    {
        var queue = new Queue<(HttpStatusCode, string)>(responses);
        return new FakeHttpMessageHandler(_ =>
        {
            if (!queue.TryDequeue(out var r))
                r = (HttpStatusCode.InternalServerError, "{}");
            return new HttpResponseMessage(r.Item1)
            {
                Content = new StringContent(r.Item2, Encoding.UTF8, "application/json")
            };
        });
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        CapturedRequests.Add(request);
        return Task.FromResult(_handler(request));
    }
}
