using LightVault.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace LightVault.Infrastructure.Services
{
    public class Sha256ApiKeyHasher : IApiKeyHasher
    {
        public (string hash, string salt) Hash(string apiKey)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var salt = Convert.ToBase64String(saltBytes);

            var combined = Encoding.UTF8.GetBytes(apiKey + salt);
            var hashBytes = SHA256.HashData(combined);

            return (Convert.ToBase64String(hashBytes), salt);
        }

        public bool Verify(string apiKey, string storedHash, string storedSalt)
        {
            var combined = Encoding.UTF8.GetBytes(apiKey + storedSalt);
            var hashBytes = SHA256.HashData(combined);
            var computedHash = Convert.ToBase64String(hashBytes);

            return computedHash == storedHash;
        }
    }
}
