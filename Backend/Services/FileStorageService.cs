using Backend.Communication.Outgoing;
using Backend.Tools;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Backend.Services;

public class FileStorageService(ILogger<FileStorageService> logger)
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
                Identifier = EncryptionTools.GetMD5HashHexString(directoryInfo.FullName),
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
                Identifier = EncryptionTools.GetMD5HashHexString(fileInfo.FullName),
                Folder = FileTools.GetPathRelativeToBaseFolder(baseFolder, fileInfo.DirectoryName)
            });
        }

        return fileDataList;
    }

    private const int BufferSize = 16 * 1024 * 1024; // 16 MB buffer size
    private const string UploadFilePath = "D:\\test.mp4";

    //Todo: handle file being uploaded to same location with same name
    //Todo: Handle additional file data to differentiate parallel file uploads
    public async Task<string> SaveViaMultipartReaderAsync(string boundary, Stream contentStream, CancellationToken cancellationToken)
    {
        string targetFilePath = Path.Combine(UploadFilePath);
        //CheckAndRemoveLocalFile(targetFilePath);

        using FileStream outputFileStream = new(
            path: targetFilePath,
            mode: FileMode.Append,
            access: FileAccess.Write,
            share: FileShare.None,
            bufferSize: BufferSize,
            useAsync: true);

        MultipartReader reader = new(boundary, contentStream);
        MultipartSection? section;
        long totalBytesRead = 0;

        // Process each section in the multipart body
        while ((section = await reader.ReadNextSectionAsync(cancellationToken)) != null)
        {
            // Check if the section is a file
            ContentDispositionHeaderValue? contentDisposition = section.GetContentDispositionHeader();
            if (contentDisposition != null && contentDisposition.IsFileDisposition())
            {
                logger.LogInformation($"Processing file: {contentDisposition.FileName.Value}");

                // Write the file content to the target file
                await section.Body.CopyToAsync(outputFileStream, cancellationToken);
                totalBytesRead += section.Body.Length;
            }
            else if (contentDisposition != null && contentDisposition.IsFormDisposition())
            {
                // Handle metadata (form fields)
                string key = contentDisposition.Name.Value!;
                using StreamReader streamReader = new(section.Body);
                string value = await streamReader.ReadToEndAsync(cancellationToken);
                logger.LogInformation($"Received metadata: {key} = {value}");
            }
        }

        logger.LogInformation($"File upload completed (via multipart). Total bytes read: {totalBytesRead} bytes.");
        return targetFilePath;
    }

    private void CheckAndRemoveLocalFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            logger.LogDebug($"Removed existing output file: {filePath}");
        }
    }
}
