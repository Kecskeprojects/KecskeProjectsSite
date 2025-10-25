using Backend.Communication.Outgoing;
using System.Security.Cryptography;
using System.Text;

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

    public static string GetHashedHexString(string fullPath)
    {
        byte[] stringBytes = Encoding.UTF8.GetBytes(fullPath.ToLower());
        byte[] hash = MD5.HashData(stringBytes);
        return Convert.ToHexString(hash).ToLower();
    }

    public static List<FileData> GetDirectoryData(string? baseFolder, string[] directoryRoutes)
    {
        List<FileData> fileDataList = [];

        foreach (string directoryRoute in directoryRoutes)
        {
            DirectoryInfo directoryInfo = new(directoryRoute);
            if (!directoryInfo.Exists || directoryInfo.Attributes.HasFlag(FileAttributes.System) || directoryInfo.Attributes.HasFlag(FileAttributes.Hidden))
            {
                continue;
            }

            fileDataList.Add(new FileData
            {
                Extension = null,
                Name = directoryInfo.Name,
                SizeInMb = 0,
                SizeInGb = 0,
                IsFolder = true,
                CreatedAtUtc = directoryInfo.CreationTime.ToUniversalTime(),
                Identifier = GetHashedHexString(directoryInfo.FullName),
                Folder = GetPathRelativeToBaseFolder(baseFolder, directoryInfo.Parent?.FullName)
            });
        }

        return fileDataList;
    }

    public static List<FileData> GetFileData(string? baseFolder, string[] fileRoutes)
    {
        List<FileData> fileDataList = [];

        foreach (string fileRoute in fileRoutes)
        {
            FileInfo fileInfo = new(fileRoute);

            if (!fileInfo.Exists || fileInfo.Attributes.HasFlag(FileAttributes.System) || fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
            {
                continue;
            }

            fileDataList.Add(new FileData
            {
                Extension = fileInfo.Extension,
                Name = fileInfo.Name,
                SizeInMb = Math.Round(fileInfo.Length / (1024.0m * 1024.0m), 3),
                SizeInGb = Math.Round(fileInfo.Length / (1024.0m * 1024.0m * 1024.0m), 3),
                IsFolder = false,
                CreatedAtUtc = fileInfo.CreationTime.ToUniversalTime(),
                Identifier = GetHashedHexString(fileInfo.FullName),
                Folder = GetPathRelativeToBaseFolder(baseFolder, fileInfo.DirectoryName)
            });
        }

        return fileDataList;
    }
}
