using Backend.Controllers.Base;
using Backend.CustomAttributes;
using DatabaseORM.Communication.Resource;
using DatabaseORM.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[ErrorLoggingFilter]
[Route("api/[controller]/[action]")]
public class FileDirectoryController(
    ILogger<FileDirectoryController> logger,
    FileDirectoryService fileDirectoryService
    ) : ApiControllerBase<FileDirectoryService>(logger, fileDirectoryService)
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        List<FileDirectoryResource> directories = await service.GetAccountAccessibleDirectoriesAsync(LoggedInAccount!);
        return ContentResult(directories);
    }
}
