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
        return Convert.ToBase64String(inArray: guid.ToByteArray());
    }

    public string PasswordDigest(string password, string timestamp, string nonce)
    {
        SHA1 sha1 = SHA1Managed.Create();
        byte[] passwordHashedBytes = sha1.ComputeHash(buffer: Encoding.UTF8.GetBytes(s: password));
        byte[] timestampBytes = string.IsNullOrEmpty(value: timestamp)
            ? new byte[0]
            : Encoding.UTF8.GetBytes(s: timestamp);
        byte[] nonceBytes = string.IsNullOrEmpty(value: nonce)
            ? new byte[0]
            : Convert.FromBase64String(s: nonce);
        byte[] input = new byte[
            passwordHashedBytes.Length + timestampBytes.Length + nonceBytes.Length
        ];
        int offset = 0;
        nonceBytes.CopyTo(array: input, index: offset);
        offset += nonceBytes.Length;
        timestampBytes.CopyTo(array: input, index: offset);
        offset += timestampBytes.Length;
        passwordHashedBytes.CopyTo(array: input, index: offset);
        return Convert.ToBase64String(inArray: sha1.ComputeHash(buffer: input));
    }
}
