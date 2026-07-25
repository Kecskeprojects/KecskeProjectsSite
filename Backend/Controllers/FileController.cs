using Backend.ApiServices;
using Backend.Communication.Outgoing;
using Backend.Controllers.Base;
using Backend.CustomAttributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Backend.Controllers;

[ApiController]
[ErrorLoggingFilter]
[Route("api/[controller]/[action]")]
public class FileController(
    ILogger<AccountController> logger,
    FileStorageService fileStorageService
    ) : ApiControllerBase<FileStorageService>(logger, fileStorageService)
{
        [Authorize]
    [HttpGet("{*targetPath}")]
    public async Task<IActionResult> GetFileList(string? targetPath)
    {
        List<FileData> fileDataList = await service.GetFilesInDirectory(LoggedInAccount!, targetPath);

        return ContentResult(fileDataList);
    }

    [Authorize]
    [HttpGet("{*targetPath}")]
    public async Task<IActionResult> GetDirectoryList(string? targetPath)
    {
        List<DirectoryData> fileDataList = await service.GetDirectoriesInDirectory(LoggedInAccount!, targetPath);

        return ContentResult(fileDataList);
    }

    [Authorize]
    [HttpGet("{clientHash}/{*targetPath}")]
    public async Task<IActionResult> GetSingle(string clientHash, string? targetPath)
    {
        string? fileRoute = await service.GetFileRoute(LoggedInAccount!, targetPath, clientHash);

        return fileRoute is not null
            ? PhysicalFile(fileRoute, "application/octet-stream", enableRangeProcessing: true)
            : ErrorResult(StatusCodes.Status500InternalServerError, "File not found");
    }

    [Authorize]
    [HttpPost("{*targetPath}")]
    [DisableFormValueModelBinding]
    public async Task<IActionResult> Upload([FromQuery] bool isNewFile, [FromRoute] string? targetPath)
    {
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
        string response = await service.SaveViaMultipartReaderAsync(LoggedInAccount!, targetPath, isNewFile, boundary, Request.Body, cancellationToken);
        return ContentResult(response);
    }
}
