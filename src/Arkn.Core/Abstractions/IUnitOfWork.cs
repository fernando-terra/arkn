namespace Arkn.Core.Abstractions;

/// <summary>
/// Unit of Work abstraction.
/// Coordinates the writing-out of changes tracked during a business transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Persists all pending changes and returns the number of affected records.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
