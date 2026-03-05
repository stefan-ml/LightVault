namespace LightVault.Domain.Interfaces;

public interface IEncryptionService
{
    (byte[] Ciphertext, byte[] Nonce, byte[] Tag) Encrypt(byte[] plaintext, string context);
    byte[] Decrypt(byte[] ciphertext, byte[] nonce, byte[] tag, string context);

    // convenience overloads for strings:
    (byte[] Ciphertext, byte[] Nonce, byte[] Tag) EncryptString(string plaintext, string context);
    string DecryptToString(byte[] ciphertext, byte[] nonce, byte[] tag, string context);
}
