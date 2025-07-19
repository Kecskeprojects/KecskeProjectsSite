namespace Backend.Logging;

public class FileLoggerConfiguration
{
    public int EventId { get; set; }

    public string MinLogLevel { get; set; } = "Warning";
}
