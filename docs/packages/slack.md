# Arkn.Extensions.Notifications.Slack

Slack notifier for Arkn.Notifications — sends rich messages via **Incoming Webhook** using Block Kit. **Zero external SDK dependency.**

```bash
dotnet add package Arkn.Extensions.Notifications.Slack
```

## Setup

```csharp
builder.Services.AddArknNotifications(n =>
{
    n.AddSlackNotifier(slack =>
    {
        slack.WebhookUrl    = config["Slack:WebhookUrl"]; // https://hooks.slack.com/services/...
        slack.Channel       = "#alerts";                  // optional: override webhook default
        slack.Username      = "Arkn";                     // optional: bot display name
        slack.IconEmoji     = ":robot_face:";             // optional: bot icon
        slack.MinimumLevel  = NotificationLevel.Warning;  // drop anything below this
        slack.MaxLogLines   = 5;                          // max log lines in message snippet
    });
});
```

## Block Kit message format

Each notification is rendered as a Slack Block Kit message with:

- **Header block** — emoji + title, color-coded by level
- **Body section** — notification message body
- **Fields section** — metadata key-value pairs (up to 10)
- **Log snippet** — last N log lines in a code block (when `logs` metadata is present)
- **Context footer** — source, level, timestamp UTC

Level color scheme:

| Level | Emoji | Color |
|---|---|---|
| Info | ℹ️ | `#36a64f` (green) |
| Warning | ⚠️ | `#ff9900` (amber) |
| Error | ❌ | `#cc0000` (red) |
| Critical | 🚨 | `#7b0000` (dark red) |

## Integration with Arkn.Jobs

When a job fails or times out, Arkn.Jobs automatically dispatches a notification with the run context:

```csharp
builder.Services.AddArknJobs(jobs =>
{
    jobs.Add<InvoiceProcessorJob>("0 2 * * *")
        .NotifyOn(JobEvent.Failed | JobEvent.TimedOut);  // per-job

    jobs.OnFailure<SlackNotifier>();  // global fallback for all jobs
});
```

The Slack message will include:
- Job name and final status
- Run duration
- Short RunId
- Error message (on failure)
- Last 5 log lines from that run

## Manual dispatch

```csharp
public class AlertService(IArknNotifierRegistry notifier)
{
    public async Task AlertAsync(string title, string body)
    {
        await notifier.DispatchAsync(
            ArknNotification.Error(title, body, "MyService",
                metadata: new Dictionary<string, object?>
                {
                    ["Environment"] = "production",
                    ["RunId"]       = Guid.NewGuid().ToString(),
                }));
    }
}
```

## Custom notifier

If you need more control over the Slack payload, implement `IArknNotifier` directly and register it:

```csharp
public sealed class CustomSlackNotifier : IArknNotifier
{
    public async Task NotifyAsync(ArknNotification notification, CancellationToken ct = default)
    {
        // Build your own Block Kit payload
    }
}

builder.Services.AddArknNotifications(n => n.AddNotifier<CustomSlackNotifier>());
```
