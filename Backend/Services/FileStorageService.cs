using Backend.Communication.Outgoing;
using Backend.Tools;

namespace Backend.Services;

public class FileStorageService
{
    public List<FileData> GetDirectoryData(string? baseFolder, string[] directoryRoutes)
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
                Identifier = HashTools.GetMD5HashHexString(directoryInfo.FullName),
                Folder = FileTools.GetPathRelativeToBaseFolder(baseFolder, directoryInfo.Parent?.FullName)
            });
        }

        return fileDataList;
    }

    public List<FileData> GetFileData(string? baseFolder, string[] fileRoutes)
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
                Identifier = HashTools.GetMD5HashHexString(fileInfo.FullName),
                Folder = FileTools.GetPathRelativeToBaseFolder(baseFolder, fileInfo.DirectoryName)
            });
        }

        return fileDataList;
    }
}
