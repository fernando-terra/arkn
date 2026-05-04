using Arkn.Notifications.Models;

namespace Arkn.Notifications.Abstractions;

/// <summary>
/// Dispatches a notification to all registered <see cref="IArknNotifier"/> instances.
/// </summary>
public interface IArknNotifierRegistry
{
    Task DispatchAsync(ArknNotification notification, CancellationToken cancellationToken = default);
}
