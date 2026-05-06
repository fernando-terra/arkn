using Arkn.Notifications.Abstractions;
using Arkn.Notifications.Models;
using System.Text;

namespace Arkn.Extensions.Notifications.Teams;

/// <summary>
/// Sends Arkn notifications to Microsoft Teams via Incoming Webhook.
/// Uses Adaptive Cards format — no external SDK required.
/// </summary>
public sealed class TeamsNotifier : IArknNotifier
{
    private readonly HttpClient          _http;
    private readonly TeamsNotifierOptions _options;

    /// <summary>Initializes the notifier with an <see cref="HttpClient"/> and options.</summary>
    public TeamsNotifier(HttpClient http, TeamsNotifierOptions options)
    {
        _http    = http;
        _options = options;
    }

    /// <inheritdoc />
    public async Task NotifyAsync(ArknNotification notification, CancellationToken cancellationToken = default)
    {
        if (notification.Level < _options.MinimumLevel) return;
        if (string.IsNullOrWhiteSpace(_options.WebhookUrl))  return;

        var json    = TeamsCardBuilder.Build(notification, _options);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_options.Timeout);

            var response = await _http.PostAsync(_options.WebhookUrl, content, cts.Token);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            // Swallow — notifiers must never crash the caller
        }
    }
}
