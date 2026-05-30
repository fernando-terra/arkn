# Arkn.Extensions.Repository.EntityFrameworkCore

EntityFrameworkCore implementation of the Arkn Repository abstractions.

## Overview

This package provides the concrete implementation of the repository and unit of work patterns using Microsoft's Entity Framework Core. It serves as a bridge between the agnostic domain contracts and the persistence layer.

## Key Components

### EfRepository<TEntity, TId>

A base class for implementing repositories using EF Core. It implements `IArknRepository<TEntity, TId>` and provides access to the underlying `DbContext` and `DbSet`.

```csharp
public class UserRepository : EfRepository<User, Guid>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.Email == email);
    }
}
```

### EfUnitOfWork

A implementation of `IUnitOfWork` that wraps the `DbContext.SaveChangesAsync()` method.

## Registration

Register your repositories and unit of work in your Dependency Injection container:

```csharp
services.AddScoped<IUnitOfWork, EfUnitOfWork>();
services.AddScoped<IUserRepository, UserRepository>();
```
