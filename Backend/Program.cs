using Backend.Authentication;
using Backend.BackgroundServices;
using Backend.Database;
using Backend.Database.Repository;
using Backend.Database.Service;
using Backend.HostedServices;
using Backend.Services;
using Backend.Tools.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Versioning;

namespace Backend;

[SupportedOSPlatform("windows")]
public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        ConfigureServices(builder.Configuration, builder.Services);

        //Setting custom file logging configuration
        _ = builder.Logging.ClearProviders();
        _ = builder.Logging.AddFileLogger();

        _ = builder.Services.AddControllers();

        _ = builder.Services.AddOpenApi();

        WebApplication app = builder.Build();

        //Swagger is only available during development
        if (app.Environment.IsDevelopment())
        {
            _ = app.MapOpenApi();
            _ = app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "v1");
            });
        }
        else
        {
            _ = app.MapGet("/", () => "The backend is running, now shoo!");
        }

        //Todo: Add proper frontend endpoint for origin, and make localhost only available during development
        _ = app.UseCors(options => options
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins("http://localhost:5173", "http://localhost:4173")
            .AllowCredentials());

        _ = app.UseHttpsRedirection();

        _ = app.UseAuthentication();
        _ = app.UseAuthorization();

        _ = app.MapControllers();

        app.Logger.LogInformation("Starting Kecske Backend");
        app.Run();
    }

    private static void ConfigureServices(ConfigurationManager configuration, IServiceCollection services)
    {
        //Background service responsible for transferring ILogger entries into permanent file logs
        services.AddHostedService<FileLoggerBackgroundService>();

        //Background service for custom RDP connection solution using temporary firewall rules
        //this service is responsible for removing expired firewall rules from the system
        services.AddHostedService<FirewallRuleExpirationWatcher>();

        //Custom authorization overriding built in microsoft cookie authentication
        services.AddTransient<AuthorizationCookieManager>();

        //Custom handler for endpoints that require authentication
        services.AddSingleton<CustomCookieAuthenticationEvents>();

        services
            .AddAuthentication(
                options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
            .AddCookie(
                CookieAuthenticationDefaults.AuthenticationScheme,
                options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
#if DEBUG
                    options.Cookie.SameSite = SameSiteMode.None; //Chrome checks the Same Site mode of the cookie, otherwise the site cannot be tested in chrome
#else
                    options.Cookie.SameSite = SameSiteMode.Strict;
#endif
                    options.EventsType = typeof(CustomCookieAuthenticationEvents);
                    options.ExpireTimeSpan = TimeSpan.FromDays(1);
                    options.SlidingExpiration = true;
                });

        //Database
        services.AddDbContext<KecskeDatabaseContext>(options => options.UseSqlServer(configuration.GetConnectionString("DatabaseConnection")));

        services.AddScoped<FileStorageService>();
        services.AddScoped<FirewallApiService>();

        services.AddScoped(typeof(GenericRepository<>));
        services.AddScoped(typeof(GenericService<>));
        services.AddScoped<AccountService>();
        services.AddScoped<PermittedIpAddressService>();
    }
}
