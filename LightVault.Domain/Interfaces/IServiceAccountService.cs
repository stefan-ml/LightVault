using LightVault.Domain.Models;

namespace LightVault.Domain.Interfaces;

public interface IServiceAccountService
{
    Task<CreateServiceAccountResult> CreateAsync(
        string appName,
        string? role,
        CancellationToken ct = default);

    Task<IReadOnlyList<ServiceAccountListItemResult>> GetAllAsync(
        CancellationToken ct = default);

    Task<RevokeServiceAccountStatus> RevokeAsync(
        Guid id,
        CancellationToken ct = default);
}