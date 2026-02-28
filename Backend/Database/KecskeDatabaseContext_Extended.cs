using Backend.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Backend.Database;

public partial class KecskeDatabaseContext
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "The other partial class is generated code")]
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
