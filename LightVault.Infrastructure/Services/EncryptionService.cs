using LightVault.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace LightVault.Infrastructure.Services;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _masterKey;

    public EncryptionService(byte[] masterKey)
    {
        if (masterKey == null || masterKey.Length != 32)
            throw new ArgumentException("Master key must be 32 bytes for AES-256.");

        _masterKey = masterKey;
    }

    /// <summary>
    /// Derives a per-secret key using HMACSHA256.
    /// </summary>
    private byte[] DeriveKey(string context)
    {
        using var hmac = new HMACSHA256(_masterKey);
        var ctx = Encoding.UTF8.GetBytes(context ?? "");
        return hmac.ComputeHash(ctx);
    }

    /// <summary>
    /// Encrypts raw bytes using AES-GCM.
    /// </summary>
    public (byte[] Ciphertext, byte[] Nonce, byte[] Tag) Encrypt(byte[] plaintext, string context)
    {
        if (plaintext is null || plaintext.Length == 0)
            throw new ArgumentException("Plaintext cannot be null or empty.");

        var key = DeriveKey(context);

        var nonce = RandomNumberGenerator.GetBytes(12);

        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(key, tagSizeInBytes: 16);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        return (ciphertext, nonce, tag);
    }

    /// <summary>
    /// Decrypts raw bytes using AES-GCM.
    /// </summary>
    public byte[] Decrypt(byte[] ciphertext, byte[] nonce, byte[] tag, string context)
    {
        if (ciphertext == null || ciphertext.Length == 0)
            throw new ArgumentException("Ciphertext cannot be null or empty.");

        if (nonce == null || nonce.Length != 12)
            throw new ArgumentException("Nonce must be 12 bytes for AES-GCM.");

        if (tag == null || tag.Length != 16)
            throw new ArgumentException("Tag must be 16 bytes.");

        var key = DeriveKey(context);
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(key, tagSizeInBytes: 16);
        try
        {
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
        }
        catch (CryptographicException)
        {
            throw new CryptographicException("Decryption failed: ciphertext or tag is invalid (possible tampering).");
        }

        return plaintext;
    }

    /// <summary>
    /// Convenience function to encrypt strings.
    /// </summary>
    public (byte[] Ciphertext, byte[] Nonce, byte[] Tag) EncryptString(string plaintext, string context)
    {
        var bytes = Encoding.UTF8.GetBytes(plaintext);
        return Encrypt(bytes, context);
    }

    /// <summary>
    /// Convenience function to decrypt to string.
    /// </summary>
    public string DecryptToString(byte[] ciphertext, byte[] nonce, byte[] tag, string context)
    {
        var bytes = Decrypt(ciphertext, nonce, tag, context);
        return Encoding.UTF8.GetString(bytes);
    }
}
