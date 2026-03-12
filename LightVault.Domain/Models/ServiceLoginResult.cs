namespace LightVault.Domain.Models;

public sealed class ServiceLoginResult
{
    public string Token { get; init; } = string.Empty;
    public string AppName { get; init; } = string.Empty;
}