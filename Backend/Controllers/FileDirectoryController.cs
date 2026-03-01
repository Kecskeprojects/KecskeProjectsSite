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
    ) : ApiControllerBase<FileDirectoryService>(logger, fileDirectoryService)
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        DatabaseActionResult<List<FileDirectoryResource>?> fileDataList = await service.GetAccountAccessibleDirectoriesAsync(LoggedInAccount!);

        return fileDataList.Status switch
        {
            DatabaseActionResultEnum.Success when fileDataList.Data is not null => ContentResult(fileDataList.Data),
            _ => ErrorResult(StatusCodes.Status500InternalServerError, "Could not retrieve directories")
        };
    }
}
