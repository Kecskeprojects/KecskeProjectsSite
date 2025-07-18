using Backend.Communication.Outgoing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.CustomAttributes;

public class ErrorLoggingFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        ErrorActionResult error = new(context.Exception.Message);

        context.Result = new ObjectResult(error)
        {
            StatusCode = StatusCodes.Status500InternalServerError,
            ContentTypes = { "application/json" }
        };
    }
}
