namespace LightVault.Infrastructure.Entities;

public class AuditEntryEntity
{
    public long Id { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string Actor { get; set; } = default!;
    public string Action { get; set; } = default!;
    public Guid? SecretId { get; set; }
    public string? Details { get; set; }


    public string PrevHash { get; set; } = default!;
    public string Hash { get; set; } = default!;
}
