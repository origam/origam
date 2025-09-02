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

#pragma warning disable IDE0005 // Using directive is unnecessary
using System.IO;
using Microsoft.Extensions.Configuration;
using Origam.Extensions;
#pragma warning restore IDE0005 // Re-enable the warning

namespace Origam.Mail;

public class MailServiceFactory
{
#if NETSTANDARD
    private static readonly IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();
#endif

    private MailServiceFactory() { }

    public static IMailService GetMailService()
    {
#if NETSTANDARD
        var mailConfig = configuration.GetSection("MailConfig");
        string username = mailConfig["UserName"];
        bool useSsl = mailConfig.GetBoolOrThrow("UseSsl");
        string password = mailConfig["Password"];
        string server = mailConfig["Server"];
        int port = mailConfig.GetIntOrThrow("Port");
        string pickupDirectoryLocation = mailConfig["PickupDirectoryLocation"];
        return new NetStandardMailService(
            server: server,
            port: port,
            pickupDirectoryLocation: pickupDirectoryLocation,
            username: username,
            password: password,
            useSsl: useSsl
        );
#else
        return new NetFxMailService();
#endif
    }
}
