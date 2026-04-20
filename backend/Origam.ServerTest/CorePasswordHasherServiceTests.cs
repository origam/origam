using IdentityServer4.Test;
using Origam.Server;
using Origam.Server.Authorization;

namespace Origam.ServerTest;

public class CorePasswordHasherServiceTests
{
    private CorePasswordHasherService hasher;
    private User user;

    [SetUp]
    public void Setup()
    {
        hasher = new CorePasswordHasherService();
        user = new User("test");
    }

    [Test]
    public void VerifyHashedPassword_Different_Hashes()
    {
        var h1 = hasher.HashPassword(user, "heslo");
        var h2 = hasher.HashPassword(user, "heslo");

        Assert.That(h2, Is.Not.EqualTo(h1));
    }

    [Test]
    public void VerifyHashedPassword_Success()
    {
        var result = hasher.VerifyHashedPassword(
            user,
            "pbkdf2-sha256.3D090.NRpgvx7jF0OLKfVAsIi+XA==.GFjXdcxdDj2g01G+/Zfv+DP2NhFiCs+9/xYcUuT58Mk=",
            "heslo"
        );
        Assert.That(
            result,
            Is.EqualTo(Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success)
        );
    }

    [Test]
    public void VerifyHashedPassword_InvalidHeader_Fail()
    {
        var result = hasher.VerifyHashedPassword(
            user,
            "3E8000.AFxWVXqnF+VIYlSBAy1rgMeZGb85ieNlj0soUWOvYwx2E3ykekDnrKoHb50snBN4YQ==",
            "heslo"
        );
        Assert.That(
            result,
            Is.EqualTo(Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
        );
    }
}
