using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Arkn.Extensions.Repository.PostgreSql;

public static class DependencyInjection
{
    /// <summary>
    /// Configures Arkn Repository to use PostgreSQL with EntityFrameworkCore.
    /// </summary>
    public static IServiceCollection AddArknPostgreSql<TContext>(
        this IServiceCollection services,
        string connectionString,
        Action<Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.NpgsqlDbContextOptionsBuilder>? npgsqlOptions = null)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions));

        return services;
    }
}
