using System.Net;
using Arkn.Extensions.Notifications.Email;
using Arkn.Notifications.Models;
using Xunit;

namespace Arkn.Extensions.Notifications.Email.Tests;

public class SendGridEmailNotifierTests
{
    private static ArknNotification Make(NotificationLevel level = NotificationLevel.Error) =>
        new("Alert", "Something failed", level, "jobs/test",
            new Dictionary<string, object?> { ["RunId"] = "xyz" });

    private static (SendGridEmailNotifier notifier, List<HttpRequestMessage> requests)
        Build(HttpStatusCode status = HttpStatusCode.Accepted)
    {
        var requests = new List<HttpRequestMessage>();
        var handler  = new FakeHandler(requests, status);
        var http     = new HttpClient(handler);
        var opts     = new SendGridEmailOptions
        {
            ApiKey = "SG.test",
            From   = new EmailAddress("noreply@example.com", "Arkn"),
            To     = [new EmailAddress("ops@example.com")],
        };
        return (new SendGridEmailNotifier(http, opts), requests);
    }

    [Fact]
    public async Task NotifyAsync_ShouldPostToSendGrid()
    {
        var (notifier, requests) = Build();
        await notifier.NotifyAsync(Make());

        Assert.Single(requests);
        Assert.Equal("https://api.sendgrid.com/v3/mail/send", requests[0].RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, requests[0].Method);
    }

    [Fact]
    public async Task NotifyAsync_ShouldIncludeBothContentTypes()
    {
        var (notifier, requests) = Build();
        await notifier.NotifyAsync(Make());

        var body = await requests[0].Content!.ReadAsStringAsync();
        Assert.Contains("text/plain", body);
        Assert.Contains("text/html",  body);
    }

    [Fact]
    public async Task NotifyAsync_BelowMinimumLevel_ShouldNotSend()
    {
        var (_, requests) = Build();
        var opts = new SendGridEmailOptions
        {
            ApiKey       = "SG.test",
            MinimumLevel = NotificationLevel.Critical,
            To           = [new EmailAddress("ops@example.com")],
        };
        var filtered = new SendGridEmailNotifier(
            new HttpClient(new FakeHandler(requests, HttpStatusCode.Accepted)), opts);

        await filtered.NotifyAsync(Make(NotificationLevel.Warning));

        Assert.Empty(requests);
    }

    [Fact]
    public async Task NotifyAsync_SendGridError_ShouldNotThrow()
    {
        var (notifier, _) = Build(HttpStatusCode.InternalServerError);
        var ex = await Record.ExceptionAsync(() => notifier.NotifyAsync(Make()));
        Assert.Null(ex);
    }

    [Fact]
    public async Task NotifyAsync_ShouldIncludeMetadataInBody()
    {
        var (notifier, requests) = Build();
        await notifier.NotifyAsync(Make());

        var body = await requests[0].Content!.ReadAsStringAsync();
        Assert.Contains("RunId", body);
        Assert.Contains("xyz",   body);
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly List<HttpRequestMessage> _requests;
        private readonly HttpStatusCode           _status;

        public FakeHandler(List<HttpRequestMessage> r, HttpStatusCode s)
        {
            _requests = r;
            _status   = s;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage req, CancellationToken ct)
        {
            _requests.Add(req);
            return Task.FromResult(new HttpResponseMessage(_status));
        }
    }
}
