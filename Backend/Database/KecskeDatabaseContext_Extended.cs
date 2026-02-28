using Backend.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Backend.Database;

public partial class KecskeDatabaseContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity
                .HasMany(d => d.Accounts)
                .WithMany(p => p.Roles)
                .UsingEntity<AccountRole>();

            entity
                .HasMany(d => d.FileDirectories)
                .WithMany(p => p.Roles)
                .UsingEntity<FileDirectoryRole>();
        });
    }
}
