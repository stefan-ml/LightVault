namespace LightVault.Domain.Models;

public sealed class SecretByNameResult
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int CurrentVersion { get; init; }
    public string Value { get; init; } = string.Empty;
    public DateTime? ExpiresAt { get; init; }
}