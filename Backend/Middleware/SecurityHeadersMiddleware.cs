using System.Runtime.Versioning;

namespace Backend.Middleware;

/// <summary>
/// Middleware that adds security-related HTTP response headers to all responses.
/// Content-Security-Policy is intentionally not set here: the backend only serves JSON and files,
/// a CSP for the site belongs on whatever serves the frontend's HTML.
/// </summary>
[SupportedOSPlatform("windows")]
public class SecurityHeadersMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // X-Content-Type-Options: Prevents MIME-type sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // X-Frame-Options: Prevents clickjacking by disallowing embedding in iframes
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // X-XSS-Protection: Explicitly disables the legacy XSS filter of older browsers,
        // as the filter itself could be abused to leak data (OWASP recommendation)
        context.Response.Headers.Append("X-XSS-Protection", "0");

        // Referrer-Policy: Controls referrer information sent to other sites
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Permissions-Policy: Disables potentially dangerous browser features
        context.Response.Headers.Append("Permissions-Policy",
            "geolocation=(), microphone=(), camera=(), payment=(), usb=(), accelerometer=(), gyroscope=()");

        await _next(context);
    }
}
