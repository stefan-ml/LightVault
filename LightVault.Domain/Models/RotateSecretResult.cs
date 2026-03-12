namespace LightVault.Domain.Models;

public sealed class RotateSecretResult
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int NewVersion { get; init; }
}