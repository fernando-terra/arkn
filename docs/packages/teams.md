# Arkn.Extensions.Notifications.Teams

Microsoft Teams notifier for Arkn via Incoming Webhook and Adaptive Cards 1.4.

```bash
dotnet add package Arkn.Extensions.Notifications.Teams
```

## Features

- **Adaptive Cards 1.4** format (modern Teams webhook payload)
- Level-coloured `Container` header: `good` (info), `warning`, `attention` (error/critical)
- `FactSet` rendering for job metadata (RunId, Duration, Status…)
- Monospace log snippet rendering
- Footer with source tag and UTC timestamp
- Configurable minimum level, max log lines, and HTTP timeout
- Zero external dependencies — `System.Text.Json` only

## Setup

```csharp
using Arkn.Extensions.Notifications.Teams.Extensions;

builder.Services.AddArknNotifications(n =>
    n.AddTeamsNotifier(opts =>
    {
        opts.WebhookUrl   = config["Teams:WebhookUrl"];
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
    jobs.OnFailure<TeamsNotifier>();
});
```

## Webhook setup

1. In Teams, open the channel → **...** → **Connectors** → **Incoming Webhook** → Configure
2. Give it a name (e.g. "Arkn Alerts") and copy the webhook URL
3. Assign the URL to `WebhookUrl` in your options

## Payload structure

The generated card follows this structure:

```
┌─────────────────────────────────────┐
│ ❌ InvoiceProcessorJob — Failed      │  ← coloured Container (attention)
├─────────────────────────────────────┤
│ Invoice processor failed after 3     │  ← body text
│ attempts                             │
├─────────────────────────────────────┤
│ RunId          │ Duration            │  ← FactSet (metadata)
│ abc-123…       │ 02:13               │
├─────────────────────────────────────┤
│ Recent logs:                         │  ← monospace snippet (if logs present)
│ [Error] NullReferenceException…      │
├─────────────────────────────────────┤
│ Source: Arkn.Jobs/InvoiceJob · Error │  ← footer
└─────────────────────────────────────┘
```
