using DatabaseORM.Model;

namespace DatabaseORM.Communication.Resource;

public class AccountResource
{
    public int AccountId { get; set; }

    public string? UserName { get; set; }

    public bool IsRegistrationApproved { get; set; }

    public DateTime? LastLoginOnUtc { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime ModifiedOnUtc { get; set; }

    public virtual List<Role?>? Roles { get; set; }
}
