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

using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Origam.Server.Configuration;

internal static class OpenIddictCertificateConfiguration
{
    public static TokenCertificateStartupInfo Configure(
        OpenIddictServerBuilder options,
        OpenIddictConfig configuration,
        IHostEnvironment environment
    )
    {
        string signingPath = configuration.SigningCertificatePath;
        string encryptionPath = configuration.EncryptionCertificatePath;
        bool hasFileConfiguration =
            !string.IsNullOrWhiteSpace(signingPath) || !string.IsNullOrWhiteSpace(encryptionPath);
        bool hasStoreConfiguration =
            !string.IsNullOrWhiteSpace(configuration.SigningCertificateThumbprint)
            || !string.IsNullOrWhiteSpace(configuration.EncryptionCertificateThumbprint);
        if (hasFileConfiguration && hasStoreConfiguration)
        {
            throw new InvalidOperationException(Resources.ErrorAmbiguousTokenCertificates);
        }

        if (hasStoreConfiguration)
        {
            if (
                string.IsNullOrWhiteSpace(configuration.SigningCertificateThumbprint)
                || string.IsNullOrWhiteSpace(configuration.EncryptionCertificateThumbprint)
            )
            {
                throw new InvalidOperationException(Resources.ErrorMissingTokenCertificates);
            }
            var signing = LoadFromStore(configuration.SigningCertificateThumbprint, configuration);
            var encryption = LoadFromStore(
                configuration.EncryptionCertificateThumbprint,
                configuration
            );
            options.AddSigningCertificate(signing);
            options.AddEncryptionCertificate(encryption);
            return new TokenCertificateStartupInfo(
                LogLevel.Information,
                string.Format(
                    Resources.TokenCertificatesLoadedFromStore,
                    configuration.CertificateStoreLocation,
                    configuration.CertificateStoreName,
                    signing.Thumbprint,
                    signing.NotAfter.ToUniversalTime(),
                    encryption.Thumbprint,
                    encryption.NotAfter.ToUniversalTime()
                )
            );
        }

        string sourceMessage = Resources.TokenCertificatesLoadedFromFiles;
        var logLevel = LogLevel.Information;
        if (hasFileConfiguration)
        {
            if (string.IsNullOrWhiteSpace(signingPath) || string.IsNullOrWhiteSpace(encryptionPath))
            {
                throw new InvalidOperationException(Resources.ErrorMissingTokenCertificates);
            }
        }
        else if (!string.IsNullOrWhiteSpace(configuration.FallbackGeneratedCertificateDirectory))
        {
            string directory = Path.GetFullPath(
                configuration.FallbackGeneratedCertificateDirectory,
                environment.ContentRootPath
            );
            signingPath = Path.Combine(directory, path2: "signing.pfx");
            encryptionPath = Path.Combine(directory, path2: "encryption.pfx");
            bool created = EnsureCertificateFiles(
                directory,
                signingPath,
                encryptionPath,
                configuration
            );
            sourceMessage = created
                ? Resources.TokenCertificatesGeneratedFallback
                : Resources.TokenCertificatesReusedFallback;
            logLevel = LogLevel.Warning;
        }
        else if (environment.IsDevelopment())
        {
            options.AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate();
            return new TokenCertificateStartupInfo(
                LogLevel.Warning,
                Resources.TokenCertificatesUsingDevelopment
            );
        }
        else
        {
            throw new InvalidOperationException(Resources.ErrorMissingTokenCertificates);
        }

        var signingCertificate = LoadCertificate(
            signingPath,
            configuration.SigningCertificatePassword,
            environment.ContentRootPath
        );
        var encryptionCertificate = LoadCertificate(
            encryptionPath,
            configuration.EncryptionCertificatePassword,
            environment.ContentRootPath
        );
        options.AddSigningCertificate(signingCertificate);
        options.AddEncryptionCertificate(encryptionCertificate);
        return new TokenCertificateStartupInfo(
            logLevel,
            string.Format(
                sourceMessage,
                Path.GetFullPath(signingPath, environment.ContentRootPath),
                signingCertificate.Thumbprint,
                signingCertificate.NotAfter.ToUniversalTime(),
                Path.GetFullPath(encryptionPath, environment.ContentRootPath),
                encryptionCertificate.Thumbprint,
                encryptionCertificate.NotAfter.ToUniversalTime()
            )
        );
    }

    private static X509Certificate2 LoadFromStore(string thumbprint, OpenIddictConfig configuration)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                Resources.ErrorTokenCertificateStoreWindowsOnly
            );
        }
        using var store = new X509Store(
            configuration.CertificateStoreName,
            configuration.CertificateStoreLocation
        );
        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
        var certificates = store.Certificates.Find(
            X509FindType.FindByThumbprint,
            thumbprint,
            validOnly: false
        );
        if (certificates.Count == 0)
        {
            throw new InvalidOperationException(
                string.Format(
                    Resources.ErrorTokenCertificateNotFound,
                    thumbprint,
                    configuration.CertificateStoreLocation,
                    configuration.CertificateStoreName
                )
            );
        }
        return certificates[0];
    }

    private static bool EnsureCertificateFiles(
        string directory,
        string signingPath,
        string encryptionPath,
        OpenIddictConfig configuration
    )
    {
        bool hasSigningFile = File.Exists(signingPath);
        bool hasEncryptionFile = File.Exists(encryptionPath);
        if (hasSigningFile && hasEncryptionFile)
        {
            return false;
        }
        if (hasSigningFile || hasEncryptionFile)
        {
            throw new InvalidOperationException(
                string.Format(Resources.ErrorIncompleteTokenCertificateDirectory, directory)
            );
        }
        if (OperatingSystem.IsWindows())
        {
            Directory.CreateDirectory(directory);
        }
        else
        {
            Directory.CreateDirectory(
                directory,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute
            );
        }
        using var signing = CreateCertificate(
            name: "Origam Token Signing",
            X509KeyUsageFlags.DigitalSignature
        );
        using var encryption = CreateCertificate(
            name: "Origam Token Encryption",
            X509KeyUsageFlags.KeyEncipherment
        );
        WriteCertificate(
            signingPath,
            signing.Export(X509ContentType.Pfx, configuration.SigningCertificatePassword)
        );
        WriteCertificate(
            encryptionPath,
            encryption.Export(X509ContentType.Pfx, configuration.EncryptionCertificatePassword)
        );
        return true;
    }

    private static X509Certificate2 CreateCertificate(string name, X509KeyUsageFlags usage)
    {
        using var key = RSA.Create(4096);
        var request = new CertificateRequest(
            new X500DistinguishedName($"CN={name}"),
            key,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );
        request.CertificateExtensions.Add(new X509KeyUsageExtension(usage, critical: true));
        var now = DateTimeOffset.UtcNow;
        return request.CreateSelfSigned(now.AddMinutes(-5), now.AddYears(2));
    }

    private static void WriteCertificate(string path, byte[] contents)
    {
        var fileOptions = new FileStreamOptions
        {
            Mode = FileMode.CreateNew,
            Access = FileAccess.Write,
            Share = FileShare.None,
        };
        if (!OperatingSystem.IsWindows())
        {
            fileOptions.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
        }
        using var stream = new FileStream(path, fileOptions);
        if (OperatingSystem.IsWindows())
        {
            using var currentIdentity = WindowsIdentity.GetCurrent();
            var security = new FileSecurity();
            security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
            foreach (
                var identity in new[]
                {
                    currentIdentity.User,
                    new SecurityIdentifier(WellKnownSidType.LocalSystemSid, domainSid: null),
                    new SecurityIdentifier(
                        WellKnownSidType.BuiltinAdministratorsSid,
                        domainSid: null
                    ),
                }
            )
            {
                security.AddAccessRule(
                    new FileSystemAccessRule(
                        identity,
                        FileSystemRights.FullControl,
                        AccessControlType.Allow
                    )
                );
            }
            new FileInfo(path).SetAccessControl(security);
        }
        stream.Write(contents);
    }

    private static X509Certificate2 LoadCertificate(
        string path,
        string password,
        string contentRootPath
    )
    {
        // Import the existing PFX private key into memory without persisting it
        // in an OS key store. Restarts reload the same key from the PFX file.
        return X509CertificateLoader.LoadPkcs12FromFile(
            Path.GetFullPath(path, contentRootPath),
            password,
            X509KeyStorageFlags.EphemeralKeySet
        );
    }
}

internal sealed record TokenCertificateStartupInfo(LogLevel Level, string Message)
{
    public const string LoggerCategory = "Origam.Server.TokenCertificates";

    public void Log(ILogger logger)
    {
        logger.Log(Level, message: "{CertificateStartupMessage}", Message);
    }
}
