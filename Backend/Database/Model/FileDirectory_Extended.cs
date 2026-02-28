namespace Backend.Database.Model;

public partial class FileDirectory
{
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
