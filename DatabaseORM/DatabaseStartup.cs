using DatabaseORM.Context;
using DatabaseORM.Repository;
using DatabaseORM.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseORM;

public class DatabaseStartup
{
    public static void ConfigureDatabaseServices(IServiceCollection services, string? databaseConnectionString)
    {
        services.AddDbContext<KecskeDatabaseContext>(options =>
            options
                .UseSqlServer(databaseConnectionString)
#if DEBUG
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
#endif
        );

        services.AddScoped(typeof(GenericRepository<>));

        services.AddScoped(typeof(GenericService<>));
        services.AddScoped<AccountService>();
        services.AddScoped<FileDirectoryService>();
        services.AddScoped<PermittedIpAddressService>();
    }
}
