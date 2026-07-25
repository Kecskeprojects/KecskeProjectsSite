using Backend.ApiServices;
using DatabaseORM.Constants;
using System.Runtime.Versioning;

namespace Backend.HostedServices;

[SupportedOSPlatform("windows")]
public class FirewallRuleExpirationWatcher(
    IServiceProvider serviceProvider,
    ILogger<FirewallRuleExpirationWatcher> logger,
    IConfiguration configuration
    ) : IHostedService, IDisposable
{
    private bool running = false;
    private bool disposed = false;
    private Timer? Timer { get; set; }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Timed Hosted Service is starting.");

        string? environment = configuration[ConfigurationKeys.Environment];
        int repeatIntervalMinutes = ConfigurationKeys.Environment == EnvironmentConstants.Production ? 5 : 1;
        TimeSpan interval = TimeSpan.FromMinutes(repeatIntervalMinutes);

        Timer = new Timer(DoWork, null, TimeSpan.Zero, interval);

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {

        if (running)
        {
            logger.LogWarning("Previous execution is still running. Skipping this run.");
            return;
        }

        running = true;
        logger.LogInformation($"Timed Hosted Service is working.");

        try
        {
            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                FirewallApiService firewallService = scope.ServiceProvider.GetRequiredService<FirewallApiService>();
                bool _ = firewallService.RemoveExpiredRDPRules().GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while checking for expired firewall rules.");
        }
        finally
        {
            running = false;
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Timed Hosted Service is stopping.");

        _ = (Timer?.Change(Timeout.Infinite, 0));

        return Task.CompletedTask;
    }

    public void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                Timer?.Dispose();
            }
            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
