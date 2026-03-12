namespace LightVault.Domain.Models;

public sealed class SecretVersionDetailsResult
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Version { get; init; }
    public string Value { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
}