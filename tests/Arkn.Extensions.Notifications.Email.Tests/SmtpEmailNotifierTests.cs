using Arkn.Extensions.Notifications.Email;
using Arkn.Notifications.Models;
using Xunit;

namespace Arkn.Extensions.Notifications.Email.Tests;

public class SmtpEmailNotifierTests
{
    private static ArknNotification Make(NotificationLevel level = NotificationLevel.Error) =>
        new("Job Failed", "Invoice processor crashed", level, "jobs/invoice",
            new Dictionary<string, object?> { ["RunId"] = "abc-123", ["Duration"] = "4:32" });

    [Fact]
    public async Task NotifyAsync_BelowMinimumLevel_ShouldNotThrow()
    {
        var opts = new SmtpEmailOptions
        {
            MinimumLevel = NotificationLevel.Error,
            To           = [new EmailAddress("test@example.com")],
            Host         = "localhost",
            Port         = 25,
            EnableSsl    = false,
        };
        var notifier = new SmtpEmailNotifier(opts);

        // Should silently do nothing (level is Warning < Error)
        var ex = await Record.ExceptionAsync(() =>
            notifier.NotifyAsync(Make(NotificationLevel.Warning)));

        Assert.Null(ex);
    }

    [Fact]
    public async Task NotifyAsync_NoRecipients_ShouldNotThrow()
    {
        var opts    = new SmtpEmailOptions { To = [] };
        var notifier = new SmtpEmailNotifier(opts);

        var ex = await Record.ExceptionAsync(() =>
            notifier.NotifyAsync(Make()));

        Assert.Null(ex);
    }

    [Fact]
    public async Task NotifyAsync_SmtpUnavailable_ShouldNotThrow()
    {
        // Port 1 will fail — notifier must swallow the error
        var opts = new SmtpEmailOptions
        {
            Host      = "127.0.0.1",
            Port      = 1,
            EnableSsl = false,
            TimeoutMs = 500,
            To        = [new EmailAddress("test@example.com")],
        };
        var notifier = new SmtpEmailNotifier(opts);

        var ex = await Record.ExceptionAsync(() =>
            notifier.NotifyAsync(Make()));

        Assert.Null(ex);
    }
}
