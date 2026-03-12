using LightVault.Domain.Models;

namespace LightVault.Domain.Interfaces;

public interface IAuditQueryService
{
    Task<IReadOnlyList<AuditEntryResult>> GetAllAsync(CancellationToken ct = default);

    Task<IReadOnlyList<AuditEntryResult>> FilterAsync(
        string? user,
        string? action,
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default);
}