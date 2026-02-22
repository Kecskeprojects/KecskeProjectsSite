using Backend.Communication.Outgoing;
using Backend.Controllers.Base;
using Backend.CustomAttributes;
using Backend.Services;
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
    ) : ApiControllerBase(logger)
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetFileList([FromQuery] string category, [FromQuery] string? subPath)
    {
        List<FileData> fileDataList = await fileStorageService.GetFilesInDirectory(LoggedInAccount!, category, subPath);

        return ContentResult(fileDataList);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetDirectoryList([FromQuery] string category, [FromQuery] string? subPath)
    {
        List<DirectoryData> fileDataList = await fileStorageService.GetDirectoriesInDirectory(LoggedInAccount!, category, subPath);

        return ContentResult(fileDataList);
    }

    [Authorize]
    [HttpGet("{clientHash}")]
    public async Task<IActionResult> GetSingle([FromRoute] string clientHash, [FromQuery] string category, [FromQuery] string? subPath)
    {
        string? fileRoute = await fileStorageService.GetFileRoute(LoggedInAccount!, category, subPath, clientHash);

        return fileRoute is not null
            ? PhysicalFile(fileRoute, "application/octet-stream", enableRangeProcessing: true)
            : ErrorResult(StatusCodes.Status500InternalServerError, "File not found");
    }

    [Authorize]
    [HttpPost]
    [DisableFormValueModelBinding]
    public async Task<IActionResult> Upload([FromQuery] bool isNewFile, [FromQuery] string category, [FromQuery] string? subPath)
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
        string response = await fileStorageService.SaveViaMultipartReaderAsync(LoggedInAccount!, category, subPath, isNewFile, boundary, Request.Body, cancellationToken);
        return ContentResult(response);
    }
}
