# Arkn.Extensions.Notifications.Discord

Discord notifier for Arkn via Webhook Embeds.

```bash
dotnet add package Arkn.Extensions.Notifications.Discord
```

## Features

- **Discord Embed** format with level-coloured sidebar
- Level colours: `#36a64f` (info), `#ff9900` (warning), `#cc0000` (error), `#7b0000` (critical)
- Inline metadata fields (RunId, Duration, Status…)
- Code-block log snippet with configurable max lines
- ISO 8601 timestamp and footer with source tag
- Optional bot username and avatar URL override
- Zero external dependencies — `System.Text.Json` only

## Setup

```csharp
using Arkn.Extensions.Notifications.Discord.Extensions;

builder.Services.AddArknNotifications(n =>
    n.AddDiscordNotifier(opts =>
    {
        opts.WebhookUrl   = config["Discord:WebhookUrl"];
        opts.Username     = "Arkn";
        opts.AvatarUrl    = "https://example.com/arkn-avatar.png";
        opts.MinimumLevel = NotificationLevel.Warning;
        opts.MaxLogLines  = 5;
        opts.Timeout      = TimeSpan.FromSeconds(10);
    }));
```

## Wiring into Arkn.Jobs

```csharp
builder.Services.AddArknJobs(jobs =>
{
    jobs.Add<InvoiceProcessorJob>("0 2 * * *")
        .NotifyOn(JobEvent.Failed | JobEvent.TimedOut);

    // or as global fallback
    jobs.OnFailure<DiscordNotifier>();
});
```

## Webhook setup

1. In Discord, open channel settings → **Integrations** → **Webhooks** → **New Webhook**
2. Optionally set a name and avatar
3. Copy the webhook URL and assign it to `WebhookUrl`

## Payload structure

```
┌─ ❌ InvoiceProcessorJob — Failed ──────────────────────┐
│  Invoice processor failed after 3 attempts             │
│                                                         │
│  RunId           Duration        Status                 │
│  abc-123…        02:13           Failed                 │
│                                                         │
│  Recent logs:                                           │
│  ```                                                    │
│  [Error] NullReferenceException in InvoiceService       │
│  ```                                                    │
│                                        ◀ red sidebar   │
│  Source: Arkn.Jobs/InvoiceJob · Error                  │
│  2026-05-06T02:13:00Z                                   │
└─────────────────────────────────────────────────────────┘
```
