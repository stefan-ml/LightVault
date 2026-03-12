namespace LightVault.Domain.Models;

public sealed class AuditVerificationResult
{
    public bool IsValid { get; init; }
    public string Message { get; init; } = string.Empty;
}