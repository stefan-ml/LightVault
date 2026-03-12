using LightVault.Domain.Models;

namespace LightVault.Domain.Interfaces;

public interface IAuditVerificationService
{
    Task<AuditVerificationResult> VerifyAsync(CancellationToken ct = default);
}