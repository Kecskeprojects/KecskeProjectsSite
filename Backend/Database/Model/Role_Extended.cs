namespace Backend.Database.Model;

public partial class Role
{
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
    public virtual ICollection<FileDirectory> FileDirectories { get; set; } = new List<FileDirectory>();
}
