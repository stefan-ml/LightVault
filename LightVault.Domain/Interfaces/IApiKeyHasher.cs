namespace LightVault.Domain.Interfaces;

public interface IApiKeyHasher
{
    (string hash, string salt) Hash(string apiKey);
    bool Verify(string apiKey, string storedHash, string storedSalt);
}