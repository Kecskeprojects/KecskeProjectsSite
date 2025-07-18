namespace Backend.Communication.Outgoing;

public class MessageActionResult(string message)
{
    public string Message { get; private set; } = message;
}
