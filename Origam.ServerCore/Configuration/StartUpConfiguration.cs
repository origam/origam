#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using System.Linq;
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
