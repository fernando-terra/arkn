using Arkn.Core.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Arkn.Extensions.Repository.EntityFrameworkCore;

/// <summary>
/// EntityFrameworkCore implementation of IUnitOfWork.
/// </summary>
public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;

    public EfUnitOfWork(DbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
