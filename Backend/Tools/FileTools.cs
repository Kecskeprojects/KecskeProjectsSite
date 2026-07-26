using Backend.Communication.Internal;
using DatabaseORM.Communication;
using DatabaseORM.Enums;

namespace Backend.Tools;

public static class FileTools
{
    public static string GetFullPathIfValid(
        DatabaseActionResult<bool> directoryAccessAllowed,
        string? baseDirectory,
        FileStorageTargetPathDetails targetPathDetails)
    {
        if (directoryAccessAllowed.Status != DatabaseActionResultEnum.Success
            || !directoryAccessAllowed.Data)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this directory.");
        }

        if (string.IsNullOrWhiteSpace(baseDirectory) || !Directory.Exists(baseDirectory))
        {
            throw new InvalidOperationException("Base location for files is not configured.");
        }

        string fullTargetDirectoryPath = Path.GetFullPath(Path.Combine(baseDirectory, targetPathDetails.RelativeDirectory));
        string relativePathToCategoryDirectory = GetPathRelativeToCategoryDirectory(baseDirectory, targetPathDetails.RootDirectory, fullTargetDirectoryPath);

        return relativePathToCategoryDirectory.Contains("..") || !Directory.Exists(fullTargetDirectoryPath)
            ? throw new DirectoryNotFoundException("This file or directory does not exist.")
            : fullTargetDirectoryPath;
    }

    public static string GetPathRelativeToCategoryDirectory(string? baseDirectory, string categoryDirectory, string? comparedPath)
    {
        string fullCategoryDirectoryPath = Path.GetFullPath(Path.Combine(baseDirectory ?? "", categoryDirectory));
        string relativePath = Path.GetRelativePath(fullCategoryDirectoryPath, comparedPath ?? comparedPath ?? "");
        return relativePath;
    }
}
