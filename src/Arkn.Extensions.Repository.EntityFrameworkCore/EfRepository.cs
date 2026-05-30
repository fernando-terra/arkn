using System.Linq.Expressions;
using Arkn.Core.Abstractions;
using Arkn.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Arkn.Extensions.Repository.EntityFrameworkCore;

/// <summary>
/// Simplified EntityFrameworkCore implementation of the Arkn Repository.
/// </summary>
public abstract class EfRepository<TEntity, TId> : IArknRepository<TEntity, TId>
    where TEntity : class, IAggregateRoot
{
    protected readonly DbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    protected EfRepository(DbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object?[] { id }, cancellationToken);
    }

    public virtual async Task AddAsync(TEntity aggregate, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(aggregate, cancellationToken);
    }

    public virtual void Update(TEntity aggregate)
    {
        DbSet.Entry(aggregate).State = EntityState.Modified;
    }

    public virtual void Remove(TEntity aggregate)
    {
        DbSet.Remove(aggregate);
    }

    public virtual async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return predicate is null
            ? await DbSet.AnyAsync(cancellationToken)
            : await DbSet.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return predicate is null
            ? await DbSet.CountAsync(cancellationToken)
            : await DbSet.CountAsync(predicate, cancellationToken);
    }
}
