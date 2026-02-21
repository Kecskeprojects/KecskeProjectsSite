using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Backend.CustomAttributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
// This attribute can be applied to controllers or actions to disable form value model binding for the decorated endpoint.
// This is useful for endpoints that handle file uploads, as it prevents the default model binding from interfering with the processing of multipart form data.
public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        IList<IValueProviderFactory> factories = context.ValueProviderFactories;
        factories.RemoveType<FormValueProviderFactory>();
        factories.RemoveType<FormFileValueProviderFactory>();
        factories.RemoveType<JQueryFormValueProviderFactory>();
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
}
