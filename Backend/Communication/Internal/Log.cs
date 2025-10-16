namespace Backend.Communication.Internal;

public class Log(LogLevel level, string log, string? loggingContext)
{
    public DateTime TimeStamp { get; private set; } = DateTime.UtcNow;
    public LogLevel Level { get; private set; } = level;

    private readonly string? loggingContext = loggingContext;

    private readonly string content = log;
    private string Title => $"[{TimeStamp:HH:mm:ss}][{Level}]:";
    public string Content =>
        Title
        + (Level == LogLevel.Information ? " " : "\t ")
        //+ (string.IsNullOrWhiteSpace(loggingContext) ? "" : $"in '{loggingContext}'\n\t\t\t ")
        + content.Replace("\n", "\n\t\t\t ");
}
