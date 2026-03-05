namespace LightVault.Infrastructure.Entities;

public class UserEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string Role { get; set; } = "Developer";
    public bool IsActive { get; set; } = true;
}
