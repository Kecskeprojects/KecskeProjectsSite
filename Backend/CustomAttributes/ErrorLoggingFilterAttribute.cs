using Backend.Communication.Outgoing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.CustomAttributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public class ErrorLoggingFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        ErrorActionResult error = new(context.Exception.Message);

        context.HttpContext.RequestServices
            .GetService<ILogger<ErrorLoggingFilterAttribute>>()?
            .LogError(
                context.Exception,
                "An error occurred in the request.\nWith status code: {statusCode}\nWith request path: {Path}",
                context.HttpContext.Response.StatusCode,
                context.HttpContext.Request.Path);

        context.Result = new ObjectResult(error)
        {
            StatusCode = StatusCodes.Status500InternalServerError,
            ContentTypes = { "application/json" }
        };
    }
}
