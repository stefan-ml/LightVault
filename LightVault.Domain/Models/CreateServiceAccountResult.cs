namespace LightVault.Domain.Models;

public sealed class CreateServiceAccountResult
{
    public CreateServiceAccountStatus Status { get; init; }

    public Guid? Id { get; init; }
    public string? AppName { get; init; }
    public string? Role { get; init; }
    public string? ApiKey { get; init; }
}