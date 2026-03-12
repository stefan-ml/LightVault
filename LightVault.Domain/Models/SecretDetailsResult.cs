namespace LightVault.Domain.Models;

public sealed class SecretDetailsResult
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int CurrentVersion { get; init; }
    public string Value { get; init; } = string.Empty;
    public DateTime? ExpiresAt { get; init; }
    public bool CanModifySecret { get; init; }
    public IReadOnlyList<SecretVersionItemResult> Versions { get; init; } = [];
}