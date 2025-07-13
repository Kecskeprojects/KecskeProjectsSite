﻿using System;
using System.Collections.Generic;

namespace Backend.Database.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedOnUtc { get; set; }

    public DateTime ModifiedOnUtc { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
