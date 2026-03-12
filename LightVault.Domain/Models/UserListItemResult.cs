namespace LightVault.Domain.Models;

public sealed class UserListItemResult
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}