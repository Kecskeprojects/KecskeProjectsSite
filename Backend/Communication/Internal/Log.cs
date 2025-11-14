namespace Backend.Communication.Internal;

public class Log(LogLevel level, string log, string? loggingContext)
{
    public DateTime TimeStamp { get; private set; } = DateTime.UtcNow;
    public LogLevel Level { get; private set; } = level;

    private readonly string? loggingContext = loggingContext;

    private string Title => $"[{TimeStamp:HH:mm:ss}][{Level}]:";
    public string Content
    {
        get => Title
            + (Level == LogLevel.Information ? " " : "\t ")
            //+ (string.IsNullOrWhiteSpace(loggingContext) ? "" : $"in '{loggingContext}'\n\t\t\t ")
            + field.Replace("\n", "\n\t\t\t ");
    } = log;
}
