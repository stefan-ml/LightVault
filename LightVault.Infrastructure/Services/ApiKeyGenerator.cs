using LightVault.Domain.Interfaces;
using System.Security.Cryptography;

namespace LightVault.Infrastructure.Services;

public sealed class ApiKeyGenerator : IApiKeyGenerator
{
    public string Generate()
    {
        var apiKeyBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(apiKeyBytes);
    }
}