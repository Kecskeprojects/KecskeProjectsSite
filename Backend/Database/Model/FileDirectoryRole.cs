using System;
using System.Collections.Generic;

namespace Backend.Database.Model;

public partial class FileDirectoryRole
{
    public int FileDirectoryId { get; set; }

    public int RoleId { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public virtual FileDirectory FileDirectory { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
