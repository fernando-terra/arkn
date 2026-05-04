# Arkn.Notifications

> **Conventions you can read. Patterns you can enforce.**

Pluggable notifier abstraction — fan-out to multiple channels with zero external dependencies.

## Install

```bash
dotnet add package Arkn.Notifications
```

## Quick example

```csharp
// Implement a custom notifier
public class EmailNotifier : IArknNotifier
{
    public async Task NotifyAsync(ArknNotification notification, CancellationToken ct = default)
    {
        if (notification.Level < NotificationLevel.Warning) return;
        // send email...
    }
}

// Register
builder.Services.AddArknNotifications(n =>
{
    n.Add<EmailNotifier>();
});

// Send a notification
await registry.DispatchAsync(ArknNotification.Error(
    title:  "Job failed",
    body:   "InvoiceProcessor failed after 3 attempts.",
    source: "Arkn.Jobs"));
```

## Part of the Arkn ecosystem

[github.com/fernando-terra/arkn](https://github.com/fernando-terra/arkn) · [nuget.org/packages/Arkn.Notifications](https://www.nuget.org/packages/Arkn.Notifications)
