using Backend.Authentication;
using Backend.Database;
using Backend.Database.Repository;
using Backend.Database.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Backend;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        // Add services to the container.
        ConfigureServices(builder.Configuration, builder.Services);

        builder.Services.AddControllers();

        builder.Services.AddOpenApi();

        WebApplication app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "v1");
            });
        }
        else
        {
            app.MapGet("/", () => "The backend is running, now shoo!");
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseCors(options => options.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

        app.MapControllers();

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
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.EventsType = typeof(CustomCookieAuthenticationEvents);
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                    options.SlidingExpiration = true;
                });

        //Database
        services.AddDbContext<KecskeDatabaseContext>(options => options.UseSqlServer(configuration.GetConnectionString("DatabaseConnection")));

        services.AddScoped(typeof(GenericRepository<>));
        services.AddScoped(typeof(GenericService<>));
        services.AddScoped<AccountService>();
    }
}
