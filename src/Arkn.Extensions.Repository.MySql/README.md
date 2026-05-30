# Arkn.Extensions.Repository.MySql

Simplified MySql/MariaDB configuration for Arkn Repository (EF Core).

## Overview

This package provides extension methods for `IServiceCollection` to easily configure Entity Framework Core with MySQL or MariaDB (using Pomelo) within an Arkn project.

## Usage

```csharp
using Arkn.Extensions.Repository.MySql;

public void ConfigureServices(IServiceCollection services)
{
    services.AddArknMySql<AppDbContext>(
        "server=localhost;database=arkn;user=root;password=password"
    );
}
```
