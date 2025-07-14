using System;
using System.Collections.Generic;

namespace Backend.Database.Model;

public partial class Account
{
    public int AccountId { get; set; }

    public byte[] Password { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public DateTime LastLoginOnUtc { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime ModifiedOnUtc { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
