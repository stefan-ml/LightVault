namespace LightVault.Domain.Models;

public sealed class SecretListItemResult
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int CurrentVersion { get; init; }
}