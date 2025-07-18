namespace Backend.Communication.Outgoing;

public class ErrorActionResult(string errorMessage)
{
    public string Error { get; private set; } = errorMessage;
}
