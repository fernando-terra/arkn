# Arkn.Extensions.Notifications.Email

Email notifier for `Arkn.Notifications` — SMTP and SendGrid support with HTML/plain-text bodies. Zero external SDK dependency.

## Channels

| Channel   | Transport                          | Dependencies      |
|-----------|------------------------------------|-------------------|
| SMTP      | `System.Net.Mail.SmtpClient`       | .NET BCL only     |
| SendGrid  | HTTP REST (`api.sendgrid.com/v3`)  | .NET BCL only     |

## Quick Start

```csharp
builder.Services.AddArknNotifications(n =>
{
    // SMTP
    n.AddSmtpEmailNotifier(opts =>
    {
        opts.Host     = "smtp.example.com";
        opts.Port     = 587;
        opts.Username = "user";
        opts.Password = "pass";
        opts.From     = new EmailAddress("noreply@example.com", "Arkn");
        opts.To       = [new EmailAddress("ops@example.com")];
        opts.MinimumLevel = NotificationLevel.Warning;
    });

    // SendGrid
    n.AddSendGridNotifier(opts =>
    {
        opts.ApiKey = "SG.your-key-here";
        opts.From   = new EmailAddress("noreply@example.com", "Arkn");
        opts.To     = [new EmailAddress("ops@example.com")];
    });
});
```

## License

Apache-2.0 — see [LICENSE](../../LICENSE).
