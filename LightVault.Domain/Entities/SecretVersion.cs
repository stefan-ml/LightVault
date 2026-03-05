public class SecretVersion
{
    public int Id { get; set; }
    public Guid SecretId { get; set; }
    public int Version { get; set; }
    public byte[] Ciphertext { get; set; } = default!;
    public byte[] Nonce { get; set; } = default!;
    public byte[] Tag { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
