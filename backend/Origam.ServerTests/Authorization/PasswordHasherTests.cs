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

using System.Globalization;
using System.Security.Cryptography;
using BrockAllen.IdentityReboot;
using Microsoft.AspNetCore.Identity;
using NUnit.Framework;
using Origam.Server.Authorization;

namespace Origam.ServerTests.Authorization;

[TestFixture]
public class PasswordHasherTests
{
    private const string Password = "TestPassword123!";

    [Test]
    public void ShouldVerifyCurrentHash()
    {
        var sut = new Sha256PasswordHasher();
        string hash = sut.HashPassword(Password);

        PasswordVerificationResult result = sut.VerifyHashedPassword(hash, Password);

        Assert.That(result, Is.EqualTo(PasswordVerificationResult.Success));
    }

    [Test]
    public void ShouldFailCurrentHashForWrongPassword()
    {
        var sut = new Sha256PasswordHasher();
        string hash = sut.HashPassword(Password);

        PasswordVerificationResult result = sut.VerifyHashedPassword(hash, "WrongPassword");

        Assert.That(result, Is.EqualTo(PasswordVerificationResult.Failed));
    }

    [TestCase("")]
    [TestCase("pbkdf2-sha256")]
    [TestCase("pbkdf2-sha256.not-hex.salt.hash")]
    [TestCase("pbkdf2-sha256.0.salt.hash")]
    [TestCase("pbkdf2-sha256.927C0.not-base64.hash")]
    [TestCase("different-prefix.927C0.salt.hash")]
    public void ShouldFailMalformedCurrentHashWithoutThrowing(string hash)
    {
        var sut = new Sha256PasswordHasher();

        PasswordVerificationResult result = sut.VerifyHashedPassword(hash, Password);

        Assert.That(result, Is.EqualTo(PasswordVerificationResult.Failed));
    }

    [Test]
    public void ShouldFailCurrentHashAboveAllowedIterationCountWithoutDerivingKey()
    {
        var sut = new Sha256PasswordHasher();
        int tooManyIterations = GetCurrentIterationCount(sut) + 1;
        string hash = $"{Sha256PasswordHasher.KEY_PREFIX}.{tooManyIterations:X}.salt.hash";

        PasswordVerificationResult result = sut.VerifyHashedPassword(hash, Password);

        Assert.That(result, Is.EqualTo(PasswordVerificationResult.Failed));
    }

    [Test]
    public void ShouldRequestRehashForLowerCurrentHashIterationCount()
    {
        var sut = new Sha256PasswordHasher();
        string hash = CreateCurrentHash(Password, iterations: 1000, saltLength: 64);

        PasswordVerificationResult result = sut.VerifyHashedPassword(hash, Password);

        Assert.That(result, Is.EqualTo(PasswordVerificationResult.SuccessRehashNeeded));
    }

    [Test]
    public void ShouldRequestRehashForValidCurrentHashWithDifferentSaltSize()
    {
        var sut = new Sha256PasswordHasher();
        string hash = CreateCurrentHash(
            Password,
            iterations: GetCurrentIterationCount(sut),
            saltLength: 16
        );

        PasswordVerificationResult result = sut.VerifyHashedPassword(hash, Password);

        Assert.That(result, Is.EqualTo(PasswordVerificationResult.SuccessRehashNeeded));
    }

    [Test]
    public void ShouldRequestRehashForLegacyAdaptiveHash()
    {
        var legacyHasher = new AdaptivePasswordHasher();
        var sut = new CorePasswordHasher();
        string legacyHash = legacyHasher.HashPassword(Password);

        PasswordVerificationResult result = sut.VerifyHashedPassword(
            user: null!,
            legacyHash,
            Password
        );

        Assert.That(result, Is.EqualTo(PasswordVerificationResult.SuccessRehashNeeded));
    }

    private static string CreateCurrentHash(string password, int iterations, int saltLength)
    {
        byte[] salt = new byte[saltLength];
        RandomNumberGenerator.Fill(salt);
        byte[] subkey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            outputLength: 32
        );
        return string.Join(
            ".",
            Sha256PasswordHasher.KEY_PREFIX,
            iterations.ToString("X", CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(subkey)
        );
    }

    private static int GetCurrentIterationCount(Sha256PasswordHasher hasher)
    {
        string hash = hasher.HashPassword(Password);
        string iterationPart = hash.Split(".")[1];
        return int.Parse(iterationPart, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
    }
}
