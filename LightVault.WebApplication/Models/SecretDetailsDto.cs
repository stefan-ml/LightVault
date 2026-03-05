namespace LightVault.WebApplication.Models;

public class SecretDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public int CurrentVersion { get; set; }
    public string? Value { get; set; }
    public List<SecretVersionDto> Versions { get; set; } = new();
    public DateTime? ExpiresAt { get; set; }
    public bool CanModifySecret { get; set; }
}
