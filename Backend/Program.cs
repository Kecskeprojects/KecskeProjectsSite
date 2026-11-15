using Backend.ApiServices;
using Backend.Authentication;
using Backend.Communication.Outgoing;
using Backend.Constants;
using Backend.HostedServices;
using Backend.Middleware;
using Backend.Tools.Extensions;
using DatabaseORM;
using DatabaseORM.Constants;
using DatabaseORM.Tools.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using System.Runtime.Versioning;
using System.Threading.RateLimiting;

namespace Backend;

[SupportedOSPlatform("windows")]
public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        ConfigureServices(builder.Configuration, builder.Services);

        //Removing built-in logging providers and setting custom file logging configuration
        _ = builder.Logging.ClearProviders();
        _ = builder.Logging.AddFileLogger();

        _ = builder.Services.AddControllers();

        _ = builder.Services.AddOpenApi();

        WebApplication app = builder.Build();

        if (builder.Configuration.IsProduction())
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

        // Security headers - runs before other middleware so every response gets them
        _ = app.UseMiddleware<SecurityHeadersMiddleware>();

        _ = app.UseCors(policy => ConfigureCors(policy, frontendDomains));

        // Must come after UseCors, otherwise rejected (429) responses lack CORS headers and the frontend can't read them
        _ = app.UseRateLimiter();

        _ = app.UseHttpsRedirection();

        _ = app.UseAuthentication();
        _ = app.UseAuthorization();

        _ = app.MapControllers();

        app.Logger.LogInformation("Starting Kecske Backend");
        app.Run();
    }

    private static void ConfigureServices(ConfigurationManager configuration, IServiceCollection services)
    {
        bool isProduction = configuration.IsProduction();

        //Background service responsible for transferring ILogger entries into permanent file logs
        _ = services.AddHostedService<FileLoggerBackgroundService>();

        //Background service for custom RDP connection solution using temporary firewall rules
        //this service is responsible for removing expired firewall rules from the system
        _ = services.AddHostedService<FirewallRuleExpirationWatcher>();

        //Custom authorization overriding built in microsoft cookie authentication
        _ = services.AddTransient<AuthorizationCookieManager>();

        //Custom handler for endpoints that require authentication
        _ = services.AddSingleton<CustomCookieAuthenticationEvents>();

        //Rate limiting, applied with [EnableRateLimiting(RateLimitPolicies.X)] on controllers or actions
        _ = services.AddRateLimiter(ConfigureRateLimiter);

        //Services
        _ = services.AddScoped<FileStorageService>();
        _ = services.AddScoped<FirewallApiService>();

        //Authentication
        _ = services
            .AddAuthentication(options => options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => ConfigureCookieAuthentication(options, isProduction));

        DatabaseStartup.ConfigureDatabaseServices(configuration, services);
    }

    #region Configuration Methods
    private static void ConfigureCors(CorsPolicyBuilder policy, string[] frontendDomains)
    {
        _ = policy
            .WithOrigins(frontendDomains)
            .AllowCredentials()
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Content-Type", "Authorization")
            .WithExposedHeaders("Content-Disposition")
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    }

    private static void ConfigureRateLimiter(RateLimiterOptions options)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = OnRateLimitRejected;

        _ = options.AddPolicy(RateLimitPolicies.Authentication, httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = RateLimitPolicies.AuthenticationPermitLimit,
                    Window = RateLimitPolicies.AuthenticationWindow,
                    QueueLimit = 0
                }));
    }

    //Returns the rejection in the usual GenericResponse format, so the frontend can display it
    private static async ValueTask OnRateLimitRejected(OnRejectedContext context, CancellationToken cancellationToken)
    {
        string message = "Too many attempts. Please try again later.";
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
        {
            int seconds = (int) Math.Ceiling(retryAfter.TotalSeconds);
            context.HttpContext.Response.Headers.RetryAfter = seconds.ToString();
            message = $"Too many attempts. Please try again in {seconds} seconds.";
        }

        await context.HttpContext.Response.WriteAsJsonAsync(new GenericResponse(message, isError: true), cancellationToken);
    }

    private static void ConfigureCookieAuthentication(CookieAuthenticationOptions options, bool isProduction)
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = isProduction
            ? SameSiteMode.Strict
            : SameSiteMode.None; //Chrome checks the Same Site mode of the cookie, otherwise the site cannot be tested in chrome
        options.EventsType = typeof(CustomCookieAuthenticationEvents);
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.SlidingExpiration = true;
    }
    #endregion
}
