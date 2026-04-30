namespace Arkn.Core.Abstractions;

/// <summary>
/// Unit of Work abstraction.
/// Coordinates the writing-out of changes tracked during a business transaction.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
