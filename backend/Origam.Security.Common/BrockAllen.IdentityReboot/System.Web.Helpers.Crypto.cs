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

/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

// Original Version Copyright:
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Original License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace BrockAllen.IdentityReboot.Internal;

internal static class Crypto
{
    private const int PBKDF2_ITERATION_COUNT = 1000; // default for Rfc2898DeriveBytes
    private const int PBKDF2_SUBKEY_LENGTH = 256 / 8; // 256 bits
    private const int SALT_SIZE = 128 / 8; // 128 bits

    /* =======================
     * HASHED PASSWORD FORMATS
     * =======================
     *
     * Version 0:
     * PBKDF2 with HMAC-SHA1, 128-bit salt, 256-bit subkey, 1000 iterations.
     * (See also: SDL crypto guidelines v5.1, Part III)
     * Format: { 0x00, salt, subkey }
     */
    public static string HashPassword(string password, int iterationCount = PBKDF2_ITERATION_COUNT)
    {
        if (password == null)
        {
            throw new ArgumentNullException("password");
        }
        // Produce a version 0 (see comment above) password hash.
        byte[] salt = RandomNumberGenerator.GetBytes(SALT_SIZE);
        byte[] subkey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterationCount,
            HashAlgorithmName.SHA1,
            PBKDF2_SUBKEY_LENGTH
        );
        byte[] outputBytes = new byte[1 + SALT_SIZE + PBKDF2_SUBKEY_LENGTH];
        Buffer.BlockCopy(salt, 0, outputBytes, 1, SALT_SIZE);
        Buffer.BlockCopy(subkey, 0, outputBytes, 1 + SALT_SIZE, PBKDF2_SUBKEY_LENGTH);
        return Convert.ToBase64String(outputBytes);
    }

    // hashedPassword must be of the format of HashWithPassword (salt + Hash(salt+input)
    public static bool VerifyHashedPassword(
        string hashedPassword,
        string password,
        int iterationCount = PBKDF2_ITERATION_COUNT
    )
    {
        if (hashedPassword == null)
        {
            throw new ArgumentNullException("hashedPassword");
        }
        if (password == null)
        {
            throw new ArgumentNullException("password");
        }
        byte[] hashedPasswordBytes = Convert.FromBase64String(hashedPassword);
        // Verify a version 0 (see comment above) password hash.
        if (
            (hashedPasswordBytes.Length != (1 + SALT_SIZE + PBKDF2_SUBKEY_LENGTH))
            || (hashedPasswordBytes[0] != 0x00)
        )
        {
            // Wrong length or version header.
            return false;
        }
        byte[] salt = new byte[SALT_SIZE];
        Buffer.BlockCopy(hashedPasswordBytes, 1, salt, 0, SALT_SIZE);
        byte[] storedSubkey = new byte[PBKDF2_SUBKEY_LENGTH];
        Buffer.BlockCopy(hashedPasswordBytes, 1 + SALT_SIZE, storedSubkey, 0, PBKDF2_SUBKEY_LENGTH);
        byte[] generatedSubkey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterationCount,
            HashAlgorithmName.SHA1,
            PBKDF2_SUBKEY_LENGTH
        );
        return ByteArraysEqual(storedSubkey, generatedSubkey);
    }

    // Compares two byte arrays for equality.
    // The method is specifically written so that the loop is not optimized.
    [MethodImpl(MethodImplOptions.NoOptimization)]
    private static bool ByteArraysEqual(byte[] a, byte[] b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }
        if ((a == null) || (b == null) || (a.Length != b.Length))
        {
            return false;
        }
        bool areSame = true;
        for (int i = 0; i < a.Length; i++)
        {
            areSame &= (a[i] == b[i]);
        }
        return areSame;
    }
}
