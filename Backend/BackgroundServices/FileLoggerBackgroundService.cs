using Backend.Communication.Internal;
using Backend.Logging;

namespace Backend.BackgroundServices;

public class FileLoggerBackgroundService(IConfiguration configuration) : BackgroundService
{
    private readonly IConfiguration configuration = configuration;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(5000, stoppingToken); // Initial delay to force ExecuteAsync to be async

        string logFolderPath = configuration.GetValue<string>("LogPath") ?? "log";

        if (!Path.IsPathFullyQualified(logFolderPath))
        {
            logFolderPath = Path.Combine(AppContext.BaseDirectory, logFolderPath);
        }

        if (!Directory.Exists(logFolderPath))
        {
            Directory.CreateDirectory(logFolderPath);
        }

        IEnumerable<Log> logs = FileLoggerLogStorage.GetConsumingEnumerable(stoppingToken);
        foreach (Log msg in logs)
        {
            int maxTries = 3;
            for (int i = 0; i < maxTries; i++)
            {
                try
                {
                    File.AppendAllText(Path.Combine(logFolderPath, $"Backend-Log-{DateTime.UtcNow:yyyy-MM-dd}.txt"), msg.Content + "\n");
                    break; // Exit the retry loop if successful
                }
                catch (Exception ex)
                {
                    FileLoggerLogStorage.AddLog(new Log(LogLevel.Critical, ex.ToString(), this.ToString()));
                    await Task.Delay(100, stoppingToken); // Wait before retrying
                }
            }
        }
    }
}
