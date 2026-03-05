namespace LightVault.WebApplication.Models;

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string Role { get; set; } = default!;
    public bool IsActive { get; set; }
}
