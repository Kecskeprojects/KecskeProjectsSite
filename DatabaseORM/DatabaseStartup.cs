using DatabaseORM.Constants;
using DatabaseORM.Context;
using DatabaseORM.Mapping.MappingProfiles;
using DatabaseORM.Repository;
using DatabaseORM.Service;
using DatabaseORM.Tools.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseORM;

public class DatabaseStartup
{
    public static void ConfigureDatabaseServices(ConfigurationManager configuration, IServiceCollection services)
    {
        string? databaseConnectionString = configuration.GetConnectionString(ConfigurationKeys.DatabaseConnection);
        bool isProduction = configuration.IsProduction();

        if (string.IsNullOrWhiteSpace(databaseConnectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured.");
        }

        // Register Mapster mappings
        MappingConfiguration.RegisterMappings();

        _ = services.AddDbContext<KecskeDatabaseContext>(options => ConfigureDbContext(options, databaseConnectionString, isProduction));

        _ = services.AddScoped(typeof(GenericRepository<>));

        _ = services.AddScoped<AccountService>();
        _ = services.AddScoped<FileDirectoryService>();
        _ = services.AddScoped<PermittedIpAddressService>();
    }

    #region Configuration Methods
    private static void ConfigureDbContext(DbContextOptionsBuilder options, string connectionString, bool isProduction)
    {
        _ = options.UseSqlServer(connectionString)
            .EnableSensitiveDataLogging(!isProduction)
            .EnableDetailedErrors(!isProduction);
    }
    #endregion
}
