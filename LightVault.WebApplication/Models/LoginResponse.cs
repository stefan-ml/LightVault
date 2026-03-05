namespace LightVault.WebApplication.Models;

public class LoginResponse
{
    public string Token { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Role { get; set; } = default!;
}
