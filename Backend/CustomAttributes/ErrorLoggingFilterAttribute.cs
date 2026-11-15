using Backend.Communication.Internal;
using Backend.Communication.Outgoing;
using DatabaseORM.Communication.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.CustomAttributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
// This filter attribute logs exceptions that occur during the execution of an action method and returns a standardized error response.
// Only the message of an ApiException or a DatabaseORM ServiceException reaches the client, with the status code from GetClientStatusCode.
// Any other exception becomes a 500 with a generic message, because its message can contain internal details (paths, SQL, COM errors).
public class ErrorLoggingFilterAttribute : ExceptionFilterAttribute
{
    private const string GenericErrorMessage = "An unexpected error occurred.";

    public override void OnException(ExceptionContext context)
    {
        ILogger<ErrorLoggingFilterAttribute>? logger = context.HttpContext.RequestServices.GetService<ILogger<ErrorLoggingFilterAttribute>>();

        int statusCode;
        string message;
        if (GetClientStatusCode(context.Exception) is int clientStatusCode)
        {
            statusCode = clientStatusCode;
            message = context.Exception.Message;

            logger?.LogWarning(
                "A request was rejected: {Message}\nWith status code: {StatusCode}\nWith request path: {Path}",
                message,
                statusCode,
                context.HttpContext.Request.Path);
        }
        else
        {
            statusCode = StatusCodes.Status500InternalServerError;
            message = GenericErrorMessage;

            logger?.LogError(
                context.Exception,
                "An error occurred in the request.\nWith status code: {StatusCode}\nWith request path: {Path}",
                statusCode,
                context.HttpContext.Request.Path);
        }

        context.Result = new ObjectResult(new GenericResponse(message, isError: true))
        {
            StatusCode = statusCode,
            ContentTypes = { "application/json" }
        };
    }

    // The status code for exceptions whose message is safe to show to the client, null for any other exception
    private static int? GetClientStatusCode(Exception exception)
    {
        return exception switch
        {
            ApiException apiException => apiException.StatusCode,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            InvalidSecretException => StatusCodes.Status400BadRequest,
            // A new ServiceException subclass without its own mapping above
            ServiceException => StatusCodes.Status400BadRequest,
            _ => null
        };
    }
}
