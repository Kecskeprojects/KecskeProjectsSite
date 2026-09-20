using System.Runtime.Versioning;

namespace Backend.Middleware;

/// <summary>
/// Middleware that adds security-related HTTP response headers to all responses.
/// Includes Content Security Policy, X-Frame-Options, and other protective headers.
/// </summary>
[SupportedOSPlatform("windows")]
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public SecurityHeadersMiddleware(RequestDelegate next, IConfiguration configuration, IWebHostEnvironment environment)
    {
        _next = next;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // X-Content-Type-Options: Prevents MIME-type sniffing
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

        // X-Frame-Options: Prevents clickjacking by disallowing embedding in iframes
        context.Response.Headers.Add("X-Frame-Options", "DENY");

        // X-XSS-Protection: Legacy header for older browsers to block XSS attempts
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

        // Referrer-Policy: Controls referrer information sent to other sites
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

        // Permissions-Policy: Disables potentially dangerous browser features
        context.Response.Headers.Add("Permissions-Policy", 
            "geolocation=(), microphone=(), camera=(), payment=(), usb=(), accelerometer=(), gyroscope=()");

        // Content-Security-Policy: Whitelist policy for resource loading and execution
        // Uses Report-Only in development to monitor violations without blocking
        string cspHeaderName = _environment.IsProduction() 
            ? "Content-Security-Policy" 
            : "Content-Security-Policy-Report-Only";

        string cspPolicy = _configuration["SecurityHeaders:ContentSecurityPolicy"] 
            ?? "default-src 'self'; script-src 'self' cdn.jsdelivr.net; style-src 'self' cdn.jsdelivr.net 'unsafe-inline'; img-src 'self' data:; font-src 'self'";

        context.Response.Headers.Add(cspHeaderName, cspPolicy);

        await _next(context);
    }
}
