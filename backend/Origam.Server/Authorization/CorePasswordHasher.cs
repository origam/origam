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
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using BrockAllen.IdentityReboot;
using Microsoft.AspNetCore.Identity;
using Origam.Security.Common;

namespace Origam.Server;

class CorePasswordHasher : IPasswordHasher<IOrigamUser>
{
    private readonly InternalPasswordHasherWithLegacySupport internalHasher =
        new InternalPasswordHasherWithLegacySupport();

    public string HashPassword(IOrigamUser user, string password)
    {
        return internalHasher.HashPassword(password);
    }

    public PasswordVerificationResult VerifyHashedPassword(
        IOrigamUser user,
        string hashedPassword,
        string providedPassword
    )
    {
        //VerificationResult verificationResult = internalHasher.VerifyHashedPassword(
        //    hashedPassword,
        //    providedPassword
        //);
        var parts = hashedPassword.Split(".");
        int count;
        Int32.TryParse(parts[0], NumberStyles.HexNumber, null, out count);
        hashedPassword = hashedPassword = parts[1];
        byte[] hashedPasswordBytes = Convert.FromBase64String(hashedPassword);
        var SALT_SIZE = 128 / 8;
        var PBKDF2_SUBKEY_LENGTH = 256 / 8;
        byte[] salt = new byte[SALT_SIZE];
        Buffer.BlockCopy(hashedPasswordBytes, 1, salt, 0, SALT_SIZE);
        byte[] storedSubkey = new byte[PBKDF2_SUBKEY_LENGTH];
        Buffer.BlockCopy(hashedPasswordBytes, 1 + SALT_SIZE, storedSubkey, 0, PBKDF2_SUBKEY_LENGTH);
        byte[] generatedSubkey;

        generatedSubkey = Rfc2898DeriveBytes.Pbkdf2(
            providedPassword,
            salt,
            count,
            HashAlgorithmName.SHA256,
            PBKDF2_SUBKEY_LENGTH
        );
        var verificationResult =
            (ByteArraysEqual(storedSubkey, generatedSubkey))
                ? VerificationResult.Success
                : VerificationResult.Failed;
        return ToAspNetCoreResult(verificationResult);
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

    private static PasswordVerificationResult ToAspNetCoreResult(VerificationResult result)
    {
        switch (result)
        {
            case VerificationResult.Failed:
                return PasswordVerificationResult.Failed;
            case VerificationResult.Success:
                return PasswordVerificationResult.Success;
            case VerificationResult.SuccessRehashNeeded:
                return PasswordVerificationResult.SuccessRehashNeeded;
            default:
                throw new NotImplementedException();
        }
    }
}
