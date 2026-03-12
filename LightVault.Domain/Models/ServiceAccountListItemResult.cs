namespace LightVault.Domain.Models;

public sealed class ServiceAccountListItemResult
{
    public Guid Id { get; init; }
    public string AppName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? LastUsedAtUtc { get; init; }
}