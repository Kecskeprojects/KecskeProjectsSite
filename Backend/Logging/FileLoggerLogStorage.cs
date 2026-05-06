using Backend.Communication.Internal;
using System.Collections.Concurrent;

namespace Backend.Logging;

// Static class that serves as a thread-safe storage for log messages produced by the FileLogger instances, allowing the FileLoggerBackgroundService to consume and write them to files asynchronously
public static class FileLoggerLogStorage
{
    private static readonly BlockingCollection<Log> LogMessages = [];

    public static void AddLog(Log log)
    {
        if (!LogMessages.TryAdd(log))
        {
            throw new InvalidOperationException("Failed to add log to the collection. The collection may be completed or disposed.");
        }
    }

    public static IEnumerable<Log> GetConsumingEnumerable(CancellationToken token)
    {
        return LogMessages.GetConsumingEnumerable(token);
    }

    public static void Dispose()
    {
        LogMessages.CompleteAdding();
        LogMessages.Dispose();
    }

    public static bool IsAddingCompleted => LogMessages.IsAddingCompleted;
    public static bool IsCompleted => LogMessages.IsCompleted;
}
