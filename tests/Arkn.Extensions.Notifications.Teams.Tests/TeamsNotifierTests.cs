using Arkn.Extensions.Notifications.Teams;
using Arkn.Notifications.Models;

namespace Arkn.Extensions.Notifications.Teams.Tests;

public class TeamsNotifierTests
{
    private static ArknNotification Make(NotificationLevel level = NotificationLevel.Error) =>
        new("Job Failed", "Invoice processor crashed", level, "jobs/invoice",
            new Dictionary<string, object?> { ["RunId"] = "abc-123" });

    [Fact]
    public async Task NotifyAsync_BelowMinimumLevel_ShouldNotThrow()
    {
        var opts = new TeamsNotifierOptions
        {
            WebhookUrl   = "https://your-org.webhook.office.com/test",
            MinimumLevel = NotificationLevel.Error,
        };
        var notifier = new TeamsNotifier(new HttpClient(), opts);

        var ex = await Record.ExceptionAsync(() =>
            notifier.NotifyAsync(Make(NotificationLevel.Warning)));

        Assert.Null(ex);
    }

    [Fact]
    public async Task NotifyAsync_EmptyWebhookUrl_ShouldNotThrow()
    {
        var opts     = new TeamsNotifierOptions { WebhookUrl = string.Empty };
        var notifier = new TeamsNotifier(new HttpClient(), opts);

        var ex = await Record.ExceptionAsync(() =>
            notifier.NotifyAsync(Make()));

        Assert.Null(ex);
    }

    [Fact]
    public async Task NotifyAsync_UnreachableUrl_ShouldNotThrow()
    {
        var opts = new TeamsNotifierOptions
        {
            WebhookUrl   = "http://127.0.0.1:1/webhook",
            MinimumLevel = NotificationLevel.Info,
            Timeout      = TimeSpan.FromMilliseconds(300),
        };
        var notifier = new TeamsNotifier(new HttpClient(), opts);

        var ex = await Record.ExceptionAsync(() =>
            notifier.NotifyAsync(Make()));

        Assert.Null(ex);
    }
}
