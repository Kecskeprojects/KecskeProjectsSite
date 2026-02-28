namespace Backend.Database.Model;

public partial class Account
{
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
