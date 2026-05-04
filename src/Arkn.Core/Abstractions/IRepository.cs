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
    /// <summary>Returns the aggregate with the given <paramref name="id"/>, or <c>null</c> if not found.</summary>
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Adds a new <paramref name="aggregate"/> to the store.</summary>
    Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>Marks an existing <paramref name="aggregate"/> as modified.</summary>
    void Update(TAggregate aggregate);

    /// <summary>Marks an existing <paramref name="aggregate"/> for deletion.</summary>
    void Remove(TAggregate aggregate);
}
