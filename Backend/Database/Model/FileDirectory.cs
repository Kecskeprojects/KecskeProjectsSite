using System;
using System.Collections.Generic;

namespace Backend.Database.Model;

public partial class FileDirectory
{
    public int FileDirectoryId { get; set; }

    public string RelativePath { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public DateTime CreatedOnUtc { get; set; }

    public DateTime ModifiedOnUtc { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
