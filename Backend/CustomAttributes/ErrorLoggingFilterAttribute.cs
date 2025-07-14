using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.CustomAttributes;

public class ErrorLoggingFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        //Todo: Define error handling logic here
        //context.Result = new Microsoft.AspNetCore.Mvc.OkObjectResult("This error was handled"); // Internal Server Error
    }
}
