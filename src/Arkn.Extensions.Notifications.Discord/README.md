# Arkn.Extensions.Notifications.Discord

Discord notifier for [Arkn](https://github.com/fernando-terra/arkn) via Webhook Embeds.

## Features

- Zero external dependencies — uses `System.Text.Json` only
- Rich Discord Embeds with level-based sidebar colours
- Inline metadata fields (RunId, Duration, etc.)
- Log snippet rendered in a monospace code block
- Configurable minimum level, username, avatar, timeout, and log line limit

## Usage

```csharp
builder.Services
    .AddArknNotifications(n => n
        .AddDiscordNotifier(opts =>
        {
            opts.WebhookUrl   = "https://discord.com/api/webhooks/...";
            opts.Username     = "Arkn";
            opts.MinimumLevel = NotificationLevel.Warning;
            opts.MaxLogLines  = 5;
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

1. In Discord, open channel settings → **Integrations** → **Webhooks** → **New Webhook**
2. Copy the webhook URL and assign it to `WebhookUrl`
