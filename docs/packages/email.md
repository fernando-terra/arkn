# Arkn.Extensions.Notifications.Email

Email notifier for Arkn.Notifications — SMTP via `System.Net.Mail` and SendGrid via HTTP. **Zero external SDK dependency.**

```bash
dotnet add package Arkn.Extensions.Notifications.Email
```

## SMTP (System.Net.Mail)

```csharp
builder.Services.AddArknNotifications(n =>
{
    n.AddSmtpEmailNotifier(smtp =>
    {
        smtp.Host        = "smtp.example.com";
        smtp.Port        = 587;
        smtp.EnableSsl   = true;
        smtp.Username    = config["Email:Username"];
        smtp.Password    = config["Email:Password"];

        smtp.From        = new EmailAddress("noreply@example.com", "Arkn Alerts");
        smtp.To          = [new EmailAddress("ops@example.com", "On-call Team")];
        smtp.Cc          = [new EmailAddress("manager@example.com")];

        smtp.SubjectPrefix  = "[Arkn]";
        smtp.MinimumLevel   = NotificationLevel.Warning;
        smtp.SendHtml       = true;    // sends HTML + plain-text alternative
        smtp.TimeoutMs      = 15_000;
    });
});
```

## SendGrid (HTTP API, no SDK)

```csharp
builder.Services.AddArknNotifications(n =>
{
    n.AddSendGridNotifier(sg =>
    {
        sg.ApiKey        = config["SendGrid:ApiKey"];  // starts with "SG."
        sg.From          = new EmailAddress("noreply@example.com", "Arkn Alerts");
        sg.To            = [new EmailAddress("ops@example.com")];
        sg.SubjectPrefix = "[Arkn]";
        sg.MinimumLevel  = NotificationLevel.Error;
        sg.TimeoutSeconds = 15;
    });
});
```

## Email body

Both notifiers send **HTML + plain-text** automatically:

- HTML — color-coded by level, metadata table, structured layout
- Plain text — fallback for clients that don't render HTML

Level color scheme:

| Level | Color |
|---|---|
| Info | `#1a73e8` (blue) |
| Warning | `#f9a825` (amber) |
| Error | `#d32f2f` (red) |
| Critical | `#7b0000` (dark red) |

## Combined with Slack

```csharp
builder.Services.AddArknNotifications(n =>
{
    n.AddSmtpEmailNotifier(smtp => { /* ... */ });
    n.AddSlackNotifier(slack => { /* ... */ });
    // Both notifiers fire concurrently for every DispatchAsync call
});
```
