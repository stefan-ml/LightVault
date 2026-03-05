namespace LightVault.WebApplication.Models;

public class AuditEntryDto
{
    public long Id { get; set; }
    public string Actor { get; set; } = default!;
    public string Action { get; set; } = default!;
    public Guid? SecretId { get; set; }
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
}
