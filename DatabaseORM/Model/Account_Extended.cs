namespace DatabaseORM.Model;

public partial class Account
{
    public virtual ICollection<Role> Roles { get; set; } = [];
}
