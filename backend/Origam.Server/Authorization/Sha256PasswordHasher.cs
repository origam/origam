using System;
using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;

namespace Origam.Server.Authorization;

public class Sha256PasswordHasher
{
    private const int SaltSize = 64; // 64 bytes
    private const int SubkeyLength = 32; // 256 bit
    private const int IterationCount = 600000; // Increased based on OWASP recommendation - https://en.wikipedia.org/wiki/PBKDF2
    public const string KEY_PREFIX = "pbkdf2-sha256";
    public const int KEY_PARTS_LENGTH = 4;

    public PasswordVerificationResult VerifyHashedPassword(
        string hashedPassword,
        string providedPassword
    )
    {
        if (string.IsNullOrEmpty(hashedPassword))
        {
            return PasswordVerificationResult.Failed;
        }
        string[] parts = hashedPassword.Split(".");
        if (
            parts.Length != KEY_PARTS_LENGTH
            || parts[0] != KEY_PREFIX
            || !int.TryParse(parts[1], NumberStyles.HexNumber, provider: null, out int iterations)
            || iterations <= 0
            || iterations > IterationCount
        )
        {
            return PasswordVerificationResult.Failed;
        }
        string saltString = parts[2];
        hashedPassword = parts[3];
        byte[] saltBytes;
        byte[] hashedPasswordBytes;
        try
        {
            saltBytes = Convert.FromBase64String(saltString);
            hashedPasswordBytes = Convert.FromBase64String(hashedPassword);
        }
        catch
        {
            return PasswordVerificationResult.Failed;
        }
        byte[] generatedSubkey = Rfc2898DeriveBytes.Pbkdf2(
            providedPassword,
            saltBytes,
            iterations,
            HashAlgorithmName.SHA256,
            SubkeyLength
        );
        bool success = CryptographicOperations.FixedTimeEquals(
            hashedPasswordBytes,
            generatedSubkey
        );
        if (success && iterations != IterationCount)
        {
            return PasswordVerificationResult.SuccessRehashNeeded;
        }
        if (success && saltBytes.Length != SaltSize)
        {
            return PasswordVerificationResult.SuccessRehashNeeded;
        }
        return success ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
    }

    public string HashPassword(string password)
    {
        if (password == null)
        {
            throw new ArgumentNullException(nameof(password));
        }
        byte[] salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);
        byte[] subkey = KeyDerivation.Pbkdf2(
            password,
            salt,
            KeyDerivationPrf.HMACSHA256,
            IterationCount,
            SubkeyLength
        );
        // [prefix].[iteration_count in hexdecimal].[salt in base64].[subkey in base64]
        return $"{KEY_PREFIX}.{IterationCount:X}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(subkey)}";
    }
}
