namespace Backend.Communication.Internal;

/// <summary>
/// An exception whose message is safe to show to the client.
/// ErrorLoggingFilterAttribute returns its message with its status code;
/// any other exception becomes a 500 with a generic message, so internal details don't leak.
/// </summary>
public class ApiException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
