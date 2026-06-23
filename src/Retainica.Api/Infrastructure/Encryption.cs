using System.Security.Cryptography;
using System.Text;

namespace Retainica.Api.Infrastructure;

public interface IEncryptionService
{
    /// <summary>Encrypts plaintext, returning base64(nonce || ciphertext || tag).</summary>
    string Encrypt(string plaintext);

    /// <summary>Reverses <see cref="Encrypt"/>. Throws if the payload is tampered or the key changed.</summary>
    string Decrypt(string payload);
}

/// <summary>
/// AES-256-GCM at-rest encryption for user API keys. The 32-byte key is derived
/// (SHA-256) from the configured <c>Encryption:Key</c> secret so any sufficiently
/// long secret string works, matching the existing <c>Jwt:Key</c> convention.
/// Stored layout: base64( nonce[12] || ciphertext[n] || tag[16] ).
/// </summary>
public sealed class AesGcmEncryptionService : IEncryptionService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private readonly byte[] _key;

    public AesGcmEncryptionService(IConfiguration config)
    {
        var secret = config["Encryption:Key"]
            ?? throw new InvalidOperationException("Encryption:Key not configured");
        if (secret.Length < 16)
            throw new InvalidOperationException("Encryption:Key must be at least 16 characters");
        _key = SHA256.HashData(Encoding.UTF8.GetBytes(secret)); // 32 bytes
    }

    public string Encrypt(string plaintext)
    {
        var plain = Encoding.UTF8.GetBytes(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipher = new byte[plain.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plain, cipher, tag);

        var payload = new byte[NonceSize + cipher.Length + TagSize];
        Buffer.BlockCopy(nonce, 0, payload, 0, NonceSize);
        Buffer.BlockCopy(cipher, 0, payload, NonceSize, cipher.Length);
        Buffer.BlockCopy(tag, 0, payload, NonceSize + cipher.Length, TagSize);
        return Convert.ToBase64String(payload);
    }

    public string Decrypt(string payload)
    {
        var bytes = Convert.FromBase64String(payload);
        if (bytes.Length < NonceSize + TagSize)
            throw new CryptographicException("Encrypted payload is malformed");

        var cipherLength = bytes.Length - NonceSize - TagSize;
        var nonce = bytes.AsSpan(0, NonceSize);
        var cipher = bytes.AsSpan(NonceSize, cipherLength);
        var tag = bytes.AsSpan(NonceSize + cipherLength, TagSize);
        var plain = new byte[cipherLength];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, cipher, tag, plain);
        return Encoding.UTF8.GetString(plain);
    }
}
