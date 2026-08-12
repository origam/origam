#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

namespace Origam.Server.Authorization;

public class Sha256PasswordHasher
{
    private const int SaltSize = 64; // 64 bytes
    private const int SubkeyLength = 32; // 256 bit
    private const int IterationCount = 600_000; // OWASP recommended minimum for PBKDF2-HMAC-SHA256.
    public const string KeyPrefix = "pbkdf2-sha256";
    public const int KeyPartsLength = 4;

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
            parts.Length != KeyPartsLength
            || parts[0] != KeyPrefix
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
        catch (Exception)
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
        byte[] subkey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            IterationCount,
            HashAlgorithmName.SHA256,
            SubkeyLength
        );
        return $"{KeyPrefix}.{IterationCount:X}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(subkey)}";
    }
}
