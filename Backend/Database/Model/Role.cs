using System;
using System.Collections.Generic;

namespace Backend.Database.Model;

public partial class Role
{
    public int RoleId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedOnUtc { get; set; }

    public DateTime ModifiedOnUtc { get; set; }

    public virtual ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();

    public virtual ICollection<FileDirectoryRole> FileDirectoryRoles { get; set; } = new List<FileDirectoryRole>();
}
