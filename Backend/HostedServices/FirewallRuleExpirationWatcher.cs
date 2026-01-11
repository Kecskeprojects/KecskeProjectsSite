using Backend.Services;
using System.Runtime.Versioning;

namespace Backend.HostedServices;

[SupportedOSPlatform("windows")]
public class FirewallRuleExpirationWatcher(
    IServiceProvider serviceProvider,
    ILogger<FirewallRuleExpirationWatcher> logger
    ) : IHostedService, IDisposable
{
    private int executionCount = 0;
    private bool running = false;
    private bool disposed = false;
    private Timer? Timer { get; set; }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Timed Hosted Service running.");

        Timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        int count = Interlocked.Increment(ref executionCount);

        if (running)
        {
            logger.LogWarning("Previous execution is still running. Skipping this run.");
            return;
        }

        running = true;
        logger.LogInformation($"Timed Hosted Service is working. Count: {count}");

        try
        {
            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                FirewallApiService firewallService = scope.ServiceProvider.GetRequiredService<FirewallApiService>();
                firewallService.RemoveExpiredRDPRules().GetAwaiter().GetResult();
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
#if DEBUG
        GC.SuppressFinalize(this);
#endif
    }
}
