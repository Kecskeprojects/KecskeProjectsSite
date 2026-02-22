namespace Backend.Communication.Outgoing;

public class DirectoryData
{
    public string Name { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public string? SubPath { get; set; }
}
