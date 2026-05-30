using System.Linq.Expressions;
using Arkn.Core.Abstractions;

namespace Arkn.Repository.Abstractions;

/// <summary>
/// Simplified repository interface for Arkn.
/// Provides basic CRUD and predicate-based querying.
/// Specialized queries should be added to domain-specific repository interfaces.
/// </summary>
public interface IArknRepository<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : class, IAggregateRoot
{
    /// <summary>Lists all entities.</summary>
    Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists entities matching the predicate.</summary>
    Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>Checks if any entity matches the predicate.</summary>
    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>Counts entities matching the predicate.</summary>
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);
}
