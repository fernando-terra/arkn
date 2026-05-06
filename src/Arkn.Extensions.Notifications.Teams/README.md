# Arkn.Extensions.Notifications.Teams

Microsoft Teams notifier for [Arkn](https://github.com/fernando-terra/arkn) via Incoming Webhook and Adaptive Cards.

## Features

- Zero external dependencies — uses `System.Text.Json` only
- Adaptive Cards 1.4 format (modern Teams webhook payload)
- Level-coloured card containers (good / warning / attention)
- Fact set rendering for job metadata (RunId, Duration, etc.)
- Log snippet support with monospace rendering
- Configurable minimum level, timeout, and log line limit

## Usage

```csharp
builder.Services
    .AddArknNotifications(n => n
        .AddTeamsNotifier(opts =>
        {
            opts.WebhookUrl    = "https://your-org.webhook.office.com/...";
            opts.MinimumLevel  = NotificationLevel.Warning;
            opts.MaxLogLines   = 5;
        }));
```

Or as part of job registration:

```csharp
builder.Services.AddArknJobs(jobs =>
{
    jobs.Add<InvoiceJob>("0 2 * * *")
        .NotifyOn(JobEvent.Failed | JobEvent.TimedOut);
});
```

## Webhook setup

1. In Teams, open the channel → **Connectors** → **Incoming Webhook** → Configure
2. Copy the webhook URL and assign it to `WebhookUrl`
