using Backend.Communication.Outgoing;
using Backend.Tools;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.Base;

public class ApiControllerBase(ILogger logger) : ControllerBase
{
    protected ILogger Logger { get; } = logger;

    [NonAction]
    protected LoggedInAccount? GetLoggedInUserFromCookie()
    {
        return ClaimsPrincipalTools.GetLoggedInUser(HttpContext.User);
    }

    [NonAction]
    protected IActionResult MessageResult(string message)
    {
        return Ok(new MessageActionResult(message));
    }

    [NonAction]
    protected IActionResult ErrorResult(int statusCode, string error)
    {
        Logger.LogError(
            "An error occurred: {error}\nWith status code: {statusCode}\nWith request path: {Path}",
            error,
            statusCode,
            HttpContext.Request.Path);
        return StatusCode(statusCode, new MessageActionResult(error));
    }
}
