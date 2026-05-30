using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Arkn.Extensions.Repository.MySql;

public static class DependencyInjection
{
    /// <summary>
    /// Configures Arkn Repository to use MySql with EntityFrameworkCore (Pomelo).
    /// </summary>
    public static IServiceCollection AddArknMySql<TContext>(
        this IServiceCollection services,
        string connectionString,
        ServerVersion? serverVersion = null)
        where TContext : DbContext
    {
        var version = serverVersion ?? ServerVersion.AutoDetect(connectionString);

        services.AddDbContext<TContext>(options =>
            options.UseMySql(connectionString, version));

        return services;
    }
}
