using System;
using System.Collections.Generic;

namespace Backend.Database.Model;

public partial class PermittedIpAddress
{
    public int PermittedIpAddressId { get; set; }

    public int AccountId { get; set; }

    public string IpAddress { get; set; } = null!;

    public DateTime ExpiresOnUtc { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime ModifiedOnUtc { get; set; }

    public virtual Account Account { get; set; } = null!;
}
