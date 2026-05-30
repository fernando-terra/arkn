# Arkn.Extensions.Repository.PostgreSql

Simplified PostgreSQL configuration for Arkn Repository (EF Core).

## Overview

This package provides extension methods for `IServiceCollection` to easily configure Entity Framework Core with PostgreSQL (Npgsql) within an Arkn project.

## Usage

```csharp
using Arkn.Extensions.Repository.PostgreSql;

public void ConfigureServices(IServiceCollection services)
{
    services.AddArknPostgreSql<AppDbContext>(
        "Host=localhost;Database=arkn;Username=postgres;Password=password"
    );
}
```
