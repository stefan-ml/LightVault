using LightVault.Infrastructure.Data;
using LightVault.Infrastructure.Entities;
using System.Security.Cryptography;
using System.Text;

namespace LightVault.Infrastructure.Seed;

public static class SeedData
{
    public static void EnsureSeeded(LightVaultDbContext db)
    {
        if (!db.Users.Any())
        {
            var admin = new UserEntity
            {
                Username = "admin",
                PasswordHash = Convert.ToBase64String(
                    SHA256.HashData(Encoding.UTF8.GetBytes("admin123"))
                ),
                Role = "Admin",
                IsActive = true
            };

            db.Users.Add(admin);
            db.SaveChanges();
        }
    }
}
