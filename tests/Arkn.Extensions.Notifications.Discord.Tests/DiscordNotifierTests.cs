using Arkn.Extensions.Notifications.Discord;
using Arkn.Notifications.Models;

namespace Arkn.Extensions.Notifications.Discord.Tests;

public class DiscordNotifierTests
{
    private static ArknNotification Make(NotificationLevel level = NotificationLevel.Error) =>
        new("Job Failed", "Invoice processor crashed", level, "jobs/invoice",
            new Dictionary<string, object?> { ["RunId"] = "abc-123" });

    [Fact]
    public async Task NotifyAsync_BelowMinimumLevel_ShouldNotThrow()
    {
        var opts = new DiscordNotifierOptions
        {
            WebhookUrl   = "https://discord.com/api/webhooks/test/token",
            MinimumLevel = NotificationLevel.Error,
        };
        var notifier = new DiscordNotifier(new HttpClient(), opts);

        var ex = await Record.ExceptionAsync(() =>
            notifier.NotifyAsync(Make(NotificationLevel.Warning)));

        Assert.Null(ex);
    }

    [Fact]
    public async Task NotifyAsync_EmptyWebhookUrl_ShouldNotThrow()
    {
        var opts     = new DiscordNotifierOptions { WebhookUrl = string.Empty };
        var notifier = new DiscordNotifier(new HttpClient(), opts);

        var ex = await Record.ExceptionAsync(() =>
            notifier.NotifyAsync(Make()));

        Assert.Null(ex);
    }

    [Fact]
    public async Task NotifyAsync_UnreachableUrl_ShouldNotThrow()
    {
        var opts = new DiscordNotifierOptions
        {
            WebhookUrl   = "http://127.0.0.1:1/webhook",
            MinimumLevel = NotificationLevel.Info,
            Timeout      = TimeSpan.FromMilliseconds(300),
        };
        var notifier = new DiscordNotifier(new HttpClient(), opts);

        var ex = await Record.ExceptionAsync(() =>
            notifier.NotifyAsync(Make()));

        Assert.Null(ex);
    }
}
