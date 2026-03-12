using LightVault.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace LightVault.Infrastructure.Services;

public sealed class Sha256PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public bool Verify(string password, string passwordHash)
    {
        var computedHash = Hash(password);
        return computedHash == passwordHash;
    }
}