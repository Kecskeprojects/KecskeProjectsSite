namespace Backend.Communication.Internal;

public class FileStorageTargetPathDetails
{
    public FileStorageTargetPathDetails(string? targetPath, bool isFile = false)
    {
        targetPath ??= string.Empty;
        string normalizedTargetPath = targetPath
            .Replace("/", Path.DirectorySeparatorChar.ToString()).Replace(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString())
            .Trim(Path.DirectorySeparatorChar)
            .Split("?")[0];

        IEnumerable<string> parts = normalizedTargetPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Count() < 1)
        {
            throw new ArgumentException("The target path must contain at least two parts: the folder and the client file hash.");
        }

        TargetPath = normalizedTargetPath;
        RootDirectory = parts.First();

        if (!isFile)
        {
            RelativeDirectory = string.Join(Path.DirectorySeparatorChar.ToString(), parts);
            return;
        }

        if (parts.Count() < 2)
        {
            throw new ArgumentException("The target path must contain at least two parts: the folder and the client file hash.");
        }

        RelativeDirectory = string.Join(Path.DirectorySeparatorChar.ToString(), parts.Take(parts.Count() - 1));
        FileClientHash = parts.Last();
    }

    public string TargetPath { get; private set; }
    public string RootDirectory { get; private set; }
    public string RelativeDirectory { get; private set; }
    public string? FileClientHash { get; private set; }
}
