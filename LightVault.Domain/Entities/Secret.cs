namespace LightVault.Domain.Entities;

public class Secret
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public int CurrentVersion { get; set; } = 1;
    public string? Tags { get; set; }
    public string? RotationPolicy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
