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
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using OpenIddict.Server;
using Origam.Server.Configuration;

namespace Origam.ServerTests;

public class OpenIddictCertificateConfigurationTests
{
    private string testDirectory;
    private readonly List<X509Certificate2> certificates = new();
    private TokenCertificateStartupInfo startupInfo;

    [SetUp]
    public void SetUp()
    {
        testDirectory = Path.GetFullPath(
            Path.Combine(Path.GetTempPath(), $"origam-token-certificates-{Guid.NewGuid():N}")
        );
        Directory.CreateDirectory(testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var certificate in certificates)
        {
            certificate.Dispose();
        }
        certificates.Clear();
        Directory.Delete(testDirectory, recursive: true);
    }

    [TestCase(null, null)]
    [TestCase("signing-password", "encryption-password")]
    public void GeneratedCertificatesArePrivateAndStillProtectTokensAfterRestart(
        string signingPassword,
        string encryptionPassword
    )
    {
        var settings = DirectorySettings();
        settings["SigningCertificatePassword"] = signingPassword;
        settings["EncryptionCertificatePassword"] = encryptionPassword;
        var first = Configure(settings);
        Assert.That(startupInfo.Level, Is.EqualTo(LogLevel.Warning));
        Assert.That(startupInfo.Message, Does.Contain("Generated new self-signed fallback"));
        Assert.That(
            startupInfo.Message,
            Does.Not.Contain("signing-password").And.Not.Contain("encryption-password")
        );
        var signing = SigningCertificate(first);
        var encryption = EncryptionCertificate(first);
        Assert.That(signing.Thumbprint, Is.Not.EqualTo(encryption.Thumbprint));
        Assert.That(
            signing.Extensions.OfType<X509KeyUsageExtension>().Single().KeyUsages,
            Is.EqualTo(X509KeyUsageFlags.DigitalSignature)
        );
        Assert.That(
            encryption.Extensions.OfType<X509KeyUsageExtension>().Single().KeyUsages,
            Is.EqualTo(X509KeyUsageFlags.KeyEncipherment)
        );
        using var signingPublicKey = signing.GetRSAPublicKey();
        Assert.That(signingPublicKey!.KeySize, Is.EqualTo(4096));
        Assert.That(
            encryption.NotAfter.ToUniversalTime(),
            Is.GreaterThan(DateTime.UtcNow.AddYears(1))
        );
        byte[] payload = RandomNumberGenerator.GetBytes(32);
        using var signingKey = signing.GetRSAPrivateKey();
        using var encryptionKey = encryption.GetRSAPublicKey();
        byte[] signature = signingKey!.SignData(
            payload,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );
        byte[] encrypted = encryptionKey!.Encrypt(payload, RSAEncryptionPadding.OaepSHA256);
        string signingPath = Path.Combine(testDirectory, "keys", "signing.pfx");
        string encryptionPath = Path.Combine(testDirectory, "keys", "encryption.pfx");
        byte[] originalFile = File.ReadAllBytes(signingPath);
        var restarted = Configure(settings);
        Assert.That(startupInfo.Level, Is.EqualTo(LogLevel.Warning));
        Assert.That(
            startupInfo.Message,
            Does.Contain("Using existing").And.Contain(signingPath).And.Contain(encryptionPath)
        );
        Assert.That(SigningCertificate(restarted).Thumbprint, Is.EqualTo(signing.Thumbprint));
        Assert.That(File.ReadAllBytes(signingPath), Is.EqualTo(originalFile));
        using var verifyKey = SigningCertificate(restarted).GetRSAPublicKey();
        using var decryptKey = EncryptionCertificate(restarted).GetRSAPrivateKey();
        Assert.That(
            verifyKey!.VerifyData(
                payload,
                signature,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1
            ),
            Is.True
        );
        Assert.That(
            decryptKey!.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA256),
            Is.EqualTo(payload)
        );
        foreach (string path in new[] { signingPath, encryptionPath })
        {
            if (OperatingSystem.IsWindows())
            {
                Assert.That(new FileInfo(path).GetAccessControl().AreAccessRulesProtected, Is.True);
            }
            else
            {
                Assert.That(
                    File.GetUnixFileMode(path),
                    Is.EqualTo(UnixFileMode.UserRead | UnixFileMode.UserWrite)
                );
            }
        }
    }

    [Test]
    public void DirectoryReusesSuppliedPasswordProtectedCertificates()
    {
        string directory = Path.Combine(testDirectory, "keys");
        Directory.CreateDirectory(directory);
        using var signing = CreateCertificate(X509KeyUsageFlags.DigitalSignature);
        using var encryption = CreateCertificate(X509KeyUsageFlags.KeyEncipherment);
        string signingPath = Path.Combine(directory, "signing.pfx");
        byte[] original = signing.Export(X509ContentType.Pfx, "signing-password");
        File.WriteAllBytes(signingPath, original);
        File.WriteAllBytes(
            Path.Combine(directory, "encryption.pfx"),
            encryption.Export(X509ContentType.Pfx, "encryption-password")
        );
        var settings = DirectorySettings();
        settings["SigningCertificatePassword"] = "signing-password";
        settings["EncryptionCertificatePassword"] = "encryption-password";
        var options = Configure(settings);
        Assert.That(SigningCertificate(options).Thumbprint, Is.EqualTo(signing.Thumbprint));
        Assert.That(EncryptionCertificate(options).Thumbprint, Is.EqualTo(encryption.Thumbprint));
        Assert.That(File.ReadAllBytes(signingPath), Is.EqualTo(original));
    }

    [Test]
    public void ExplicitAzureStylePathsOverrideGenerationAndResolveRelativeToContentRoot()
    {
        var settings = FileSettings();
        using var signing = CreateCertificate(X509KeyUsageFlags.DigitalSignature);
        using var encryption = CreateCertificate(X509KeyUsageFlags.KeyEncipherment);
        File.WriteAllBytes(
            Path.Combine(testDirectory, "signing.p12"),
            signing.Export(X509ContentType.Pfx)
        );
        File.WriteAllBytes(
            Path.Combine(testDirectory, "encryption.p12"),
            encryption.Export(X509ContentType.Pfx)
        );
        var options = Configure(settings);
        Assert.That(SigningCertificate(options).Thumbprint, Is.EqualTo(signing.Thumbprint));
        Assert.That(EncryptionCertificate(options).Thumbprint, Is.EqualTo(encryption.Thumbprint));
        Assert.That(Directory.Exists(Path.Combine(testDirectory, "keys")), Is.False);
        Assert.That(startupInfo.Level, Is.EqualTo(LogLevel.Information));
        Assert.That(
            startupInfo.Message,
            Does.Contain("configured PFX files")
                .And.Contain(Path.Combine(testDirectory, "signing.p12"))
                .And.Contain(signing.Thumbprint)
                .And.Contain(encryption.Thumbprint)
        );
    }

    [TestCase("missing")]
    [TestCase("corrupt")]
    [TestCase("password")]
    [TestCase("public-only")]
    [TestCase("wrong-usage")]
    public void InvalidExplicitCertificatesNeverFallBackToGeneration(string failure)
    {
        var settings = FileSettings();
        using var signing = CreateCertificate(
            failure == "wrong-usage"
                ? X509KeyUsageFlags.KeyEncipherment
                : X509KeyUsageFlags.DigitalSignature
        );
        using var encryption = CreateCertificate(X509KeyUsageFlags.KeyEncipherment);
        string signingPath = Path.Combine(testDirectory, "signing.p12");
        if (failure == "corrupt")
        {
            File.WriteAllText(signingPath, "not a certificate");
        }
        else if (failure == "public-only")
        {
            File.WriteAllBytes(signingPath, signing.Export(X509ContentType.Cert));
        }
        else if (failure != "missing")
        {
            File.WriteAllBytes(signingPath, signing.Export(X509ContentType.Pfx, "correct"));
            settings["SigningCertificatePassword"] = failure == "password" ? "wrong" : "correct";
        }
        File.WriteAllBytes(
            Path.Combine(testDirectory, "encryption.p12"),
            encryption.Export(X509ContentType.Pfx)
        );
        Assert.Catch(() => Configure(settings));
        Assert.That(Directory.Exists(Path.Combine(testDirectory, "keys")), Is.False);
    }

    [TestCase("SigningCertificatePath")]
    [TestCase("EncryptionCertificatePath")]
    [TestCase("SigningCertificateThumbprint")]
    [TestCase("EncryptionCertificateThumbprint")]
    public void IncompleteExplicitSourceFailsEvenInDevelopment(string setting)
    {
        var settings = DirectorySettings();
        settings[setting] = "configured";
        Assert.Throws<InvalidOperationException>(() =>
            Configure(settings, Environments.Development)
        );
        Assert.That(Directory.Exists(Path.Combine(testDirectory, "keys")), Is.False);
    }

    [Test]
    public void MixedFileAndStoreConfigurationIsRejected()
    {
        var settings = FileSettings();
        settings["SigningCertificateThumbprint"] = "configured";
        Assert.Throws<InvalidOperationException>(() => Configure(settings));
        Assert.That(Directory.Exists(Path.Combine(testDirectory, "keys")), Is.False);
    }

    [TestCase("signing.pfx")]
    [TestCase("encryption.pfx")]
    public void PartialDirectoryIsNeverRepairedByReplacingKeys(string fileName)
    {
        string directory = Path.Combine(testDirectory, "keys");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, fileName);
        File.WriteAllText(path, "existing key material");
        Assert.Throws<InvalidOperationException>(() => Configure(DirectorySettings()));
        Assert.That(File.ReadAllText(path), Is.EqualTo("existing key material"));
        Assert.That(Directory.GetFiles(directory), Has.Length.EqualTo(1));
    }

    [Test]
    public void CorruptDirectoryFilesAreNeverReplaced()
    {
        string directory = Path.Combine(testDirectory, "keys");
        Directory.CreateDirectory(directory);
        foreach (string fileName in new[] { "signing.pfx", "encryption.pfx" })
        {
            File.WriteAllText(Path.Combine(directory, fileName), "existing key material");
        }
        Assert.Catch(() => Configure(DirectorySettings()));
        Assert.That(
            File.ReadAllText(Path.Combine(directory, "signing.pfx")),
            Is.EqualTo("existing key material")
        );
    }

    [Test]
    public void ExpiredDirectoryCertificatesAreNeverReplaced()
    {
        string directory = Path.Combine(testDirectory, "keys");
        Directory.CreateDirectory(directory);
        using var signing = CreateCertificate(X509KeyUsageFlags.DigitalSignature, expired: true);
        using var encryption = CreateCertificate(X509KeyUsageFlags.KeyEncipherment, expired: true);
        string signingPath = Path.Combine(directory, "signing.pfx");
        byte[] original = signing.Export(X509ContentType.Pfx);
        File.WriteAllBytes(signingPath, original);
        File.WriteAllBytes(
            Path.Combine(directory, "encryption.pfx"),
            encryption.Export(X509ContentType.Pfx)
        );
        Assert.Catch(() => Configure(DirectorySettings()));
        Assert.That(File.ReadAllBytes(signingPath), Is.EqualTo(original));
    }

    [Test]
    public void ProductionStillRequiresAnExplicitCertificateSource()
    {
        Assert.Throws<InvalidOperationException>(() => Configure(new Dictionary<string, string>()));
    }

    [Test]
    public void MissingStoreCertificatesNeverFallBackToGeneration()
    {
        var settings = DirectorySettings();
        settings["SigningCertificateThumbprint"] = new string('0', 40);
        settings["EncryptionCertificateThumbprint"] = new string('1', 40);
        settings["CertificateStoreLocation"] = "CurrentUser";
        if (OperatingSystem.IsWindows())
        {
            Assert.Throws<InvalidOperationException>(() => Configure(settings));
        }
        else
        {
            Assert.Throws<PlatformNotSupportedException>(() => Configure(settings));
        }
        Assert.That(Directory.Exists(Path.Combine(testDirectory, "keys")), Is.False);
    }

    [Test]
    [Platform("Win")]
    public void WindowsStoreLoadsPrivateKeysWithoutPfxFiles()
    {
        using var signing = CreateCertificate(X509KeyUsageFlags.DigitalSignature);
        using var encryption = CreateCertificate(X509KeyUsageFlags.KeyEncipherment);
        var stored = new[] { signing, encryption }
            .Select(certificate =>
                X509CertificateLoader.LoadPkcs12(
                    certificate.Export(X509ContentType.Pfx),
                    null,
                    X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet
                )
            )
            .ToArray();
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadWrite);
        try
        {
            foreach (var certificate in stored)
            {
                store.Add(certificate);
            }
            var settings = DirectorySettings();
            settings["SigningCertificateThumbprint"] = signing.Thumbprint;
            settings["EncryptionCertificateThumbprint"] = encryption.Thumbprint;
            settings["CertificateStoreLocation"] = "CurrentUser";
            var options = Configure(settings);
            Assert.That(SigningCertificate(options).Thumbprint, Is.EqualTo(signing.Thumbprint));
            Assert.That(
                EncryptionCertificate(options).Thumbprint,
                Is.EqualTo(encryption.Thumbprint)
            );
            Assert.That(SigningCertificate(options).HasPrivateKey, Is.True);
            Assert.That(EncryptionCertificate(options).HasPrivateKey, Is.True);
            Assert.That(Directory.Exists(Path.Combine(testDirectory, "keys")), Is.False);
            Assert.That(startupInfo.Level, Is.EqualTo(LogLevel.Information));
            Assert.That(
                startupInfo.Message,
                Does.Contain("Windows store CurrentUser\\My")
                    .And.Contain(signing.Thumbprint)
                    .And.Contain(encryption.Thumbprint)
            );
        }
        finally
        {
            foreach (var certificate in stored)
            {
                store.Remove(certificate);
                using var key = certificate.GetRSAPrivateKey();
                if (key is RSACng cng && !cng.Key.IsEphemeral)
                {
                    cng.Key.Delete();
                }
                certificate.Dispose();
            }
        }
    }

    private OpenIddictServerOptions Configure(
        Dictionary<string, string> settings,
        string environmentName = null
    )
    {
        var values = settings.ToDictionary(
            setting => $"OpenIddictConfig:{setting.Key}",
            setting => setting.Value
        );
        values["OpenIddictConfig:ClientApplicationTemplates:Configured"] = "true";
        var configuration = new OpenIddictConfig(
            new ConfigurationBuilder().AddInMemoryCollection(values).Build()
        );
        var environment = new Mock<IHostEnvironment>();
        environment
            .SetupGet(host => host.EnvironmentName)
            .Returns(environmentName ?? Environments.Production);
        environment.SetupGet(host => host.ContentRootPath).Returns(testDirectory);
        var services = new ServiceCollection();
        var builder = services.AddOpenIddict().AddServer();
        builder.AllowClientCredentialsFlow().SetTokenEndpointUris("connect/token");
        startupInfo = OpenIddictCertificateConfiguration.Configure(
            builder,
            configuration,
            environment.Object
        );
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<OpenIddictServerOptions>>().Value;
        certificates.Add(SigningCertificate(options));
        certificates.Add(EncryptionCertificate(options));
        return options;
    }

    private static X509Certificate2 SigningCertificate(OpenIddictServerOptions options)
    {
        return ((X509SecurityKey)options.SigningCredentials.Single().Key).Certificate;
    }

    private static X509Certificate2 EncryptionCertificate(OpenIddictServerOptions options)
    {
        return ((X509SecurityKey)options.EncryptionCredentials.Single().Key).Certificate;
    }

    private static X509Certificate2 CreateCertificate(X509KeyUsageFlags usage, bool expired = false)
    {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest(
            $"CN=Origam certificate test {Guid.NewGuid():N}",
            key,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );
        request.CertificateExtensions.Add(new X509KeyUsageExtension(usage, critical: true));
        if (expired)
        {
            return request.CreateSelfSigned(
                DateTimeOffset.UtcNow.AddDays(-3),
                DateTimeOffset.UtcNow.AddDays(-1)
            );
        }
        return request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow.AddDays(2)
        );
    }

    private static Dictionary<string, string> DirectorySettings()
    {
        return new Dictionary<string, string>
        {
            ["FallbackGeneratedCertificateDirectory"] = "keys",
        };
    }

    private static Dictionary<string, string> FileSettings()
    {
        var settings = DirectorySettings();
        settings["SigningCertificatePath"] = "signing.p12";
        settings["EncryptionCertificatePath"] = "encryption.p12";
        return settings;
    }
}
