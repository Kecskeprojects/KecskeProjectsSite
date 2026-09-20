using System.Collections.Concurrent;
using System.Runtime.Versioning;

namespace Backend.Middleware;

/// <summary>
/// Middleware that implements rate limiting to prevent brute-force attacks.
/// Tracks failed login attempts by IP address and applies exponential backoff.
/// Applies to unauthenticated and unauthorized endpoints only.
/// </summary>
[SupportedOSPlatform("windows")]
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, LoginAttemptTracker> AttemptTrackers = 
        new();

    // Configuration
    private const int MaxAttempts = 5;
    private const int BaseBackoffSeconds = 2; // Exponential base: 2^n seconds

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only apply rate limiting to login endpoint
        if (context.Request.Path.StartsWithSegments("/api/Account/Login", StringComparison.OrdinalIgnoreCase))
        {
            string clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Get or create tracker for this IP
            LoginAttemptTracker tracker = AttemptTrackers.AddOrUpdate(
                clientIp,
                new LoginAttemptTracker(),
                (_, existing) => existing);

            // Check if client is currently in cooldown
            if (tracker.IsInCooldown(out int remainingSeconds))
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers.Add("Retry-After", remainingSeconds.ToString());
                await context.Response.WriteAsync($"Too many login attempts. Please try again in {remainingSeconds} seconds.");
                return;
            }
        }

        // Store the original response stream
        using (var responseBody = new MemoryStream())
        {
            Stream originalBody = context.Response.Body;
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                // Check if this is a failed login attempt (401 response to login endpoint)
                if (context.Request.Path.StartsWithSegments("/api/Account/Login", StringComparison.OrdinalIgnoreCase) 
                    && context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    string clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    LoginAttemptTracker tracker = AttemptTrackers.AddOrUpdate(
                        clientIp,
                        new LoginAttemptTracker(),
                        (_, existing) => existing);

                    tracker.RecordFailedAttempt();

                    // Apply exponential backoff after exceeding max attempts
                    if (tracker.FailedAttempts >= MaxAttempts)
                    {
                        int backoffSeconds = (int)Math.Pow(BaseBackoffSeconds, tracker.FailedAttempts - MaxAttempts);
                        tracker.EnterCooldown(backoffSeconds);
                    }
                }
                // Clear failed attempts on successful login (200 response)
                else if (context.Request.Path.StartsWithSegments("/api/Account/Login", StringComparison.OrdinalIgnoreCase) 
                    && context.Response.StatusCode == StatusCodes.Status200OK)
                {
                    string clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    if (AttemptTrackers.TryGetValue(clientIp, out LoginAttemptTracker? tracker))
                    {
                        tracker.Reset();
                    }
                }

                await responseBody.CopyToAsync(originalBody);
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }
    }

    /// <summary>
    /// Tracks login attempts for a single IP address
    /// </summary>
    private class LoginAttemptTracker
    {
        public int FailedAttempts { get; set; }
        public DateTime CooldownUntil { get; set; }

        public bool IsInCooldown(out int remainingSeconds)
        {
            if (DateTime.UtcNow < CooldownUntil)
            {
                remainingSeconds = (int)Math.Ceiling((CooldownUntil - DateTime.UtcNow).TotalSeconds);
                return true;
            }

            remainingSeconds = 0;
            return false;
        }

        public void EnterCooldown(int seconds)
        {
            CooldownUntil = DateTime.UtcNow.AddSeconds(seconds);
        }

        public void RecordFailedAttempt()
        {
            FailedAttempts++;
        }

        public void Reset()
        {
            FailedAttempts = 0;
            CooldownUntil = DateTime.UtcNow;
        }
    }
}
