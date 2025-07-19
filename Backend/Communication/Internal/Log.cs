namespace Backend.Communication.Internal;

public class Log(LogLevel level, string log)
{
    public DateTime TimeStamp { get; private set; } = DateTime.UtcNow;
    public LogLevel Level { get; private set; } = level;

    private readonly string content = log;
    public string Content { get => $"[{TimeStamp:HH:mm:ss}][{Level}]:\t" + content.Replace("\n", "\n\t\t\t"); }
}
