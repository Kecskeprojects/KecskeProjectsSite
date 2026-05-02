namespace DatabaseORM.Model;

public partial class Role
{
    public int RoleId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedOnUtc { get; set; }

    public DateTime ModifiedOnUtc { get; set; }

    public virtual ICollection<AccountRole> AccountRoles { get; set; } = [];

    public virtual ICollection<FileDirectoryRole> FileDirectoryRoles { get; set; } = [];
}
