#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

using System.Security.Cryptography;
using System.Text;

namespace Origam.Server;

// TEMPORARY — verifying that the CodeQL pipeline detects known-bad patterns.
// DO NOT MERGE. Revert this file before closing the smoke-test PR.
public static class CodeqlSmokeTest
{
    // Expected CodeQL alert: cs/weak-cryptographic-algorithm (MD5)
    public static string HashPassword(string input)
    {
        using var md5 = MD5.Create();
        var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return System.Convert.ToBase64String(bytes);
    }
}
