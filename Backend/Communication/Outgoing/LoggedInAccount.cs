
namespace Backend.Communication.Outgoing;

public class LoggedInAccount
{
    public int AccountId { get; set; }
    public string? UserName { get; set; }
    public List<string?> Roles { get; set; } = [];
}
