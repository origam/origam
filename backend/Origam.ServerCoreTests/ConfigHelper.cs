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

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Origam.ServerCoreTests;
class ConfigHelper
{
    public static IConfigurationRoot GetIConfigurationRoot(string outputPath)
    {
        var secretsIdConfig = new ConfigurationBuilder()
            .SetBasePath(outputPath)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
        IConfigurationSection configurationSection = secretsIdConfig.GetSection("UserSecretsId");
        var userSecretsId = configurationSection.Value;
        return new ConfigurationBuilder()
            .SetBasePath(outputPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets(userSecretsId)
            //.AddEnvironmentVariables()
            .Build();
    }
    public static ServerCoreTestConfiguration GetApplicationConfiguration(string outputPath)
    {
        var configuration = new ServerCoreTestConfiguration();
        var iConfig = GetIConfigurationRoot(outputPath);
        iConfig
            .GetSection("ServerCoreTestConfiguration")
            .Bind(configuration);
        return configuration;
    }
}
public class ServerCoreTestConfiguration
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string TestDbName { get; set; }
    public string PathToBakFile { get; set; }
    public string ServerName { get; set; }
    public string UserIdInTestDatabase { get; set; }
}
