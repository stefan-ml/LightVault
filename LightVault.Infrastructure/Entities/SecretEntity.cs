namespace LightVault.Infrastructure.Entities;

public class SecretEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public int CurrentVersion { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid CreatedByUserId { get; set; }

    public List<SecretVersionEntity> Versions { get; set; } = new();
}
