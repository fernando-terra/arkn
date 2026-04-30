namespace Arkn.Core.Abstractions;

/// <summary>
/// Generic repository abstraction for aggregate roots.
/// Infrastructure implementations live in separate packages (e.g., Arkn.Extensions.EfCore).
/// </summary>
/// <typeparam name="TAggregate">The aggregate root type.</typeparam>
/// <typeparam name="TId">The type of the aggregate's identity.</typeparam>
public interface IRepository<TAggregate, TId>
    where TAggregate : IAggregateRoot
{
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
    void Update(TAggregate aggregate);
    void Remove(TAggregate aggregate);
}
