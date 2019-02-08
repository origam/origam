using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Origam.ServerCoreTests
{
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
    }
}
