using System.Net;
using System.Net.Mail;
using System.Text;
using Arkn.Notifications.Abstractions;
using Arkn.Notifications.Models;

namespace Arkn.Extensions.Notifications.Email;

/// <summary>
/// Sends Arkn notifications via SMTP using <see cref="System.Net.Mail.SmtpClient"/>.
/// Zero external dependencies — uses only the .NET BCL.
/// </summary>
public sealed class SmtpEmailNotifier : IArknNotifier
{
    private readonly SmtpEmailOptions _options;

    public SmtpEmailNotifier(SmtpEmailOptions options) => _options = options;

    public async Task NotifyAsync(ArknNotification notification, CancellationToken cancellationToken = default)
    {
        if (notification.Level < _options.MinimumLevel) return;
        if (_options.To.Count == 0) return;

        try
        {
            using var client = BuildClient();
            using var message = BuildMessage(notification);
            await client.SendMailAsync(message, cancellationToken);
        }
        catch
        {
            // Notifiers must never crash the caller
        }
    }

    private SmtpClient BuildClient()
    {
        var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Timeout   = _options.TimeoutMs,
        };

        if (_options.Username is not null && _options.Password is not null)
            client.Credentials = new NetworkCredential(_options.Username, _options.Password);

        return client;
    }

    private MailMessage BuildMessage(ArknNotification notification)
    {
        var subject = $"{_options.SubjectPrefix} {notification.Title}";
        var from    = new MailAddress(_options.From.Address, _options.From.DisplayName);

        var message = new MailMessage
        {
            From       = from,
            Subject    = subject,
            IsBodyHtml = _options.SendHtml,
        };

        foreach (var to in _options.To)
            message.To.Add(new MailAddress(to.Address, to.DisplayName));

        foreach (var cc in _options.Cc)
            message.CC.Add(new MailAddress(cc.Address, cc.DisplayName));

        message.Body = _options.SendHtml
            ? BuildHtmlBody(notification)
            : BuildPlainBody(notification);

        if (_options.SendHtml)
        {
            // Also attach plain text alternative
            var plain = new AlternateView(
                new System.IO.MemoryStream(Encoding.UTF8.GetBytes(BuildPlainBody(notification))),
                "text/plain; charset=utf-8");
            message.AlternateViews.Add(plain);
        }

        return message;
    }

    private static string BuildPlainBody(ArknNotification notification)
    {
        var sb = new StringBuilder();
        sb.AppendLine(notification.Title);
        sb.AppendLine(new string('-', notification.Title.Length));
        sb.AppendLine(notification.Body);
        sb.AppendLine();
        sb.AppendLine($"Level:  {notification.Level}");
        sb.AppendLine($"Source: {notification.Source}");
        sb.AppendLine($"Time:   {notification.Timestamp:yyyy-MM-dd HH:mm:ss} UTC");

        if (notification.Metadata is { Count: > 0 })
        {
            sb.AppendLine();
            sb.AppendLine("Details:");
            foreach (var (k, v) in notification.Metadata)
                sb.AppendLine($"  {k}: {v}");
        }

        return sb.ToString();
    }

    private static string BuildHtmlBody(ArknNotification notification)
    {
        var levelColor = notification.Level switch
        {
            NotificationLevel.Info     => "#1a73e8",
            NotificationLevel.Warning  => "#f9a825",
            NotificationLevel.Error    => "#d32f2f",
            NotificationLevel.Critical => "#7b0000",
            _                          => "#555555",
        };

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><body style=\"font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px\">");
        sb.AppendLine($"<div style=\"border-left:4px solid {levelColor};padding-left:16px;margin-bottom:20px\">");
        sb.AppendLine($"  <h2 style=\"color:{levelColor};margin:0 0 8px 0\">{WebUtility.HtmlEncode(notification.Title)}</h2>");
        sb.AppendLine($"  <p style=\"margin:0;color:#333\">{WebUtility.HtmlEncode(notification.Body)}</p>");
        sb.AppendLine("</div>");

        sb.AppendLine("<table style=\"width:100%;border-collapse:collapse;margin-bottom:16px\">");
        sb.AppendLine($"  <tr><td style=\"padding:4px 8px;background:#f5f5f5;font-weight:bold\">Level</td><td style=\"padding:4px 8px\">{notification.Level}</td></tr>");
        sb.AppendLine($"  <tr><td style=\"padding:4px 8px;background:#f5f5f5;font-weight:bold\">Source</td><td style=\"padding:4px 8px\">{WebUtility.HtmlEncode(notification.Source)}</td></tr>");
        sb.AppendLine($"  <tr><td style=\"padding:4px 8px;background:#f5f5f5;font-weight:bold\">Time</td><td style=\"padding:4px 8px\">{notification.Timestamp:yyyy-MM-dd HH:mm:ss} UTC</td></tr>");
        sb.AppendLine("</table>");

        if (notification.Metadata is { Count: > 0 })
        {
            sb.AppendLine("<h4 style=\"color:#555\">Details</h4><table style=\"width:100%;border-collapse:collapse\">");
            foreach (var (k, v) in notification.Metadata)
                sb.AppendLine($"  <tr><td style=\"padding:4px 8px;background:#f5f5f5;font-weight:bold\">{WebUtility.HtmlEncode(k)}</td><td style=\"padding:4px 8px\">{WebUtility.HtmlEncode(v?.ToString() ?? "")}</td></tr>");
            sb.AppendLine("</table>");
        }

        sb.AppendLine("<p style=\"color:#999;font-size:12px;margin-top:24px\">Sent by Arkn.Notifications</p>");
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }
}
