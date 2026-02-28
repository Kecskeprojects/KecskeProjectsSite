using Backend.Communication.Internal;
using Backend.Constants;
using Backend.Logging;

namespace Backend.BackgroundServices;

public class FileLoggerBackgroundService(IConfiguration configuration) : BackgroundService
{
    private readonly IConfiguration configuration = configuration;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(5000, stoppingToken); // Initial delay to force ExecuteAsync to be async

        string logDirectoryPath = configuration.GetValue<string>(ConfigurationConstants.LogPathKey) ?? "log";

        if (!Path.IsPathFullyQualified(logDirectoryPath))
        {
            logDirectoryPath = Path.Combine(AppContext.BaseDirectory, logDirectoryPath);
        }

        if (!Directory.Exists(logDirectoryPath))
        {
            _ = Directory.CreateDirectory(logDirectoryPath);
        }

        IEnumerable<Log> logs = FileLoggerLogStorage.GetConsumingEnumerable(stoppingToken);
        foreach (Log msg in logs)
        {
            int maxTries = 3;
            for (int i = 0; i < maxTries; i++)
            {
                try
                {
                    File.AppendAllText(Path.Combine(logDirectoryPath, $"Backend-Log-{DateTime.UtcNow:yyyy-MM-dd}.txt"), msg.Content + "\n");
                    break; // Exit the retry loop if successful
                }
                catch (Exception ex)
                {
                    FileLoggerLogStorage.AddLog(new Log(LogLevel.Critical, "Internal FileLogger Exception!", ex, ToString()));
                    await Task.Delay(100, stoppingToken); // Wait before retrying
                }
            }
        }
    }
}
