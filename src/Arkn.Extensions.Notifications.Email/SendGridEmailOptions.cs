using Arkn.Notifications.Models;

namespace Arkn.Extensions.Notifications.Email;

/// <summary>Configuration for the SendGrid email notifier (HTTP API, no SDK).</summary>
public sealed class SendGridEmailOptions
{
    /// <summary>SendGrid API key (starts with "SG.").</summary>
    public string ApiKey { get; set; } = string.Empty;

    public EmailAddress From { get; set; } = new("noreply@example.com", "Arkn");
    public List<EmailAddress> To { get; set; } = [];

    public string SubjectPrefix { get; set; } = "[Arkn]";
    public NotificationLevel MinimumLevel { get; set; } = NotificationLevel.Warning;

    /// <summary>Request timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 15;
}
