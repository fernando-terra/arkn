using Arkn.Notifications.Abstractions;
using Arkn.Notifications.Models;
using System.Text;

namespace Arkn.Extensions.Notifications.Slack;

/// <summary>
/// Sends Arkn notifications to Slack via Incoming Webhook.
/// Uses <see cref="HttpClient"/> directly — no external HTTP library required.
/// </summary>
public sealed class SlackNotifier : IArknNotifier
{
    private readonly HttpClient           _http;
    private readonly SlackNotifierOptions _options;

    public SlackNotifier(HttpClient http, SlackNotifierOptions options)
    {
        _http    = http;
        _options = options;
    }

    public async Task NotifyAsync(ArknNotification notification, CancellationToken cancellationToken = default)
    {
        if (notification.Level < _options.MinimumLevel) return;
        if (string.IsNullOrWhiteSpace(_options.WebhookUrl)) return;

        var json    = SlackBlockBuilder.Build(notification, _options);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _http.PostAsync(_options.WebhookUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            // Swallow — notifiers must not crash the caller
        }
    }
}
