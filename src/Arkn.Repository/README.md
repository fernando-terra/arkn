# Arkn.Repository

Simplified, agnostic repository abstractions for modern .NET applications.

## Overview

The `Arkn.Repository` package provides the core contracts for data access within the Arkn ecosystem. It is designed to be completely independent of any specific database technology or ORM, following the principles of Clean Architecture and DDD.

## Core Abstractions

### IArknRepository<TEntity, TId>

A generic repository interface that provides standard CRUD operations and simple predicate-based querying.

```csharp
public interface IArknRepository<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : class, IAggregateRoot
{
    Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
}
```

## Usage

Define your domain-specific repository interfaces by inheriting from `IArknRepository`:

```csharp
public interface IUserRepository : IArknRepository<User, Guid>
{
    // Add specialized queries here
    Task<User?> GetByEmailAsync(string email);
}
```

## Extensions

Implementations are provided in separate extension packages:
- `Arkn.Extensions.Repository.EntityFrameworkCore`: EF Core implementation.
- `Arkn.Extensions.Repository.PostgreSql`: PostgreSql configuration.
- `Arkn.Extensions.Repository.MySql`: MySql configuration.
