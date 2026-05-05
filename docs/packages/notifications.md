# Arkn.Notifications

Pluggable notification abstractions — fan-out to N destinations.

```bash
dotnet add package Arkn.Notifications
dotnet add package Arkn.Extensions.Notifications.Slack
dotnet add package Arkn.Extensions.Notifications.Email
```

## Setup

```csharp
builder.Services.AddArknNotifications(n =>
{
    n.AddSlackNotifier(slack =>
    {
        slack.WebhookUrl   = config["Slack:WebhookUrl"];
        slack.Channel      = "#alerts";
        slack.Username     = "Arkn";
        slack.MinimumLevel = NotificationLevel.Warning;
    });

    n.AddSmtpEmailNotifier(smtp =>
    {
        smtp.Host = "smtp.example.com";
        smtp.Port = 587;
        smtp.From = new EmailAddress("noreply@example.com", "Arkn");
        smtp.To   = [new EmailAddress("ops@example.com")];
    });
});
```

## Manual dispatch

```csharp
public class AlertService(IArknNotifierRegistry notifier)
{
    public async Task SendAlertAsync(string title, string body)
    {
        await notifier.DispatchAsync(
            ArknNotification.Error(title, body, "MyService"));
    }
}
```

## Custom notifier

```csharp
public sealed class PagerDutyNotifier : IArknNotifier
{
    public async Task NotifyAsync(ArknNotification notification, CancellationToken ct = default)
    {
        // POST to PagerDuty Events API v2
    }
}

// Register
builder.Services.AddArknNotifications(n => n.AddNotifier<PagerDutyNotifier>());
```
