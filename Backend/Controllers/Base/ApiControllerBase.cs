using Backend.Communication.Outgoing;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.Base;

public class ApiControllerBase : ControllerBase
{
    protected IActionResult MessageResult(string message)
    {
        return Ok(new MessageActionResult(message));
    }

    protected IActionResult ErrorResult(int statusCode, string error)
    {
        return StatusCode(statusCode, new MessageActionResult(error));
    }
}
