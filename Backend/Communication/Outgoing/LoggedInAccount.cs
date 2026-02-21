
namespace Backend.Communication.Outgoing;

public class LoggedInAccount
{
    public int AccountId { get; set; }
    public string UserName { get; set; } = null!;
    public List<string> Roles { get; set; } = null!;
}
