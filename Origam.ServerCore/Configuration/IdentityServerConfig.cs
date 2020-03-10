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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Origam.ServerCore.Configuration
{
    public class IdentityServerConfig
    {
        public string PathToJwtCertificate { get; }
        public string PasswordForJwtCertificate { get; }
        public string GoogleClientId { get; }
        public string GoogleClientSecret { get; }
        public bool UseGoogleLogin { get;}
       
        public WebClient WebClient { get; set; }
        public MobileClient MobileClient { get; set; }

        public IdentityServerConfig(IConfiguration configuration)
        {
            IConfigurationSection identityServerSection = configuration.GetSection("IdentityServerConfig");
            PathToJwtCertificate = identityServerSection.GetValue<string>("PathToJwtCertificate");
            PasswordForJwtCertificate = identityServerSection.GetValue<string>("PasswordForJwtCertificate");
            UseGoogleLogin = identityServerSection.GetValue("UseGoogleLogin", false);
            GoogleClientId = identityServerSection["GoogleClientId"] ?? "";
            GoogleClientSecret = identityServerSection["GoogleClientSecret"] ?? "";
          
            
            WebClient = new WebClient
            {
                PostLogoutRedirectUris = identityServerSection
                    .GetSection("WebClient")
                    .GetSection("PostLogoutRedirectUris")
                    .Get<string[]>(),
                RedirectUris = identityServerSection
                    .GetSection("WebClient")
                    .GetSection("RedirectUris")
                    .Get<string[]>()
            };           
            
            MobileClient = new MobileClient
            {
                PostLogoutRedirectUris = identityServerSection
                    .GetSection("MobileClient")
                    .GetSection("PostLogoutRedirectUris")
                    .Get<string[]>(),
                RedirectUris = identityServerSection
                    .GetSection("MobileClient")
                    .GetSection("RedirectUris")
                    .Get<string[]>(),
                ClientSecret= identityServerSection
                    .GetSection("MobileClient")
                    .GetValue<string>("ClientSecret") 
                              ?? throw new Exception("ClientSecret not found in config")
            };
        }
    }

    public class WebClient
    {
        public string[] RedirectUris { get; set; }
        public string[] PostLogoutRedirectUris { get; set; }
    }
    
    public class MobileClient
    {
        public string[] RedirectUris { get; set; }
        public string[] PostLogoutRedirectUris { get; set; }
        public string ClientSecret { get; set; }
    }
}