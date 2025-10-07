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

namespace Origam.Rule.XsltFunctions;

public class XsltCryptoFunctionContainer
{
    public string Nonce()
    {
        Guid guid = Guid.NewGuid();
        return Convert.ToBase64String(guid.ToByteArray());
    }

    public string PasswordDigest(string password, string timestamp, string nonce)
    {
        SHA1 sha1 = SHA1Managed.Create();
        byte[] passwordHashedBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
        byte[] timestampBytes = string.IsNullOrEmpty(timestamp)
            ? new byte[0]
            : Encoding.UTF8.GetBytes(timestamp);
        byte[] nonceBytes = string.IsNullOrEmpty(nonce)
            ? new byte[0]
            : Convert.FromBase64String(nonce);
        byte[] input = new byte[
            passwordHashedBytes.Length + timestampBytes.Length + nonceBytes.Length
        ];
        int offset = 0;
        nonceBytes.CopyTo(input, offset);
        offset += nonceBytes.Length;
        timestampBytes.CopyTo(input, offset);
        offset += timestampBytes.Length;
        passwordHashedBytes.CopyTo(input, offset);
        return Convert.ToBase64String(sha1.ComputeHash(input));
    }
}
