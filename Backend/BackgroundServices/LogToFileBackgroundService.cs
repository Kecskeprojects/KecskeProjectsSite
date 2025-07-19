using Backend.Communication.Internal;
using Backend.Logging;

namespace Backend.BackgroundServices;

public class LogToFileBackgroundService(IConfiguration configuration) : BackgroundService
{
    private readonly IConfiguration configuration = configuration;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string logFolderPath = configuration.GetValue<string>("LogPath") ?? "log";

        if (!Path.IsPathFullyQualified(logFolderPath))
        {
            logFolderPath = Path.Combine(AppContext.BaseDirectory, logFolderPath);
        }

        if (!Directory.Exists(logFolderPath))
        {
            Directory.CreateDirectory(logFolderPath);
        }

        IEnumerable<Log> logs = LogStorage.GetConsumingEnumerable(stoppingToken);
        foreach (Log msg in logs)
        {
            int maxTries = 3;
            for (int i = 0; i < maxTries; i++)
            {
                try
                {
                    File.AppendAllText(Path.Combine(logFolderPath, $"Backend_Log[{DateTime.UtcNow:yyyy-MM-dd}].txt"), "\n" + msg.Content);
                    break; // Exit the retry loop if successful
                }
                catch (Exception ex)
                {
                    LogStorage.AddLog(new Log(LogLevel.Critical, ex.ToString()));
                    await Task.Delay(100, stoppingToken); // Wait before retrying
                }
            }
        }
    }
}
