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

using System.Linq;
using Microsoft.Extensions.Configuration;
using Origam.Extensions;

namespace Origam.Server.Configuration
{
    public class IdentityServerConfig
    {
        public string PathToJwtCertificate { get; }
        public string PasswordForJwtCertificate { get; }
        public GoogleLogin GoogleLogin { get; }
        public MicrosoftLogin MicrosoftLogin { get; }
        public AzureAdLogin AzureAdLogin {get;}
        public WebClient WebClient { get; }
        public MobileClient MobileClient { get; }
        public ServerClient ServerClient { get; }
        public bool CookieSlidingExpiration { get; } 
        public int CookieExpirationMinutes { get; }

        public IdentityServerConfig(IConfiguration configuration)
        {
            IConfigurationSection identityServerSection = configuration
                .GetSectionOrThrow("IdentityServerConfig");
            PathToJwtCertificate = identityServerSection
                .GetStringOrThrow("PathToJwtCertificate");
            PasswordForJwtCertificate = identityServerSection.GetValue<string>(
                    "PasswordForJwtCertificate");
            CookieSlidingExpiration = identityServerSection
                .GetValue("CookieSlidingExpiration", true);
            CookieExpirationMinutes = identityServerSection
                .GetValue("CookieExpirationMinutes", 60);
            GoogleLogin = ConfigureGoogleLogin(identityServerSection);
            MicrosoftLogin = ConfigureMicrosoftLogin(identityServerSection);
            AzureAdLogin = ConfigureAzureAdLogin(identityServerSection);
            WebClient = ConfigureWebClient(identityServerSection);
            MobileClient = ConfigureMobileClient(identityServerSection);
            ServerClient = ConfigureServerClient(identityServerSection);
        }

        private ServerClient ConfigureServerClient(
            IConfigurationSection identityServerSection)
        {
            var serverClientSection = identityServerSection
                .GetSection("ServerClient");
            if (identityServerSection.GetChildren().Any())
            {
                return new ServerClient
                {
                    ClientSecret = serverClientSection.GetStringOrThrow("ClientSecret")
                };
            }
            return null;
        }

        private MobileClient ConfigureMobileClient(
            IConfigurationSection identityServerSection)
        {
            var mobileClientSection = identityServerSection
                .GetSection("MobileClient");
            if (mobileClientSection.GetChildren().Any())
            {
                return new MobileClient
                {
                    PostLogoutRedirectUris = mobileClientSection
                        .GetSectionOrThrow("PostLogoutRedirectUris")
                        .GetStringArrayOrThrow(),
                    RedirectUris = mobileClientSection
                        .GetSectionOrThrow("RedirectUris")
                        .GetStringArrayOrThrow(),
                    ClientSecret= mobileClientSection.GetStringOrThrow("ClientSecret")
                }; 
            }
            return null;
        }

        private WebClient ConfigureWebClient(
            IConfigurationSection identityServerSection)
        {
            var webClientSection = identityServerSection
                .GetSection("WebClient");
            if (webClientSection.GetChildren().Any())
            {
                return new WebClient
                {
                    PostLogoutRedirectUris = webClientSection
                        .GetSectionOrThrow("PostLogoutRedirectUris")
                        .GetStringArrayOrThrow(),
                    RedirectUris = webClientSection
                        .GetSectionOrThrow("RedirectUris")
                        .GetStringArrayOrThrow(),
                    AllowedCorsOrigins = webClientSection
                        .GetSection("AllowedCorsOrigins")
                        ?.Get<string[]>()
                };
            }
            return null;
        }

        private GoogleLogin ConfigureGoogleLogin(
            IConfigurationSection identityServerSection)
        {
            var googleLoginSection = identityServerSection
                .GetSection("GoogleLogin");
            if (googleLoginSection.GetChildren().Any())
            {
                return new GoogleLogin()
                {
                    ClientId = googleLoginSection.GetStringOrThrow("ClientId"),
                    ClientSecret = googleLoginSection
                        .GetStringOrThrow("ClientSecret")
                };
            }
            return null;
        }
        
        private MicrosoftLogin ConfigureMicrosoftLogin(
            IConfigurationSection identityServerSection)
        {
            var microsoftLoginSection = identityServerSection
                .GetSection("MicrosoftLogin");
            if (microsoftLoginSection.GetChildren().Any())
            {
                return new MicrosoftLogin()
                {
                    ClientId = microsoftLoginSection.GetStringOrThrow(
                        "ClientId"),
                    ClientSecret = microsoftLoginSection.GetStringOrThrow(
                    "ClientSecret")
                };
            }
            return null;
        }
        
        private AzureAdLogin ConfigureAzureAdLogin(
            IConfigurationSection identityServerSection)
        {
            var azureAdLoginSection = identityServerSection
                .GetSection("AzureAdLogin");
            if (azureAdLoginSection.GetChildren().Any())
            {
                return new AzureAdLogin()
                {
                    ClientId = azureAdLoginSection.GetStringOrThrow("ClientId"),
                    TenantId = azureAdLoginSection.GetStringOrThrow("TenantId")
                };
            }
            return null;
        }
    }

    public class ServerClient
    {
        public string ClientSecret { get; set; }
    }

    public class WebClient
    {
        public string[] RedirectUris { get; set; }
        public string[] PostLogoutRedirectUris { get; set; }
        public string[] AllowedCorsOrigins { get; set; }  
    }
    
    public class MobileClient
    {
        public string[] RedirectUris { get; set; }
        public string[] PostLogoutRedirectUris { get; set; }
        public string ClientSecret { get; set; }
    }

    public class GoogleLogin
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class MicrosoftLogin
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
    
    public class AzureAdLogin
    {
        public string ClientId { get; set; }
        public string TenantId { get; set; }
    }
}