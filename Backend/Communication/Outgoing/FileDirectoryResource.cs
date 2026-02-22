namespace Backend.Communication.Outgoing;

public class FileDirectoryResource
{
    public int FileDirectoryId { get; set; }

    public string RelativePath { get; set; } = null!;

    public string DisplayName { get; set; } = null!;
}
