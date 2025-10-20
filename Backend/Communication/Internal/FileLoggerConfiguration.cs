namespace Backend.Communication.Internal;

public class FileLoggerConfiguration
{
    public int EventId { get; set; }

    public string MinLogLevel { get; set; } = "Warning";
}
