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

#pragma warning disable IDE0005
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Origam.Extensions;
#pragma warning restore IDE0005
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Origam.Mail;

public class MailServiceFactory
{
#if NETSTANDARD
    private static IServiceProvider DIServiceProvider;

    public static void SetDIServiceProvider(IServiceProvider diServiceProvider)
    {
        DIServiceProvider = diServiceProvider;
    }
#endif

    private MailServiceFactory() { }

    public static IMailService GetMailService()
    {
#if NETSTANDARD
        var configuration = DIServiceProvider.GetService<IConfiguration>();
        var mailConfig = configuration.GetSection(key: "MailConfig");
        string username = mailConfig[key: "UserName"];
        bool useSsl = mailConfig.GetBoolOrThrow(key: "UseSsl");
        string password = mailConfig[key: "Password"];
        string server = mailConfig[key: "Server"];
        int port = mailConfig.GetIntOrThrow(key: "Port");
        string pickupDirectoryLocation = mailConfig[key: "PickupDirectoryLocation"];
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
