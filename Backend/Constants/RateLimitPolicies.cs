namespace Backend.Constants;

public static class RateLimitPolicies
{
    /// <summary>
    /// Per-IP limit for unauthenticated endpoints that check credentials (login, register, password reset)
    /// </summary>
    public const string Authentication = "Authentication";

    public const int AuthenticationPermitLimit = 5;
    public static readonly TimeSpan AuthenticationWindow = TimeSpan.FromMinutes(1);
}
