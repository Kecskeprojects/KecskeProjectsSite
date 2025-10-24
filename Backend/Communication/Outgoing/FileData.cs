namespace Backend.Communication.Outgoing;

public class FileData
{
    public string Extension { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string SizeInMb { get; set; } = null!;
    public string SizeInGb { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string RelativeRoute { get; set; } = null!;
    public bool IsFolder { get; set; }
}
