namespace Backend.Communication.Internal;

public class Log(LogLevel level, string log)
{
    public DateTime TimeStamp { get; private set; } = DateTime.UtcNow;
    public LogLevel Level { get; private set; } = level;

    private readonly string content = log;
    public string Title => $"[{TimeStamp:HH:mm:ss}][{Level}]:";
    public string Content =>
        Title
        + (Level == LogLevel.Information? " " : "\t ")
        + content.Replace("\n", "\n\t\t\t ");
}
