using System;
using System.Collections.Generic;
using Backend.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Backend.Database;

public partial class KecskeDatabaseContext : DbContext
{
    public KecskeDatabaseContext(DbContextOptions<KecskeDatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<FileFolder> FileFolders { get; set; }

    public virtual DbSet<PermittedIpAddress> PermittedIpAddresses { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK_Account_AccountId");

            entity.ToTable("Account", tb => tb.HasTrigger("AccountUpdated"));

            entity.HasIndex(e => e.UserName, "UQ_Account_UserName").IsUnique();

            entity.Property(e => e.CreatedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_Account_CreatedOnUtc")
                .HasColumnType("datetime");
            entity.Property(e => e.LastLoginOnUtc).HasColumnType("datetime");
            entity.Property(e => e.ModifiedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_Account_ModifiedOnUtc")
                .HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(84);
            entity.Property(e => e.SecretKey).HasMaxLength(84);
            entity.Property(e => e.UserName).HasMaxLength(200);
        });

        modelBuilder.Entity<FileFolder>(entity =>
        {
            entity.HasKey(e => e.FileFolderId).HasName("PK_FileFolder_FileFolderId");

            entity.ToTable("FileFolder", tb => tb.HasTrigger("FileFolderUpdated"));

            entity.HasIndex(e => e.RelativePath, "UQ_FileFolder_Name").IsUnique();

            entity.Property(e => e.CreatedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_Folder_CreatedOnUtc")
                .HasColumnType("datetime");
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.ModifiedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_Folder_ModifiedOnUtc")
                .HasColumnType("datetime");
            entity.Property(e => e.RelativePath).HasMaxLength(200);
        });

        modelBuilder.Entity<PermittedIpAddress>(entity =>
        {
            entity.HasKey(e => e.PermittedIpAddressId).HasName("PK_PermittedIpAddress_PermittedIpAddressId");

            entity.ToTable("PermittedIpAddress");

            entity.Property(e => e.CreatedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_PermittedIpAddress_CreatedOnUtc")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiresOnUtc).HasColumnType("datetime");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_PermittedIpAddress_ModifiedOnUtc")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.PermittedIpAddresses)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PermittedIpAddress_AccountId");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK_Role_RoleId");

            entity.ToTable("Role", tb => tb.HasTrigger("RoleUpdated"));

            entity.HasIndex(e => e.Name, "UQ_Role_Name").IsUnique();

            entity.Property(e => e.CreatedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_Role_CreatedOnUtc")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_Role_ModifiedOnUtc")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasMany(d => d.Accounts).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "AccountRole",
                    r => r.HasOne<Account>().WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_AccountRole_Account"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_AccountRole_Server"),
                    j =>
                    {
                        j.HasKey("RoleId", "AccountId").HasName("PK_AccountRole_RoleId_AccountId");
                        j.ToTable("AccountRole");
                    });

            entity.HasMany(d => d.FileFolders).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "FileFolderRole",
                    r => r.HasOne<FileFolder>().WithMany()
                        .HasForeignKey("FileFolderId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_FileFolderRole_FileFolder"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_FileFolderRole_Role"),
                    j =>
                    {
                        j.HasKey("RoleId", "FileFolderId").HasName("PK_FileFolderRole_RoleId_FileFolder");
                        j.ToTable("FileFolderRole");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
