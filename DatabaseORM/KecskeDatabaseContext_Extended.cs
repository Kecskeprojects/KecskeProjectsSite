using DatabaseORM.Model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseORM;

public partial class KecskeDatabaseContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<Role>(entity =>
        {
            _ = entity
                .HasMany(d => d.Accounts)
                .WithMany(p => p.Roles)
                .UsingEntity<AccountRole>();

            _ = entity
                .HasMany(d => d.FileDirectories)
                .WithMany(p => p.Roles)
                .UsingEntity<FileDirectoryRole>();
        });
    }
}
