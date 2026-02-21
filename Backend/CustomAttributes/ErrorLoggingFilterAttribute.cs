using Backend.Communication.Outgoing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.CustomAttributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
// This filter attribute is used to log exceptions that occur during the execution of an action method. It captures the exception details and logs them using the ILogger service.
// Additionally, it returns a standardized error response to the client with a 500 Internal Server Error status code.
public class ErrorLoggingFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        GenericResponse error = new(context.Exception.Message, isError: true);

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
