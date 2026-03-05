using Microsoft.EntityFrameworkCore;
using LightVault.Infrastructure.Entities;

namespace LightVault.Infrastructure.Data;

public class LightVaultDbContext : DbContext
{
    public LightVaultDbContext(DbContextOptions<LightVaultDbContext> options)
        : base(options) { }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<SecretEntity> Secrets => Set<SecretEntity>();
    public DbSet<SecretVersionEntity> SecretVersions => Set<SecretVersionEntity>();
    public DbSet<AuditEntryEntity> AuditEntries => Set<AuditEntryEntity>();

    public DbSet<ServiceAccount> ServiceAccounts => Set<ServiceAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>()
            .HasIndex(x => x.Username)
            .IsUnique();

        modelBuilder.Entity<SecretVersionEntity>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<ServiceAccount>(e =>
        {
            e.HasIndex(x => x.AppName).IsUnique();
        });
    }
}
