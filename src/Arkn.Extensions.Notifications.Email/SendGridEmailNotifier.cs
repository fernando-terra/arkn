using System.Net;
using System.Text;
using System.Text.Json;
using Arkn.Notifications.Abstractions;
using Arkn.Notifications.Models;

namespace Arkn.Extensions.Notifications.Email;

/// <summary>
/// Sends Arkn notifications via SendGrid REST API.
/// Uses <see cref="HttpClient"/> directly — no SendGrid SDK required.
/// </summary>
public sealed class SendGridEmailNotifier : IArknNotifier, IDisposable
{
    private readonly HttpClient           _http;
    private readonly SendGridEmailOptions _options;

    private const string ApiUrl = "https://api.sendgrid.com/v3/mail/send";

    public SendGridEmailNotifier(SendGridEmailOptions options) : this(new HttpClient(), options) { }

    internal SendGridEmailNotifier(HttpClient http, SendGridEmailOptions options)
    {
        _http    = http;
        _options = options;
        _http.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);
    }

    public async Task NotifyAsync(ArknNotification notification, CancellationToken cancellationToken = default)
    {
        if (notification.Level < _options.MinimumLevel) return;
        if (_options.To.Count == 0 || string.IsNullOrWhiteSpace(_options.ApiKey)) return;

        try
        {
            var payload = BuildPayload(notification);
            var json    = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented        = false,
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _http.PostAsync(ApiUrl, content, cancellationToken);
            // 202 Accepted = success. Ignore failures silently.
        }
        catch
        {
            // Must not crash caller
        }
    }

    private object BuildPayload(ArknNotification notification)
    {
        var subject  = $"{_options.SubjectPrefix} {notification.Title}";
        var htmlBody = BuildHtml(notification);
        var txtBody  = BuildPlain(notification);

        return new
        {
            personalizations = new[]
            {
                new
                {
                    to = _options.To.Select(t => new { email = t.Address, name = t.DisplayName }).ToArray()
                }
            },
            from    = new { email = _options.From.Address, name = _options.From.DisplayName },
            subject = subject,
            content = new[]
            {
                new { type = "text/plain", value = txtBody },
                new { type = "text/html",  value = htmlBody },
            }
        };
    }

    private static string BuildPlain(ArknNotification n)
    {
        var sb = new StringBuilder();
        sb.AppendLine(n.Title);
        sb.AppendLine(n.Body);
        sb.AppendLine($"Level: {n.Level} | Source: {n.Source} | {n.Timestamp:yyyy-MM-dd HH:mm} UTC");
        if (n.Metadata is { Count: > 0 })
            foreach (var (k, v) in n.Metadata)
                sb.AppendLine($"{k}: {v}");
        return sb.ToString();
    }

    private static string BuildHtml(ArknNotification n)
    {
        var color = n.Level switch
        {
            NotificationLevel.Info     => "#1a73e8",
            NotificationLevel.Warning  => "#f9a825",
            NotificationLevel.Error    => "#d32f2f",
            NotificationLevel.Critical => "#7b0000",
            _                          => "#555",
        };

        var sb = new StringBuilder();
        sb.Append("<html><body style=\"font-family:sans-serif;padding:20px\">");
        sb.Append($"<h2 style=\"color:{color}\">{WebUtility.HtmlEncode(n.Title)}</h2>");
        sb.Append($"<p>{WebUtility.HtmlEncode(n.Body)}</p>");
        sb.Append($"<p><b>Level:</b> {n.Level} | <b>Source:</b> {WebUtility.HtmlEncode(n.Source)} | {n.Timestamp:yyyy-MM-dd HH:mm} UTC</p>");

        if (n.Metadata is { Count: > 0 })
        {
            sb.Append("<ul>");
            foreach (var (k, v) in n.Metadata)
                sb.Append($"<li><b>{WebUtility.HtmlEncode(k)}:</b> {WebUtility.HtmlEncode(v?.ToString() ?? "")}</li>");
            sb.Append("</ul>");
        }

        sb.Append("</body></html>");
        return sb.ToString();
    }

    public void Dispose() => _http.Dispose();
}
