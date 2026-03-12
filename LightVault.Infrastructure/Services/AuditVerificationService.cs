using LightVault.Domain.Interfaces;
using LightVault.Domain.Models;

namespace LightVault.Infrastructure.Services;

public sealed class AuditVerificationService : IAuditVerificationService
{
    private readonly AuditService _auditService;

    public AuditVerificationService(AuditService auditService)
    {
        _auditService = auditService;
    }

    public async Task<AuditVerificationResult> VerifyAsync(CancellationToken ct = default)
    {
        var result = await _auditService.VerifyChainAsync(ct);

        return new AuditVerificationResult
        {
            IsValid = result.IsValid,
            Message = result.IsValid ? "Ok" : $"{result.Reason} Broken at:  {result.BrokenAtId}",
        };
    }
}