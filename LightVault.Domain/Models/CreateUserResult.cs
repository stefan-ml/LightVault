namespace LightVault.Domain.Models;

public sealed class CreateUserResult
{
    public CreateUserStatus Status { get; init; }
    public Guid? Id { get; init; }
    public string? Username { get; init; }
    public string? Role { get; init; }
    public bool? IsActive { get; init; }
}