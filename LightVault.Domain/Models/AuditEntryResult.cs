namespace LightVault.Domain.Models;

public sealed class AuditEntryResult
{
    public long Id { get; init; }
    public string Actor { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string? Details { get; init; }
    public DateTime TimestampUtc { get; init; }
    public string Hash { get; init; } = string.Empty;
    public string PreviousHash { get; init; } = string.Empty;
}