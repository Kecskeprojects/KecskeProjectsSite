
using Backend.Communication.Internal;
using Backend.Tools;

namespace Backend.Logging;

public class FileLogger(string name, Func<FileLoggerConfiguration> getCurrentConfig) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return default!;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        LogLevel minLogLevel = EnumTools.ParseEnum<LogLevel>(getCurrentConfig().MinLogLevel);
        return ((int) minLogLevel) <= ((int) logLevel);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        FileLoggerConfiguration config = getCurrentConfig();
        if (config.EventId == 0 || config.EventId == eventId.Id)
        {
            LogStorage.AddLog(new Log(logLevel, formatter(state, exception), name));
        }
    }
}
