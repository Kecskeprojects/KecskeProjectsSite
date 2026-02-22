using Backend.Communication.Outgoing;
using Backend.Controllers.Base;
using Backend.CustomAttributes;
using Backend.Services;
using Backend.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Backend.Controllers;

[ApiController]
[ErrorLoggingFilter]
[Route("api/[controller]/[action]")]
public class FileController(
    ILogger<AccountController> logger,
    FileStorageService fileStorageService,
    IConfiguration configuration
    ) : ApiControllerBase(logger)
{
    //Todo:Rewrite logic to consider the FileFolder and FileFolderRole tables, these folders are only Top level folders, categories if you will
    //Perhaps top level folders should be part of the parameters of the endpoints, followed by a parameter for the relative path within that folder, if the file isn't top level within it

    [Authorize]
    [HttpGet]
    public IActionResult GetList([FromQuery] string? folder)
    {
        string? baseFolder = configuration.GetValue<string>("BaseFileRoute");
        string fullPath = FileTools.GetFullPath(baseFolder, folder);

        string[] directories = Directory.GetDirectories(fullPath);
        List<FileData> fileDataList = fileStorageService.GetDirectoryData(baseFolder, directories);

        string[] files = Directory.GetFiles(fullPath);
        fileDataList.AddRange(fileStorageService.GetFileData(baseFolder, files));

        return ContentResult(fileDataList);
    }

    [Authorize]
    [HttpGet("{clientHash}")]
    public IActionResult GetSingle(string clientHash, [FromQuery] string? folder)
    {
        string? baseFolder = configuration.GetValue<string>("BaseFileRoute");
        string fullPath = FileTools.GetFullPath(baseFolder, folder);

        string[] files = Directory.GetFiles(fullPath);

        foreach (string file in files)
        {
            string fileHash = EncryptionTools.GetMD5HashHexString(file);
            if (clientHash.Equals(fileHash, StringComparison.OrdinalIgnoreCase))
            {
                return PhysicalFile(file, "application/octet-stream", enableRangeProcessing: true);
            }
        }

        return ErrorResult(StatusCodes.Status500InternalServerError, "File not found");
    }

    [Authorize]
    [HttpPost]
    [DisableFormValueModelBinding]
    public async Task<IActionResult> Upload([FromQuery] string? folder, [FromQuery] bool newFile)
    {
        string? baseFolder = configuration.GetValue<string>("BaseFileRoute");
        string fullPath = FileTools.GetFullPath(baseFolder, folder);

        if (!Request.ContentType?.StartsWith("multipart/form-data") ?? true)
        {
            return ErrorResult(StatusCodes.Status400BadRequest, "The request does not contain valid multipart form data.");
        }

        string? boundary = HeaderUtilities.RemoveQuotes(MediaTypeHeaderValue.Parse(Request.ContentType).Boundary).Value;
        if (string.IsNullOrWhiteSpace(boundary))
        {
            return ErrorResult(StatusCodes.Status400BadRequest, "Missing boundary in multipart form data.");
        }

        CancellationToken cancellationToken = HttpContext.RequestAborted;
        string response = await fileStorageService.SaveViaMultipartReaderAsync(fullPath, newFile, boundary, Request.Body, cancellationToken);
        return ContentResult(response);
    }
}
