using Backend.Authentication;
using Backend.Communication.Incoming;
using Backend.Communication.Internal;
using Backend.Communication.Outgoing;
using Backend.Controllers.Base;
using Backend.CustomAttributes;
using Backend.Database.Service;
using Backend.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
[Route("api/[controller]/[action]/{id}")]
public class FileController(
    ILogger<AccountController> logger
    ) : ApiControllerBase(logger)
{
    [Authorize]
    [ErrorLoggingFilter]
    [HttpGet]
    public IActionResult GetList()
    {
        string[] files = Directory.GetFiles("C:\\Users\\Kirsch_Adam_Peter\\Downloads");

        IEnumerable<FileInfo> fileInfos = files.Select(filePath => new FileInfo(filePath));
        IEnumerable<FileData> fileDataList = fileInfos.Select(fileInfo => new FileData
        {
            Extension = fileInfo.Extension,
            Name = fileInfo.Name,
            SizeInMb = (fileInfo.Length / (1024 * 1024)).ToString(),
            SizeInGb = (fileInfo.Length / (1024 * 1024 * 1024)).ToString(),
            CreatedAt = fileInfo.CreationTime,
            RelativeRoute = fileInfo.FullName.Replace("C:\\Users\\Kirsch_Adam_Peter\\Downloads\\", "")
        });

        return Ok(fileDataList);
    }

    [Authorize]
    [ErrorLoggingFilter]
    [HttpGet]
    public IActionResult GetSingle(string id)
    {
        return PhysicalFile($"C:\\Users\\Kirsch_Adam_Peter\\Downloads\\{id}", "application/octet-stream", enableRangeProcessing: true);
    }
}
