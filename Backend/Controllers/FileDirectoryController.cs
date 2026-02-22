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
[ErrorLoggingFilter]
[Route("api/[controller]/[action]")]
public class FileDirectoryController(
    ILogger<AccountController> logger,
    FileDirectoryService fileDirectoryService
    ) : ApiControllerBase(logger)
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetCategories([FromQuery] string category, [FromQuery] string? subPath)
    {
        DatabaseActionResult<List<FileDirectoryResource>?> fileDataList = await fileDirectoryService.GetAccountAccessibleDirectoriesAsync(LoggedInAccount!);

        return fileDataList.Status switch
        {
            DatabaseActionResultEnum.Success when fileDataList.Data is not null => ContentResult(fileDataList.Data),
            _ => ErrorResult(StatusCodes.Status500InternalServerError, "Could not retrieve directories")
        };
    }
}
