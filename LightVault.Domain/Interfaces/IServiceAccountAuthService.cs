using LightVault.Domain.Models;

namespace LightVault.Domain.Interfaces;

public interface IServiceAccountAuthService
{
    Task<ServiceLoginResult?> LoginAsync(string apiKey, CancellationToken ct = default);
}