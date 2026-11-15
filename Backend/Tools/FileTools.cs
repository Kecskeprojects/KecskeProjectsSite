using Backend.Communication.Internal;

namespace Backend.Tools;

public static class FileTools
{
    public static string GetFullPathIfValid(
        bool directoryAccessAllowed,
        string? baseDirectory,
        FileStorageTargetPathDetails targetPathDetails)
    {
        if (!directoryAccessAllowed)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "You do not have permission to access this directory.");
        }

        if (string.IsNullOrWhiteSpace(baseDirectory) || !Directory.Exists(baseDirectory))
        {
            throw new InvalidOperationException("Base location for files is not configured.");
        }

        string fullCategoryDirectoryPath = Path.GetFullPath(Path.Combine(baseDirectory, targetPathDetails.RootDirectory));
        string fullTargetDirectoryPath = Path.GetFullPath(Path.Combine(baseDirectory, targetPathDetails.RelativeDirectory));

        return !IsSameOrInsideDirectory(fullCategoryDirectoryPath, fullTargetDirectoryPath) || !Directory.Exists(fullTargetDirectoryPath)
            ? throw new ApiException(StatusCodes.Status404NotFound, "This file or directory does not exist.")
            : fullTargetDirectoryPath;
    }

    /// <summary>
    /// Returns the full path of a file with the given name directly inside the given directory.
    /// The name comes from the client, so anything that isn't a plain file name
    /// (separators, "..", drive letters, invalid characters) is rejected.
    /// </summary>
    public static string GetFilePathInDirectory(string directoryPath, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)
            || fileName is "." or ".."
            || fileName != Path.GetFileName(fileName)
            || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "The file name is not valid.");
        }

        string fullDirectoryPath = Path.GetFullPath(directoryPath);
        string fullFilePath = Path.GetFullPath(Path.Combine(fullDirectoryPath, fileName));

        string? parentDirectoryPath = Path.GetDirectoryName(fullFilePath);
        return parentDirectoryPath is null || !IsSameDirectory(parentDirectoryPath, fullDirectoryPath)
            ? throw new ApiException(StatusCodes.Status400BadRequest, "The file name is not valid.")
            : fullFilePath;
    }

    /// <summary>
    /// The path the frontend uses for a directory: the category folder followed by the
    /// folders below it, separated by "/" (e.g. "Videos/2024/Summer")
    /// </summary>
    public static string GetClientSubPath(string? baseDirectory, string categoryDirectory, string? directoryPath)
    {
        string relativePath = GetPathRelativeToCategoryDirectory(baseDirectory, categoryDirectory, directoryPath);

        // Path.GetRelativePath returns "." for the category folder itself
        return relativePath == "."
            ? categoryDirectory
            : $"{categoryDirectory}/{relativePath.Replace(Path.DirectorySeparatorChar, '/')}";
    }

    public static string GetPathRelativeToCategoryDirectory(string? baseDirectory, string categoryDirectory, string? comparedPath)
    {
        string fullCategoryDirectoryPath = Path.GetFullPath(Path.Combine(baseDirectory ?? "", categoryDirectory));
        string relativePath = Path.GetRelativePath(fullCategoryDirectoryPath, comparedPath ?? "");
        return relativePath;
    }

    public static bool IsSameOrInsideDirectory(string fullParentPath, string fullPath)
    {
        string relativePath = Path.GetRelativePath(fullParentPath, fullPath);

        // A path on another drive comes back rooted, a path outside the parent starts with ".."
        return !Path.IsPathRooted(relativePath)
            && relativePath != ".."
            && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal);
    }

    private static bool IsSameDirectory(string firstPath, string secondPath)
    {
        return string.Equals(
            Path.TrimEndingDirectorySeparator(firstPath),
            Path.TrimEndingDirectorySeparator(secondPath),
            StringComparison.OrdinalIgnoreCase);
    }
}
