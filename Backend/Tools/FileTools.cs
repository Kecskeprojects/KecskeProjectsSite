namespace Backend.Tools;

public static class FileTools
{
    public static string GetFullPath(string? baseFolder, string? relativeRoute)
    {
        if (string.IsNullOrWhiteSpace(baseFolder) || !Directory.Exists(baseFolder))
        {
            throw new InvalidOperationException("Base location for files is not configured.");
        }

        string fullPath = Path.GetFullPath(Path.Combine(baseFolder, relativeRoute ?? ""));
        string relativeToBaseFolder = GetPathRelativeToBaseFolder(baseFolder, fullPath);

        return relativeToBaseFolder.Contains("..") || !Directory.Exists(fullPath)
            ? throw new DirectoryNotFoundException("This file or directory does not exist.")
            : fullPath;
    }

    public static string GetPathRelativeToBaseFolder(string? baseFolder, string? comparedPath)
    {
        return Path.GetRelativePath(baseFolder ?? "", comparedPath ?? comparedPath ?? "");
    }
}
