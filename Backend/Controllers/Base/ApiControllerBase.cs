using Backend.Authentication;
using Backend.Communication.Outgoing;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.Base;

public class ApiControllerBase(
    AuthorizationCookieManager userManager,
    ILogger<ApiControllerBase> logger
    ) : ControllerBase
{
    protected readonly AuthorizationCookieManager userManager = userManager;
    private readonly ILogger<ApiControllerBase> logger = logger;

    [NonAction]
    protected LoggedInAccount? GetLoggedInUserFromCookie()
    {
        return userManager.GetLoggedInUser(HttpContext.User);
    }

    [NonAction]
    protected IActionResult MessageResult(string message)
    {
        return Ok(new MessageActionResult(message));
    }

    [NonAction]
    protected IActionResult ErrorResult(int statusCode, string error)
    {
        logger.LogError(
            "An error occurred: {error}\nWith status code: {statusCode}\nWith request path: {Path}",
            error,
            statusCode,
            HttpContext.Request.Path);
        return StatusCode(statusCode, new MessageActionResult(error));
    }
}
