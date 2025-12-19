using Backend.Authentication;
using Backend.BackgroundServices;
using Backend.Database;
using Backend.Database.Repository;
using Backend.Database.Service;
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

        _ = builder.Services.AddHostedService<FileLoggerBackgroundService>();
        _ = builder.Logging.ClearProviders();
        _ = builder.Logging.AddFileLogger();

        // Add services to the container.
        ConfigureServices(builder.Configuration, builder.Services);

        _ = builder.Services.AddControllers();

        _ = builder.Services.AddOpenApi();

        WebApplication app = builder.Build();

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
        //Authorization
        services.AddTransient<AuthorizationCookieManager>();
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

        services.AddScoped(typeof(GenericRepository<>));
        services.AddScoped(typeof(GenericService<>));
        services.AddScoped<AccountService>();
        services.AddScoped<FileStorageService>();
        services.AddScoped<FirewallApiService>();
    }
}
