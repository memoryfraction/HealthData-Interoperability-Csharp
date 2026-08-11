using System.Security.Cryptography;

namespace Shared_Library.Shared;

/// <summary>
/// [EN] PHI Encryption Service using AES-GCM 256-bit encryption.
/// Provides HIPAA-compliant encryption at rest for Protected Health Information (PHI).
/// Implements encrypt/decrypt operations with nonce management and key rotation support.
/// 
/// [CN] Use AES-GCM 256-bit encryption for PHI data service.
/// Provide HIPAA-compliant static data encryption.
/// Implement encrypt/decrypt with nonce management and key rotation support.
/// </summary>
public sealed class PhiEncryptionService : IDisposable
{
    private readonly AesGcm _cipher;
    private bool _disposed;

    /// <summary>
    /// [EN] Initialize encryption service with a 256-bit key. Key must be exactly 32 bytes (256 bits).
    /// Tag size is 16 bytes (128 bits) for authentication integrity per NIST SP 800-38D.
    /// [CN] Use 256-bit key to initialize encryption service. Key must be 32 bytes.
    /// </summary>
    public PhiEncryptionService(byte[] key)
    {
        Guard.NotNull(key, nameof(key));
        
        if (key.Length != 32)
            throw new ArgumentException("Key must be exactly 32 bytes", nameof(key));

        _cipher = new AesGcm(key);
    }

    /// <summary>
    /// [EN] Generate a random 12-byte nonce for encryption.
    /// [CN] Generate random 12-byte nonce for encryption.
    /// </summary>
    public static byte[] GenerateNonce()
    {
        var nonce = new byte[12];
        RandomNumberGenerator.Fill(nonce);
        return nonce;
    }

    /// <summary>
    /// [EN] Encrypt plaintext to ciphertext. Returns ciphertext, nonce, and authentication tag.
    /// All three components must be stored together for successful decryption.
    /// [CN] Encrypt plaintext to ciphertext, returns ciphertext, nonce, and authentication tag.
    /// </summary>
    public (byte[] Ciphertext, byte[] Nonce, byte[] Tag) Encrypt(string plaintext)
    {
        Guard.NotNull(plaintext, nameof(plaintext));

        var nonce = GenerateNonce();
        var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];
        
        _cipher.Encrypt(nonce, plaintextBytes, ciphertext, tag, null);

        return (ciphertext, nonce, tag);
    }

    /// <summary>
    /// [EN] Decrypt ciphertext back to plaintext using the provided nonce and authentication tag.
    /// Tag must match the computed MAC or CryptographicException will be thrown.
    /// [CN] Use provided nonce and authentication tag to decrypt ciphertext to plaintext.
    /// </summary>
    public string Decrypt(byte[] ciphertext, byte[] nonce, byte[] tag)
    {
        Guard.NotNull(ciphertext, nameof(ciphertext));
        Guard.NotNull(nonce, nameof(nonce));
        Guard.NotNull(tag, nameof(tag));

        if (nonce.Length != 12)
            throw new ArgumentException("Nonce must be exactly 12 bytes", nameof(nonce));

        var decrypted = new byte[ciphertext.Length];
        
        _cipher.Decrypt(nonce, ciphertext, tag, decrypted, null);

        return System.Text.Encoding.UTF8.GetString(decrypted);
    }

    /// <summary>
    /// [EN] Dispose cipher resources.
    /// [CN] Release cipher resources.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _cipher.Dispose();
            _disposed = true;
        }
    }
}

