using Backend.Communication.Internal;
using Backend.Communication.Outgoing;
using Backend.Constants;
using Backend.Database.Service;
using Backend.Tools;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Backend.Services;

public class FileStorageService(
    ILogger<FileStorageService> logger,
    IConfiguration configuration,
    FileDirectoryService fileDirectoryService)
{
    public async Task<List<FileData>> GetFilesInDirectory(LoggedInAccount loggedInAccount, string categoryDirectory, string? subPath)
    {
        string fullTargetDirectoryPath = await GetFullPath(loggedInAccount, categoryDirectory, subPath);

        string[] fileRoutes = Directory.GetFiles(fullTargetDirectoryPath);

        List<FileData> fileDataList = [];

        string? baseDirectory = configuration.GetValue<string>(ConfigurationConstants.BaseFileDirectoryKey);
        foreach (string fileRoute in fileRoutes)
        {
            FileInfo fileInfo = new(fileRoute);
            if (!fileInfo.Exists
                || fileInfo.Attributes.HasFlag(FileAttributes.System)
                || fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
            {
                continue;
            }

            string relativePath = FileTools.GetPathRelativeToCategoryDirectory(baseDirectory, categoryDirectory, fileInfo.DirectoryName);
            relativePath = relativePath.Replace(".", "");

            fileDataList.Add(new FileData
            {
                Extension = fileInfo.Extension,
                Name = fileInfo.Name,
                SizeInMb = Math.Round(fileInfo.Length / (1024.0m * 1024.0m), 3),
                SizeInGb = Math.Round(fileInfo.Length / (1024.0m * 1024.0m * 1024.0m), 3),
                CreatedAtUtc = fileInfo.CreationTime.ToUniversalTime(),
                Identifier = EncryptionTools.GetMD5HashHexString(fileInfo.FullName),
                SubPath = relativePath
            });
        }

        return fileDataList;
    }

    public async Task<List<DirectoryData>> GetDirectoriesInDirectory(LoggedInAccount loggedInAccount, string categoryDirectory, string? subPath)
    {
        string fullTargetDirectoryPath = await GetFullPath(loggedInAccount, categoryDirectory, subPath);

        string[] directoryRoutes = Directory.GetDirectories(fullTargetDirectoryPath);

        List<DirectoryData> directoryDataList = [];

        string? baseDirectory = configuration.GetValue<string>(ConfigurationConstants.BaseFileDirectoryKey);
        foreach (string directoryRoute in directoryRoutes)
        {
            DirectoryInfo directoryInfo = new(directoryRoute);
            if (!directoryInfo.Exists
                || directoryInfo.Attributes.HasFlag(FileAttributes.System)
                || directoryInfo.Attributes.HasFlag(FileAttributes.Hidden))
            {
                continue;
            }

            string relativePath = FileTools.GetPathRelativeToCategoryDirectory(baseDirectory, categoryDirectory, directoryInfo.FullName);
            relativePath = relativePath.Replace(".", "");

            directoryDataList.Add(new DirectoryData
            {
                Name = directoryInfo.Name,
                CreatedAtUtc = directoryInfo.CreationTime.ToUniversalTime(),
                SubPath = relativePath
            });
        }

        return directoryDataList;
    }

    public async Task<string?> GetFileRoute(LoggedInAccount loggedInAccount, string categoryDirectory, string? subPath, string clientHash)
    {
        string fullTargetDirectoryPath = await GetFullPath(loggedInAccount, categoryDirectory, subPath);

        string[] fileRoutes = Directory.GetFiles(fullTargetDirectoryPath);

        foreach (string fileRoute in fileRoutes)
        {
            string fileHash = EncryptionTools.GetMD5HashHexString(fileRoute);
            if (clientHash.Equals(fileHash, StringComparison.OrdinalIgnoreCase))
            {
                return fileRoute;
            }
        }

        return null;
    }

    public async Task<string> SaveViaMultipartReaderAsync(
        LoggedInAccount loggedInAccount,
        string categoryDirectory,
        string? subPath,
        bool isNewFile,
        string boundary,
        Stream contentStream,
        CancellationToken cancellationToken)
    {
        string fullTargetDirectoryPath = await GetFullPath(loggedInAccount, categoryDirectory, subPath);

        MultipartReader reader = new(boundary, contentStream);
        MultipartSection? section;
        long totalBytesRead = 0;

        // Process each section in the multipart body
        string? fileName = null;
        while ((section = await reader.ReadNextSectionAsync(cancellationToken)) != null)
        {
            // Check if the section is a file
            ContentDispositionHeaderValue? contentDisposition = section.GetContentDispositionHeader();
            if (contentDisposition != null && contentDisposition.IsFileDisposition())
            {
                totalBytesRead = await HandleFileReadAsync(fullTargetDirectoryPath, isNewFile, section, totalBytesRead, fileName, contentDisposition!, cancellationToken);
            }
            else if (contentDisposition != null && contentDisposition.IsFormDisposition())
            {
                // Handle metadata (form fields)
                fileName = await HandleMetadataAsync(section, fileName, contentDisposition!, cancellationToken);
            }
        }

        logger.LogInformation($"File upload completed (via multipart). Total bytes read: {totalBytesRead} bytes.");
        return "Success!";
    }

    private async Task<string> GetFullPath(LoggedInAccount loggedInAccount, string categoryDirectory, string? subPath)
    {
        string? baseDirectory = configuration.GetValue<string>(ConfigurationConstants.BaseFileDirectoryKey);
        DatabaseActionResult<bool> directoryAccessAllowed =
            await fileDirectoryService.AccountHasAccessToDirectoryAsync(loggedInAccount, categoryDirectory);

        string fullPath = FileTools.GetFullPathIfValid(directoryAccessAllowed, baseDirectory, categoryDirectory, subPath);
        return fullPath;
    }

    private async Task<long> HandleFileReadAsync(
        string fullPath,
        bool newFile,
        MultipartSection section,
        long totalBytesRead,
        string? fileName,
        ContentDispositionHeaderValue contentDisposition,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentNullException(fileName, "File name must be earlier in the form data than the file content!");
        }

        if (newFile && File.Exists(Path.Combine(fullPath, fileName)))
        {
            throw new IOException($"File '{fileName}' already exists in this directory.");
        }

        using FileStream outputFileStream = new(
            path: Path.Combine(fullPath, fileName),
            mode: FileMode.Append,
            access: FileAccess.Write,
            share: FileShare.None,
            bufferSize: 16 * 1024 * 1024, // 16 MB buffer size
            useAsync: true);

        logger.LogInformation($"Processing file: {contentDisposition.FileName.Value}");

        // Write the file content to the target file
        await section.Body.CopyToAsync(outputFileStream, cancellationToken);
        totalBytesRead += section.Body.Length;
        return totalBytesRead;
    }

    private async Task<string?> HandleMetadataAsync(
        MultipartSection section,
        string? fileName,
        ContentDispositionHeaderValue contentDisposition,
        CancellationToken cancellationToken)
    {
        string key = contentDisposition.Name.Value!;
        using StreamReader streamReader = new(section.Body);
        string value = await streamReader.ReadToEndAsync(cancellationToken);
        logger.LogInformation($"Received metadata: {key}-{value}");
        switch (key)
        {
            case "fileName":
            {
                fileName = value;
                break;
            }
            default:
                break;
        }

        return fileName;
    }
}
