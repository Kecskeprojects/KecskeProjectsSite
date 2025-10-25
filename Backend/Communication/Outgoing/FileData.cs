namespace Backend.Communication.Outgoing;

public class FileData
{
    public string? Extension { get; set; }
    public string Name { get; set; } = null!;
    public decimal SizeInMb { get; set; }
    public decimal SizeInGb { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string Identifier { get; set; } = null!;
    public bool IsFolder { get; set; }
    public string? Folder { get; set; }
}
