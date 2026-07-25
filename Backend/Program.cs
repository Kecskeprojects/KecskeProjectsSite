using Backend.ApiServices;
using Backend.Authentication;
using Backend.HostedServices;
using Backend.Tools.Extensions;
using DatabaseORM;
using DatabaseORM.Constants;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Runtime.Versioning;

namespace Backend;

[SupportedOSPlatform("windows")]
public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        ConfigureServices(builder.Configuration, builder.Services);
        string? environment = builder.Configuration[ConfigurationKeys.Environment];

        //Removing built-in logging providers and setting custom file logging configuration
        _ = builder.Logging.ClearProviders();
        _ = builder.Logging.AddFileLogger();

        _ = builder.Services.AddControllers();

        _ = builder.Services.AddOpenApi();

        WebApplication app = builder.Build();

        if (environment == EnvironmentConstants.Production)
        {
            _ = app.MapGet("/", () => "The backend is running, now shoo!");
        }
        else
        {
            //Swagger is only available during development
            _ = app.MapOpenApi();
            _ = app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "v1");
            });
        }

        IConfiguration section = builder.Configuration.GetSection(ConfigurationKeys.FrontendDomains) ?? throw new InvalidOperationException("FrontendDomain configuration value is missing");
        string[] frontendDomains = section.Get<string[]>() ?? throw new InvalidOperationException("FrontendDomain configuration value is missing");

        _ = app.UseCors(options => options
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins(frontendDomains)
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
        string? environment = configuration[ConfigurationKeys.Environment];

        //Background service responsible for transferring ILogger entries into permanent file logs
        _ = services.AddHostedService<FileLoggerBackgroundService>();

        //Background service for custom RDP connection solution using temporary firewall rules
        //this service is responsible for removing expired firewall rules from the system
        _ = services.AddHostedService<FirewallRuleExpirationWatcher>();

        //Custom authorization overriding built in microsoft cookie authentication
        _ = services.AddTransient<AuthorizationCookieManager>();

        //Custom handler for endpoints that require authentication
        _ = services.AddSingleton<CustomCookieAuthenticationEvents>();

        //Services
        _ = services.AddScoped<FileStorageService>();
        _ = services.AddScoped<FirewallApiService>();

        //Authentication
        _ = services
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
                    options.Cookie.SameSite =
                        environment == EnvironmentConstants.Production
                            ? SameSiteMode.Strict
                            : SameSiteMode.None; //Chrome checks the Same Site mode of the cookie, otherwise the site cannot be tested in chrome
                    options.EventsType = typeof(CustomCookieAuthenticationEvents);
                    options.ExpireTimeSpan = TimeSpan.FromDays(1);
                    options.SlidingExpiration = true;
                });

        DatabaseStartup.ConfigureDatabaseServices(configuration, services);
    }
}
