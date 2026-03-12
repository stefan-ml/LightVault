using LightVault.Domain.Models;

namespace LightVault.Domain.Interfaces;

public interface ISecretService
{
    Task<IReadOnlyList<SecretListItemResult>> GetAllAsync(CancellationToken ct = default);
    Task<SecretDetailsResult?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Guid> CreateAsync(string name, string value, CancellationToken ct = default);
    Task<SecretVersionDetailsResult?> GetByVersionAsync(Guid id, int version, CancellationToken ct = default);
    Task<RotateSecretResult?> RotateAsync(Guid id, string newValue, CancellationToken ct = default);
    Task<SecretByNameResult?> GetByNameAsync(string name, CancellationToken ct = default);
}