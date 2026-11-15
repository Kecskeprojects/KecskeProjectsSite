using Backend.Communication.Internal;
using Backend.Communication.Outgoing;
using Backend.Tools;
using DatabaseORM.Communication.Resource;
using DatabaseORM.Constants;
using DatabaseORM.Service;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Backend.ApiServices;

public class FileStorageService(
    ILogger<FileStorageService> logger,
    IConfiguration configuration,
    FileDirectoryService fileDirectoryService)
{
    //Todo: Cleanup for better readability and maintainability (e.g. split into multiple methods, add more logging, add more error handling, etc.)
    public async Task<List<FileData>> GetFilesInDirectory(LoggedInAccount loggedInAccount, FileStorageTargetPathDetails targetPathDetails)
    {
        string fullTargetDirectoryPath = await GetFullPath(loggedInAccount, targetPathDetails);

        string[] fileRoutes = Directory.GetFiles(fullTargetDirectoryPath);

        List<FileData> fileDataList = [];

        string? baseDirectory = configuration.GetValue<string>(ConfigurationKeys.BaseFileDirectoryKey);
        foreach (string fileRoute in fileRoutes)
        {
            FileInfo fileInfo = new(fileRoute);
            if (!fileInfo.Exists
                || fileInfo.Attributes.HasFlag(FileAttributes.System)
                || fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
            {
                continue;
            }

            string fullSubPath = FileTools.GetClientSubPath(baseDirectory, targetPathDetails.RootDirectory, fileInfo.DirectoryName);

            fileDataList.Add(new FileData
            {
                Extension = fileInfo.Extension,
                Name = fileInfo.Name,
                SizeInMb = Math.Round(fileInfo.Length / (1024.0m * 1024.0m), 3),
                SizeInGb = Math.Round(fileInfo.Length / (1024.0m * 1024.0m * 1024.0m), 3),
                CreatedAtUtc = fileInfo.CreationTime.ToUniversalTime(),
                Identifier = EncryptionTools.GetMD5HashHexString(fileInfo.FullName),
                SubPath = fullSubPath
            });
        }

        return fileDataList;
    }

    public async Task<List<DirectoryData>> GetDirectoriesInDirectory(LoggedInAccount loggedInAccount, FileStorageTargetPathDetails targetPathDetails)
    {
        string fullTargetDirectoryPath = await GetFullPath(loggedInAccount, targetPathDetails);

        string[] directoryRoutes = Directory.GetDirectories(fullTargetDirectoryPath);

        List<DirectoryData> directoryDataList = [];

        string? baseDirectory = configuration.GetValue<string>(ConfigurationKeys.BaseFileDirectoryKey);
        foreach (string directoryRoute in directoryRoutes)
        {
            DirectoryInfo directoryInfo = new(directoryRoute);
            if (!directoryInfo.Exists
                || directoryInfo.Attributes.HasFlag(FileAttributes.System)
                || directoryInfo.Attributes.HasFlag(FileAttributes.Hidden))
            {
                continue;
            }

            string fullSubPath = FileTools.GetClientSubPath(baseDirectory, targetPathDetails.RootDirectory, directoryInfo.FullName);

            directoryDataList.Add(new DirectoryData
            {
                Name = directoryInfo.Name,
                CreatedAtUtc = directoryInfo.CreationTime.ToUniversalTime(),
                SubPath = fullSubPath
            });
        }

        return directoryDataList;
    }

    public async Task<string?> GetFileRoute(LoggedInAccount loggedInAccount, FileStorageTargetPathDetails targetPathDetails)
    {
        string fullTargetDirectoryPath = await GetFullPath(loggedInAccount, targetPathDetails);

        string[] fileRoutes = Directory.GetFiles(fullTargetDirectoryPath);

        foreach (string fileRoute in fileRoutes)
        {
            // Files hidden from the listing must not be downloadable either
            FileAttributes attributes = File.GetAttributes(fileRoute);
            if (attributes.HasFlag(FileAttributes.System) || attributes.HasFlag(FileAttributes.Hidden))
            {
                continue;
            }

            string fileHash = EncryptionTools.GetMD5HashHexString(fileRoute);
            if (targetPathDetails.FileClientHash?.Equals(fileHash, StringComparison.OrdinalIgnoreCase) == true)
            {
                return fileRoute;
            }
        }

        return null;
    }

    public async Task<string> SaveViaMultipartReaderAsync(
        LoggedInAccount loggedInAccount,
        FileStorageTargetPathDetails targetPathDetails,
        bool isNewFile,
        string boundary,
        Stream contentStream,
        CancellationToken cancellationToken)
    {
        string fullTargetDirectoryPath = await GetFullPath(loggedInAccount, targetPathDetails);

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

    private async Task<string> GetFullPath(LoggedInAccount loggedInAccount, FileStorageTargetPathDetails targetPathDetails)
    {
        string? baseDirectory = configuration.GetValue<string>(ConfigurationKeys.BaseFileDirectoryKey);
        bool directoryAccessAllowed =
            await fileDirectoryService.AccountHasAccessToDirectoryAsync(loggedInAccount, targetPathDetails.RootDirectory);

        string fullPath = FileTools.GetFullPathIfValid(directoryAccessAllowed, baseDirectory, targetPathDetails);
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
            throw new ApiException(StatusCodes.Status400BadRequest, "File name must be earlier in the form data than the file content!");
        }

        // The file name comes from the client, so it must not be able to point outside the target directory
        string filePath = FileTools.GetFilePathInDirectory(fullPath, fileName);

        if (newFile && File.Exists(filePath))
        {
            throw new ApiException(StatusCodes.Status409Conflict, $"File '{fileName}' already exists in this directory.");
        }

        using FileStream outputFileStream = new(
            path: filePath,
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
