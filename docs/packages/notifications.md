# Arkn.Notifications

Pluggable notification abstractions — fan-out to N destinations.

```bash
dotnet add package Arkn.Notifications
dotnet add package Arkn.Extensions.Notifications.Slack
dotnet add package Arkn.Extensions.Notifications.Email
dotnet add package Arkn.Extensions.Notifications.Teams   # v0.3.0+
dotnet add package Arkn.Extensions.Notifications.Discord # v0.3.0+
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

    n.AddTeamsNotifier(teams =>
    {
        teams.WebhookUrl   = config["Teams:WebhookUrl"];
        teams.MinimumLevel = NotificationLevel.Warning;
    });

    n.AddDiscordNotifier(discord =>
    {
        discord.WebhookUrl   = config["Discord:WebhookUrl"];
        discord.Username     = "Arkn";
        discord.MinimumLevel = NotificationLevel.Warning;
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

## Microsoft Teams — Adaptive Cards <Badge type="tip" text="v0.3.0" />

```bash
dotnet add package Arkn.Extensions.Notifications.Teams
```

Uses **Adaptive Cards 1.4** via Incoming Webhook — no Teams SDK required.

- Level-coloured header container (`good` / `warning` / `attention`)
- `FactSet` rendering for job metadata (RunId, Duration, etc.)
- Monospace log snippet block
- Footer with source tag and UTC timestamp

**Webhook setup:** Teams channel → Connectors → Incoming Webhook → Configure → copy URL.

```csharp
n.AddTeamsNotifier(opts =>
{
    opts.WebhookUrl   = "https://your-org.webhook.office.com/...";
    opts.MinimumLevel = NotificationLevel.Warning;
    opts.MaxLogLines  = 5;
    opts.Timeout      = TimeSpan.FromSeconds(10);
});
```

## Discord — Embeds <Badge type="tip" text="v0.3.0" />

```bash
dotnet add package Arkn.Extensions.Notifications.Discord
```

Uses **Discord Webhook Embeds** — no Discord SDK required.

- Level-coloured sidebar (green / amber / red / dark red)
- Inline metadata fields (RunId, Duration, …)
- Code-block log snippet with configurable max lines
- ISO 8601 timestamp and footer with source tag
- Optional bot username and avatar URL override

**Webhook setup:** Discord channel settings → Integrations → Webhooks → New Webhook → copy URL.

```csharp
n.AddDiscordNotifier(opts =>
{
    opts.WebhookUrl   = "https://discord.com/api/webhooks/...";
    opts.Username     = "Arkn";
    opts.AvatarUrl    = "https://example.com/arkn-avatar.png";
    opts.MinimumLevel = NotificationLevel.Warning;
    opts.MaxLogLines  = 5;
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

## Notification levels

| Level | Value | Default minimum |
|---|---|---|
| `Info` | 0 | — |
| `Warning` | 1 | ✅ Most notifiers default here |
| `Error` | 2 | — |
| `Critical` | 3 | — |

## Wiring into Arkn.Jobs

```csharp
jobs.Add<InvoiceProcessorJob>("0 2 * * *")
    .NotifyOn(JobEvent.Failed | JobEvent.TimedOut);

// Global fallback — fires on any unhandled job failure
jobs.OnFailure<SlackNotifier>();
```
