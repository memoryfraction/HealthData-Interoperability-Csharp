using HealthDataInteropSharedLibrary.Shared;
using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HealthDataInteropSharedLibrary;

namespace HealthData.Interop.Tests.EncryptionTests;

/// <summary>
/// [EN] Unit tests for PhiEncryptionService AES-GCM encryption.
/// [CN] PhiEncryptionService AES-GCM 加密的单元测试。
/// </summary>
[TestClass]
public sealed class PhiEncryptionServiceTests
{
    private readonly byte[] _key = new byte[32];

    public PhiEncryptionServiceTests()
    {
        RandomNumberGenerator.Fill(_key);
    }

    [TestMethod]
    public void Constructor_WithValidKey_ShouldSucceed()
    {
        var service = new PhiEncryptionService(_key);
        service.Should().NotBeNull();
        service.Dispose();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WithNullKey_ShouldThrow()
    {
        _ = new PhiEncryptionService(null!);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_WithInvalidKeyLength_ShouldThrow()
    {
        var shortKey = new byte[16];
        _ = new PhiEncryptionService(shortKey);
    }

    [TestMethod]
    public void GenerateNonce_ShouldReturn12Bytes()
    {
        var nonce = PhiEncryptionService.GenerateNonce();
        nonce.Should().HaveCount(12, "Nonce should be 96 bits");
    }

    [TestMethod]
    public void Encrypt_Decrypt_RoundTrip_ShouldSucceed()
    {
        using var service = new PhiEncryptionService(_key);
        
        var plaintext = "Name: John Doe, DOB: 1990-01-15";
        var (ciphertext, nonce, tag) = service.Encrypt(plaintext);
        var decrypted = service.Decrypt(ciphertext, nonce, tag);

        decrypted.Should().Be(plaintext);
    }

    [TestMethod]
    public void Encrypt_Decrypt_EmptyString_ShouldSucceed()
    {
        using var service = new PhiEncryptionService(_key);
        
        var (ciphertext, nonce, tag) = service.Encrypt("");
        var decrypted = service.Decrypt(ciphertext, nonce, tag);

        decrypted.Should().Be("");
    }

    [TestMethod]
    public void Encrypt_Decrypt_LongText_ShouldSucceed()
    {
        using var service = new PhiEncryptionService(_key);
        
        var longPlaintext = new string('A', 10000);
        var (ciphertext, nonce, tag) = service.Encrypt(longPlaintext);
        var decrypted = service.Decrypt(ciphertext, nonce, tag);

        decrypted.Should().Be(longPlaintext);
    }

    [TestMethod]
    public void Encrypt_ShouldProduceDifferentCiphertextEachTime()
    {
        using var service = new PhiEncryptionService(_key);
        
        var plaintext = "Same input";
        var (cipher1, nonce1, tag1) = service.Encrypt(plaintext);
        var (cipher2, nonce2, tag2) = service.Encrypt(plaintext);

        // Nonces should differ due to random generation
        nonce1.Should().NotBeEquivalentTo(nonce2);
        
        // Ciphertexts should also differ due to different nonces
        cipher1.Should().NotBeEquivalentTo(cipher2);
    }

    [TestMethod]
    public void Decrypt_WithWrongKey_ShouldThrowAuthenticationException()
    {
        var key1 = new byte[32];
        var key2 = new byte[32];
        RandomNumberGenerator.Fill(key1);
        RandomNumberGenerator.Fill(key2);

        using (var service1 = new PhiEncryptionService(key1))
        {
            var plaintext = "Secret PHI data";
            var (ciphertext, nonce, tag) = service1.Encrypt(plaintext);

            using var service2 = new PhiEncryptionService(key2);
            
            // When using wrong key, authentication tag will not match
            Assert.ThrowsException<AuthenticationTagMismatchException>(() => 
                service2.Decrypt(ciphertext, nonce, tag));
        }
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Decrypt_WithInvalidNonceLength_ShouldThrow()
    {
        using var service = new PhiEncryptionService(_key);
        
        var invalidNonce = new byte[8]; // Wrong length (should be 12)
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];
        service.Decrypt(new byte[10], invalidNonce, tag);
    }

    [TestMethod]
    public void Dispose_ShouldNotThrowOnMultipleCalls()
    {
        var service = new PhiEncryptionService(_key);
        service.Dispose();
        
        try
        {
            service.Dispose();
        }
        catch (Exception)
        {
            Assert.Fail("Dispose should not throw on multiple calls");
        }
    }
}
