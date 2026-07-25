using DatabaseORM.Constants;
using DatabaseORM.Context;
using DatabaseORM.Mapping.MappingProfiles;
using DatabaseORM.Repository;
using DatabaseORM.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseORM;

public class DatabaseStartup
{
    public static void ConfigureDatabaseServices(ConfigurationManager configuration, IServiceCollection services)
    {
        string? databaseConnectionString = configuration.GetConnectionString(ConfigurationKeys.DatabaseConnection);
        string? environment = configuration[ConfigurationKeys.Environment];

        if (string.IsNullOrWhiteSpace(databaseConnectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured.");
        }

        // Register Mapster mappings
        MappingConfiguration.RegisterMappings();

        _ = services.AddDbContext<KecskeDatabaseContext>(options =>
                options.UseSqlServer(databaseConnectionString)
                    .EnableSensitiveDataLogging(environment != EnvironmentConstants.Production)
                    .EnableDetailedErrors(environment != EnvironmentConstants.Production)
        );

        _ = services.AddScoped(typeof(GenericRepository<>));

        _ = services.AddScoped(typeof(GenericService<>));
        _ = services.AddScoped<AccountService>();
        _ = services.AddScoped<FileDirectoryService>();
        _ = services.AddScoped<PermittedIpAddressService>();
    }
}
