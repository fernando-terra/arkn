# Arkn.Extensions.Notifications.Slack

> **Conventions you can read. Patterns you can enforce.**

Slack notifier for Arkn via Incoming Webhook with Block Kit formatting. Zero external SDKs.

## Install

```bash
dotnet add package Arkn.Extensions.Notifications.Slack
```

## Quick example

```csharp
// Register
builder.Services.AddSlackNotifier(options =>
{
    options.WebhookUrl    = "https://hooks.slack.com/services/...";
    options.Channel       = "#alerts";
    options.Username      = "Arkn";
    options.MinimumLevel  = NotificationLevel.Warning;
});

// Integrate with Arkn.Jobs
builder.Services.AddArknJobs(jobs =>
{
    jobs.Add<InvoiceProcessorJob>("0 2 * * *")
        .NotifyOn(JobEvent.Failed);

    jobs.OnFailure<SlackNotifier>();
});
```

## Block Kit output

Each notification is sent as a rich Slack message with:
- Header with severity emoji (⚠️ ℹ️ ❌ ⛔)
- Body section
- Context bar: source, level, timestamp
- Color-coded attachment sidebar

## Part of the Arkn ecosystem

[github.com/fernando-terra/arkn](https://github.com/fernando-terra/arkn) · [nuget.org/packages/Arkn.Extensions.Notifications.Slack](https://www.nuget.org/packages/Arkn.Extensions.Notifications.Slack)
