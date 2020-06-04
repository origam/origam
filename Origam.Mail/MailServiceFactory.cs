#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using Microsoft.Extensions.Configuration;
using Origam.Extensions;

namespace Origam.Mail
{
    public class MailServiceFactory
    {
#if NETSTANDARD
        private static readonly IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
#endif
        private MailServiceFactory()
        {

        }
        
        public static AbstractMailService GetMailService()
        {
#if NETSTANDARD
            var mailConfig = configuration.GetSection("MailConfig");
            string userName = mailConfig["UserName"];
            bool useSsl = mailConfig.GetBoolOrThrow("UseSsl");
            string password = mailConfig["Password"];
            string server = mailConfig["Server"];
            int port = mailConfig.GetIntOrThrow("Port");

            return new SystemNetMailService(
                server:server, port:port, userName: userName, password:password, useSsl:useSsl);            
#else
            return new SystemNetMailService();            
#endif
        }
    }
}
