using Arkn.Notifications.Abstractions;
using Arkn.Notifications.Models;

namespace Arkn.Notifications.Core;

/// <summary>
/// Default registry that fans out to all registered notifiers concurrently.
/// Individual notifier failures are swallowed to protect the dispatch pipeline.
/// </summary>
public sealed class ArknNotifierRegistry : IArknNotifierRegistry
{
    private readonly IReadOnlyList<IArknNotifier> _notifiers;

    /// <summary>Initializes the registry with the provided notifiers.</summary>
    public ArknNotifierRegistry(IEnumerable<IArknNotifier> notifiers)
        => _notifiers = notifiers.ToList().AsReadOnly();

    /// <inheritdoc />
    public async Task DispatchAsync(ArknNotification notification, CancellationToken cancellationToken = default)
    {
        var tasks = _notifiers.Select(n => SafeNotifyAsync(n, notification, cancellationToken));
        await Task.WhenAll(tasks);
    }

    private static async Task SafeNotifyAsync(
        IArknNotifier notifier,
        ArknNotification notification,
        CancellationToken cancellationToken)
    {
        try { await notifier.NotifyAsync(notification, cancellationToken); }
        catch { /* notifiers must never crash the dispatch pipeline */ }
    }
}
