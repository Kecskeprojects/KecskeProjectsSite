namespace DatabaseORM.Model;

public partial class Role
{
    public virtual ICollection<Account> Accounts { get; set; } = [];
    public virtual ICollection<FileDirectory> FileDirectories { get; set; } = [];
}
