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
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Origam.Server.Configuration;

internal static class OpenIddictCertificateConfiguration
{
    public static void Configure(
        OpenIddictServerBuilder options,
        OpenIddictConfig configuration,
        IHostEnvironment environment
    )
    {
        bool hasSigningCertificate = !string.IsNullOrWhiteSpace(
            configuration.SigningCertificatePath
        );
        bool hasEncryptionCertificate = !string.IsNullOrWhiteSpace(
            configuration.EncryptionCertificatePath
        );
        if (!hasSigningCertificate && !hasEncryptionCertificate && environment.IsDevelopment())
        {
            options.AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate();
            return;
        }

        if (!hasSigningCertificate || !hasEncryptionCertificate)
        {
            throw new InvalidOperationException(Resources.ErrorMissingTokenCertificates);
        }

        options.AddSigningCertificate(
            LoadCertificate(
                configuration.SigningCertificatePath,
                configuration.SigningCertificatePassword,
                environment.ContentRootPath
            )
        );
        options.AddEncryptionCertificate(
            LoadCertificate(
                configuration.EncryptionCertificatePath,
                configuration.EncryptionCertificatePassword,
                environment.ContentRootPath
            )
        );
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
