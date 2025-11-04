using Backend.Communication.Outgoing;
using Backend.Controllers.Base;
using Backend.CustomAttributes;
using Backend.Services;
using Backend.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class FileController(
    ILogger<AccountController> logger,
    FileStorageService fileStorageService,
    IConfiguration configuration
    ) : ApiControllerBase(logger)
{
    [Authorize]
    [ErrorLoggingFilter]
    [HttpGet]
    public IActionResult GetList([FromQuery] string? folder)
    {
        string? baseFolder = configuration.GetValue<string>("BaseFileRoute");
        string fullPath = FileTools.GetFullPath(baseFolder, folder);

        string[] directories = Directory.GetDirectories(fullPath);
        List<FileData> fileDataList = fileStorageService.GetDirectoryData(baseFolder, directories);

        string[] files = Directory.GetFiles(fullPath);
        fileDataList.AddRange(fileStorageService.GetFileData(baseFolder, files));

        return Ok(fileDataList);
    }

    [Authorize]
    [ErrorLoggingFilter]
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

        return ErrorResult(StatusCodes.Status404NotFound, "File not found");
    }
}
