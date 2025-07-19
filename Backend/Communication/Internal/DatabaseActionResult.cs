using Backend.Enums;

namespace Backend.Communication.Internal;

public class DatabaseActionResult<T>(DatabaseActionResultEnum result, T? data = default, string? specialMessage = null)
{
    public DatabaseActionResultEnum Status { get; private set; } = result;
    public T? Data { get; private set; } = data;
    public string? SpecialMessage { get; private set; } = specialMessage;
}
