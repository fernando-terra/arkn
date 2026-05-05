using Arkn.Notifications.Models;

namespace Arkn.Extensions.Notifications.Email;

/// <summary>Configuration for the SMTP email notifier.</summary>
public sealed class SmtpEmailOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }

    public EmailAddress From { get; set; } = new("noreply@example.com", "Arkn");
    public List<EmailAddress> To { get; set; } = [];

    /// <summary>Optional CC recipients.</summary>
    public List<EmailAddress> Cc { get; set; } = [];

    /// <summary>Subject prefix prepended to the notification title.</summary>
    public string SubjectPrefix { get; set; } = "[Arkn]";

    /// <summary>Minimum notification level to send.</summary>
    public NotificationLevel MinimumLevel { get; set; } = NotificationLevel.Warning;

    /// <summary>Send HTML body in addition to plain text.</summary>
    public bool SendHtml { get; set; } = true;

    /// <summary>Timeout in milliseconds for SMTP operations.</summary>
    public int TimeoutMs { get; set; } = 15_000;
}
