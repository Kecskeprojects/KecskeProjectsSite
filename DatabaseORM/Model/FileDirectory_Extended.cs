namespace DatabaseORM.Model;

public partial class FileDirectory
{
    public virtual ICollection<Role> Roles { get; set; } = [];
}
