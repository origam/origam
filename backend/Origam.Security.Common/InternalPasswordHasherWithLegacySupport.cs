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
using System.Security.Cryptography;
using System.Text;
using BrockAllen.IdentityReboot;

namespace Origam.Security.Common;

public class InternalPasswordHasherWithLegacySupport : AdaptivePasswordHasher
{
    public override VerificationResult VerifyHashedPassword(
        string hashedPassword,
        string providedPassword
    )
    {
        if (String.IsNullOrWhiteSpace(value: hashedPassword))
        {
            return VerificationResult.Failed;
        }
        string[] passwordProperties = hashedPassword.Split(separator: '|');
        if (passwordProperties.Length != 2)
        {
            // use AdaptiveHasher
            return base.VerifyHashedPassword(
                hashedPassword: hashedPassword,
                providedPassword: providedPassword
            );
        }
        // migrated account from NetMembership
        // format hashedFormat|salt
        string passwordHash = passwordProperties[0];
        string salt = passwordProperties[1];

        if (
            String.Equals(
                a: EncryptPassword(pass: providedPassword, salt: salt),
                b: passwordHash,
                comparisonType: StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return VerificationResult.SuccessRehashNeeded;
        }

        return VerificationResult.Failed;
    }

    private string EncryptPassword(string pass, string salt)
    {
        byte[] bIn = Encoding.Unicode.GetBytes(s: pass);
        byte[] bSalt = Convert.FromBase64String(s: salt);
        byte[] bRet = null;
        HashAlgorithm hm = HashAlgorithm.Create(hashName: "SHA1");
        if (hm is KeyedHashAlgorithm)
        {
            KeyedHashAlgorithm kha = (KeyedHashAlgorithm)hm;
            if (kha.Key.Length == bSalt.Length)
            {
                kha.Key = bSalt;
            }
            else if (kha.Key.Length < bSalt.Length)
            {
                byte[] bKey = new byte[kha.Key.Length];
                Buffer.BlockCopy(
                    src: bSalt,
                    srcOffset: 0,
                    dst: bKey,
                    dstOffset: 0,
                    count: bKey.Length
                );
                kha.Key = bKey;
            }
            else
            {
                byte[] bKey = new byte[kha.Key.Length];
                for (int iter = 0; iter < bKey.Length; )
                {
                    int len = Math.Min(val1: bSalt.Length, val2: bKey.Length - iter);
                    Buffer.BlockCopy(
                        src: bSalt,
                        srcOffset: 0,
                        dst: bKey,
                        dstOffset: iter,
                        count: len
                    );
                    iter += len;
                }
                kha.Key = bKey;
            }
            bRet = kha.ComputeHash(buffer: bIn);
        }
        else
        {
            byte[] bAll = new byte[bSalt.Length + bIn.Length];
            Buffer.BlockCopy(
                src: bSalt,
                srcOffset: 0,
                dst: bAll,
                dstOffset: 0,
                count: bSalt.Length
            );
            Buffer.BlockCopy(
                src: bIn,
                srcOffset: 0,
                dst: bAll,
                dstOffset: bSalt.Length,
                count: bIn.Length
            );
            bRet = hm.ComputeHash(buffer: bAll);
        }
        return Convert.ToBase64String(inArray: bRet);
    }
}
