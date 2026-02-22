using Backend.Communication.Outgoing;
using Backend.Tools;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.Base;

public class ApiControllerBase(ILogger logger) : ControllerBase
{
    protected ILogger Logger { get; } = logger;
    protected LoggedInAccount? LoggedInAccount => ClaimsPrincipalTools.GetLoggedInAccount(HttpContext.User);

    [NonAction]
    protected IActionResult ContentResult<T>(T content)
    {
        return Ok(new GenericResponse<T>(content));
    }

    [NonAction]
    protected IActionResult MessageResult(string message)
    {
        return Ok(new GenericResponse(message));
    }

    [NonAction]
    protected IActionResult ErrorResult(int statusCode, string error)
    {
        Logger.LogError($"An error occurred: {error}\nWith status code: {statusCode}\nWith request path: {HttpContext.Request.Path}");
        return StatusCode(statusCode, new GenericResponse(error, isError: true));
    }
}
