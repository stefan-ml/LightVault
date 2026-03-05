namespace LightVault.WebApplication.Models;

public class SecretDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public int CurrentVersion { get; set; }
}
