using DatabaseORM.Model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseORM;

public partial class KecskeDatabaseContext : DbContext
{
    public KecskeDatabaseContext(DbContextOptions<KecskeDatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AccountRole> AccountRoles { get; set; }

    public virtual DbSet<FileDirectory> FileDirectories { get; set; }

    public virtual DbSet<FileDirectoryRole> FileDirectoryRoles { get; set; }

    public virtual DbSet<PermittedIpAddress> PermittedIpAddresses { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        _ = modelBuilder.Entity<Account>(entity =>
        {
            _ = entity.HasKey(e => e.AccountId).HasName("PK_Account_AccountId");

            _ = entity.ToTable("Account", tb => tb.HasTrigger("AccountUpdated"));

            _ = entity.HasIndex(e => e.UserName, "UQ_Account_UserName").IsUnique();

            _ = entity.Property(e => e.CreatedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_Account_CreatedOnUtc")
                .HasColumnType("datetime");
            _ = entity.Property(e => e.LastLoginOnUtc).HasColumnType("datetime");
            _ = entity.Property(e => e.ModifiedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_Account_ModifiedOnUtc")
                .HasColumnType("datetime");
            _ = entity.Property(e => e.Password).HasMaxLength(84);
            _ = entity.Property(e => e.SecretKey).HasMaxLength(84);
            _ = entity.Property(e => e.UserName).HasMaxLength(200);
        });

        _ = modelBuilder.Entity<AccountRole>(entity =>
        {
            _ = entity.HasKey(e => new { e.RoleId, e.AccountId }).HasName("PK_AccountRole_RoleId_AccountId");

            _ = entity.ToTable("AccountRole");

            _ = entity.Property(e => e.CreatedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_AccountRole_CreatedOnUtc")
                .HasColumnType("datetime");

            _ = entity.HasOne(d => d.Account).WithMany(p => p.AccountRoles)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AccountRole_Account");

            _ = entity.HasOne(d => d.Role).WithMany(p => p.AccountRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AccountRole_Server");
        });

        _ = modelBuilder.Entity<FileDirectory>(entity =>
        {
            _ = entity.HasKey(e => e.FileDirectoryId).HasName("PK_FileDirectory_FileDirectoryId");

            _ = entity.ToTable("FileDirectory", tb => tb.HasTrigger("FileDirectoryUpdated"));

            _ = entity.HasIndex(e => e.RelativePath, "UQ_FileDirectory_Name").IsUnique();

            _ = entity.Property(e => e.CreatedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_FileDirectory_CreatedOnUtc")
                .HasColumnType("datetime");
            _ = entity.Property(e => e.DisplayName).HasMaxLength(200);
            _ = entity.Property(e => e.ModifiedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_FileDirectory_ModifiedOnUtc")
                .HasColumnType("datetime");
            _ = entity.Property(e => e.RelativePath).HasMaxLength(200);
        });

        _ = modelBuilder.Entity<FileDirectoryRole>(entity =>
        {
            _ = entity.HasKey(e => new { e.RoleId, e.FileDirectoryId }).HasName("PK_FileDirectoryRole_RoleId_FileDirectory");

            _ = entity.ToTable("FileDirectoryRole");

            _ = entity.Property(e => e.CreatedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_FileDirectoryRole_CreatedOnUtc")
                .HasColumnType("datetime");

            _ = entity.HasOne(d => d.FileDirectory).WithMany(p => p.FileDirectoryRoles)
                .HasForeignKey(d => d.FileDirectoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FileDirectoryRole_FileDirectory");

            _ = entity.HasOne(d => d.Role).WithMany(p => p.FileDirectoryRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FileDirectoryRole_Role");
        });

        _ = modelBuilder.Entity<PermittedIpAddress>(entity =>
        {
            _ = entity.HasKey(e => e.PermittedIpAddressId).HasName("PK_PermittedIpAddress_PermittedIpAddressId");

            _ = entity.ToTable("PermittedIpAddress", tb => tb.HasTrigger("PermittedIpAddressUpdated"));

            _ = entity.Property(e => e.CreatedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_PermittedIpAddress_CreatedOnUtc")
                .HasColumnType("datetime");
            _ = entity.Property(e => e.ExpiresOnUtc).HasColumnType("datetime");
            _ = entity.Property(e => e.IpAddress)
                .HasMaxLength(15)
                .IsUnicode(false);
            _ = entity.Property(e => e.ModifiedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_PermittedIpAddress_ModifiedOnUtc")
                .HasColumnType("datetime");

            _ = entity.HasOne(d => d.Account).WithMany(p => p.PermittedIpAddresses)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PermittedIpAddress_AccountId");
        });

        _ = modelBuilder.Entity<Role>(entity =>
        {
            _ = entity.HasKey(e => e.RoleId).HasName("PK_Role_RoleId");

            _ = entity.ToTable("Role", tb => tb.HasTrigger("RoleUpdated"));

            _ = entity.HasIndex(e => e.Name, "UQ_Role_Name").IsUnique();

            _ = entity.Property(e => e.CreatedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_Role_CreatedOnUtc")
                .HasColumnType("datetime");
            _ = entity.Property(e => e.ModifiedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_Role_ModifiedOnUtc")
                .HasColumnType("datetime");
            _ = entity.Property(e => e.Name).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
