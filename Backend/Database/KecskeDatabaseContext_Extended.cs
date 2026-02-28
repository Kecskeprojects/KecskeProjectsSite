using Backend.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Backend.Database;

public partial class KecskeDatabaseContext
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "The other partial class is generated code")]
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
