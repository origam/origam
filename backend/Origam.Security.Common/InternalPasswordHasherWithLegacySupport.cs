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
        if (String.IsNullOrWhiteSpace(hashedPassword))
        {
            return VerificationResult.Failed;
        }
        string[] passwordProperties = hashedPassword.Split('|');
        if (passwordProperties.Length != 2)
        {
            // use AdaptiveHasher
            return base.VerifyHashedPassword(hashedPassword, providedPassword);
        }
        // migrated account from NetMembership
        // format hashedFormat|salt
        string passwordHash = passwordProperties[0];
        string salt = passwordProperties[1];

        if (
            String.Equals(
                EncryptPassword(providedPassword, salt),
                passwordHash,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return VerificationResult.SuccessRehashNeeded;
        }

        return VerificationResult.Failed;
    }

    private string EncryptPassword(string pass, string salt)
    {
        byte[] bIn = Encoding.Unicode.GetBytes(pass);
        byte[] bSalt = Convert.FromBase64String(salt);
        byte[] bAll = new byte[bSalt.Length + bIn.Length];
        Buffer.BlockCopy(bSalt, 0, bAll, 0, bSalt.Length);
        Buffer.BlockCopy(bIn, 0, bAll, bSalt.Length, bIn.Length);
        byte[] bRet = SHA1.HashData(bAll);
        return Convert.ToBase64String(bRet);
    }
}
