using Backend.Communication.Outgoing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json;

namespace Backend.Authentication;

public sealed class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
{
    private static readonly JsonSerializerOptions DefaultJsonSerializeOption = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public override async Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        ArgumentNullException.ThrowIfNull(context);

        GenericResponse error = new("You do not have the right roles for this page!", isError: true);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status403Forbidden;

        await context.Response.WriteAsync(JsonSerializer.Serialize(error, DefaultJsonSerializeOption));
    }

    public override async Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        ArgumentNullException.ThrowIfNull(context);

        GenericResponse error = new("You are not logged in!", isError: true);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

        await context.Response.WriteAsync(JsonSerializer.Serialize(error, DefaultJsonSerializeOption));
    }
}
