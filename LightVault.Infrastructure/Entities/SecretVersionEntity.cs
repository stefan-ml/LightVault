namespace LightVault.Infrastructure.Entities;

public class SecretVersionEntity
{
    public int Id { get; set; }
    public Guid SecretId { get; set; }
    public SecretEntity? Secret { get; set; }

    public int Version { get; set; }

    public byte[] Ciphertext { get; set; } = default!;
    public byte[] Nonce { get; set; } = default!;
    public byte[] Tag { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
}
