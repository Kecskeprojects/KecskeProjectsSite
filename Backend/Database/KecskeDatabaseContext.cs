using System;
using System.Collections.Generic;
using Backend.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Database;

public partial class KecskeDatabaseContext : DbContext
{
    public KecskeDatabaseContext(DbContextOptions<KecskeDatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK_Account_AccountId");

            entity.ToTable("Account", tb => tb.HasTrigger("AccountUpdated"));

            entity.HasIndex(e => e.Email, "UQ_Account_Email").IsUnique();

            entity.Property(e => e.CreatedOnUtc)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.LastLoginUtc).HasColumnType("datetime");
            entity.Property(e => e.ModifiedOnUtc)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(84);
            entity.Property(e => e.UserName).HasMaxLength(100);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK_Role_RoleId");

            entity.ToTable("Role", tb => tb.HasTrigger("RoleUpdated"));

            entity.HasIndex(e => e.Name, "UQ_Role_Name").IsUnique();

            entity.Property(e => e.CreatedOnUtc)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedOnUtc)
                .HasDefaultValueSql("(getutcdate())")
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
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
