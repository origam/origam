using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Origam.ServerCore.Configuration
{
    public class StartUpConfiguration
    {
        private readonly IConfiguration configuration;

        public StartUpConfiguration(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string SecurityKey {
            get
            {
                string securityKey = configuration["SecurityKey"];
                if (string.IsNullOrWhiteSpace(securityKey))
                {
                    throw new ArgumentException("SecurityKey was not found in configuration. Please add it to appsettings.json");
                }

                if (securityKey.Length < 16)
                {
                    throw new ArgumentException("SecurityKey found in appsettings.json has to be at least 16 characters long!");
                }
                return securityKey;
            }
        }

        public IEnumerable<string> UserApiPublicRoutes =>  
            configuration
                .GetSection("UserApiOptions")
                .GetSection("PublicRoutes")
                .GetChildren()
                .Select(c => c.Value);

        public IEnumerable<string> UserApiRestrictedRoutes =>
            configuration
                .GetSection("UserApiOptions")
                .GetSection("RestrictedRoutes")
                .GetChildren()
                .Select(c => c.Value);

        public string PathToClientApp => configuration["PathToClientApp"];
    }
}
