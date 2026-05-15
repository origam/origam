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

using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Origam.Server;

// TEMPORARY — verifying that the CodeQL pipeline detects known-bad patterns.
// DO NOT MERGE. Revert before closing the smoke-test PR.
[ApiController]
[Route("api/canary")]
public class CodeqlCanaryController : ControllerBase
{
    // Expected CodeQL alert: cs/hardcoded-credentials
    private const string AdminPassword = "P@ssw0rd123!";

    // Expected CodeQL alert: cs/hardcoded-credentials
    private const string ApiKey = "canary-fake-token-do-not-use-in-real-code";

    // Expected CodeQL alert: cs/sql-injection (High)
    [HttpGet("user/{name}")]
    public IActionResult GetUser(string name)
    {
        using var conn = new SqlConnection("Server=.;Database=test;");
        var cmd = new SqlCommand(
            "SELECT * FROM Users WHERE Name = '" + name + "'",
            conn
        );
        return Ok(cmd.CommandText);
    }

    // Expected CodeQL alert: cs/path-injection (High)
    [HttpGet("file")]
    public IActionResult ReadFile(string path)
    {
        var content = File.ReadAllText(path);
        return Ok(content);
    }

    // Expected CodeQL alert: cs/weak-cryptographic-algorithm (MD5)
    [HttpGet("hash")]
    public IActionResult HashPassword(string input)
    {
        using var md5 = MD5.Create();
        var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input + AdminPassword));
        return Ok(System.Convert.ToBase64String(bytes));
    }

    // Keeps ApiKey from being optimized away.
    [HttpGet("apikey-length")]
    public IActionResult GetApiKeyLength() => Ok(ApiKey.Length);
}
