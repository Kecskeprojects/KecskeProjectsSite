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

    public virtual DbSet<AccountRole> AccountRoles { get; set; }

    public virtual DbSet<FileDirectory> FileDirectories { get; set; }

    public virtual DbSet<FileDirectoryRole> FileDirectoryRoles { get; set; }

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

        modelBuilder.Entity<AccountRole>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.AccountId }).HasName("PK_AccountRole_RoleId_AccountId");

            entity.ToTable("AccountRole");

            entity.Property(e => e.CreatedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_AccountRole_CreatedOnUtc")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountRoles)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AccountRole_Account");

            entity.HasOne(d => d.Role).WithMany(p => p.AccountRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AccountRole_Server");
        });

        modelBuilder.Entity<FileDirectory>(entity =>
        {
            entity.HasKey(e => e.FileDirectoryId).HasName("PK_FileDirectory_FileDirectoryId");

            entity.ToTable("FileDirectory", tb => tb.HasTrigger("FileDirectoryUpdated"));

            entity.HasIndex(e => e.RelativePath, "UQ_FileDirectory_Name").IsUnique();

            entity.Property(e => e.CreatedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_FileDirectory_CreatedOnUtc")
                .HasColumnType("datetime");
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.ModifiedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_FileDirectory_ModifiedOnUtc")
                .HasColumnType("datetime");
            entity.Property(e => e.RelativePath).HasMaxLength(200);
        });

        modelBuilder.Entity<FileDirectoryRole>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.FileDirectoryId }).HasName("PK_FileDirectoryRole_RoleId_FileDirectory");

            entity.ToTable("FileDirectoryRole");

            entity.Property(e => e.CreatedOnUtc)
                .HasDefaultValueSql("(getutcdate())", "DF_FileDirectoryRole_CreatedOnUtc")
                .HasColumnType("datetime");

            entity.HasOne(d => d.FileDirectory).WithMany(p => p.FileDirectoryRoles)
                .HasForeignKey(d => d.FileDirectoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FileDirectoryRole_FileDirectory");

            entity.HasOne(d => d.Role).WithMany(p => p.FileDirectoryRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FileDirectoryRole_Role");
        });

        modelBuilder.Entity<PermittedIpAddress>(entity =>
        {
            entity.HasKey(e => e.PermittedIpAddressId).HasName("PK_PermittedIpAddress_PermittedIpAddressId");

            entity.ToTable("PermittedIpAddress", tb => tb.HasTrigger("PermittedIpAddressUpdated"));

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
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
