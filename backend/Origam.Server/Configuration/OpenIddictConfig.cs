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
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.Extensions.Configuration;
using Origam.Extensions;

namespace Origam.Server.Configuration;

public enum AuthenticationType
{
    Email,
    Username,
}

public enum AuthenticationMethod
{
    Cookie,
    Token,
}

public static class IdentityServerDefaults
{
    public const string AzureAdScheme = "AzureAd";
    public const string WindowsAdScheme = "Windows";
}

public class OpenIddictConfig
{
    public GoogleLogin GoogleLogin { get; }
    public MicrosoftLogin MicrosoftLogin { get; }
    public AzureAdLogin AzureAdLogin { get; }
    public bool CookieSlidingExpiration { get; }
    public int CookieExpirationMinutes { get; }
    public string AuthenticationPostProcessor { get; }
    public string AccessTokenIssuer { get; }
    public AuthenticationMethod PrivateApiAuthentication { get; }
    public ClientApplicationTemplates ClientApplicationTemplates { get; set; }

    public OpenIddictConfig(IConfiguration configuration)
    {
        var openIddictSection = configuration.GetSectionOrThrow("OpenIddictConfig");
        CookieSlidingExpiration = openIddictSection.GetValue("CookieSlidingExpiration", true);
        PrivateApiAuthentication = openIddictSection.GetValue(
            "PrivateApiAuthentication",
            AuthenticationMethod.Cookie
        );
        CookieExpirationMinutes = openIddictSection.GetValue("CookieExpirationMinutes", 60);
        GoogleLogin = ConfigureGoogleLogin(openIddictSection);
        MicrosoftLogin = ConfigureMicrosoftLogin(openIddictSection);
        AzureAdLogin = ConfigureAzureAdLogin(openIddictSection);
        var clientSection = openIddictSection.GetSectionOrThrow("ClientApplicationTemplates");
        ClientApplicationTemplates = new ClientApplicationTemplates
        {
            WebClient = ConfigureWebClient(clientSection),
            MobileClient = ConfigureMobileClient(clientSection),
            ServerClient = ConfigureServerClient(clientSection),
        };
        AuthenticationPostProcessor = openIddictSection.GetValue("AuthenticationPostProcessor", "");
        AccessTokenIssuer = openIddictSection.GetValue("AccessTokenIssuer", "");
    }

    private ServerClient ConfigureServerClient(IConfigurationSection identityServerSection)
    {
        var serverClientSection = identityServerSection.GetSection("ServerClient");
        if (serverClientSection.Exists())
        {
            return new ServerClient
            {
                ClientSecret = serverClientSection.GetStringOrThrow("ClientSecret"),
            };
        }
        return null;
    }

    private MobileClient ConfigureMobileClient(IConfigurationSection identityServerSection)
    {
        var mobileClientSection = identityServerSection.GetSection("MobileClient");
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
            };
        }
        return null;
    }

    private WebClient ConfigureWebClient(IConfigurationSection identityServerSection)
    {
        var webClientSection = identityServerSection.GetSection("WebClient");
        if (webClientSection.GetChildren().Any())
        {
            return new WebClient
            {
                PostLogoutRedirectUris = webClientSection
                    .GetSectionOrThrow("PostLogoutRedirectUris")
                    .GetStringArrayOrThrow(),
                RedirectUris = webClientSection
                    .GetSectionOrThrow("RedirectUris")
                    .GetStringArrayOrThrow()
                    .Select(x => x.Replace("#", ""))
                    .ToArray(),
                AllowedCorsOrigins =
                    webClientSection.GetSection("AllowedCorsOrigins")?.Get<string[]>() ?? [],
            };
        }
        return null;
    }

    private GoogleLogin ConfigureGoogleLogin(IConfigurationSection identityServerSection)
    {
        var googleLoginSection = identityServerSection.GetSection("GoogleLogin");
        if (googleLoginSection.GetChildren().Any())
        {
            return new GoogleLogin(googleLoginSection)
            {
                ClientId = googleLoginSection.GetStringOrThrow("ClientId"),
                ClientSecret = googleLoginSection.GetStringOrThrow("ClientSecret"),
            };
        }
        return null;
    }

    private MicrosoftLogin ConfigureMicrosoftLogin(IConfigurationSection identityServerSection)
    {
        var microsoftLoginSection = identityServerSection.GetSection("MicrosoftLogin");
        if (microsoftLoginSection.GetChildren().Any())
        {
            return new MicrosoftLogin(microsoftLoginSection)
            {
                ClientId = microsoftLoginSection.GetStringOrThrow("ClientId"),
                ClientSecret = microsoftLoginSection.GetStringOrThrow("ClientSecret"),
            };
        }
        return null;
    }

    private AzureAdLogin ConfigureAzureAdLogin(IConfigurationSection identityServerSection)
    {
        var azureAdLoginSection = identityServerSection.GetSection("AzureAdLogin");
        if (azureAdLoginSection.GetChildren().Any())
        {
            return new AzureAdLogin(azureAdLoginSection)
            {
                ClientId = azureAdLoginSection.GetStringOrThrow("ClientId"),
                TenantId = azureAdLoginSection.GetStringOrThrow("TenantId"),
            };
        }
        return null;
    }

    public ExternalCallbackProcessingInfo GetExternalCallbackProcessingInfo(
        string authenticationScheme
    )
    {
        switch (authenticationScheme)
        {
            case GoogleDefaults.AuthenticationScheme:
            {
                return GoogleLogin;
            }
            case MicrosoftAccountDefaults.AuthenticationScheme:
            {
                return MicrosoftLogin;
            }
            case IdentityServerDefaults.AzureAdScheme:
            {
                return AzureAdLogin;
            }
            case IdentityServerDefaults.WindowsAdScheme:
            {
                return new WindowsLogin();
            }
            default:
            {
                throw new ArgumentOutOfRangeException(
                    nameof(authenticationScheme),
                    $@"Invalid authentication scheme {authenticationScheme}"
                );
            }
        }
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
}

public abstract class ExternalCallbackProcessingInfo
{
    public AuthenticationType AuthenticationType { get; set; }
    public string ClaimType { get; set; }

    protected ExternalCallbackProcessingInfo(IConfigurationSection configurationSection)
    {
        AuthenticationType = configurationSection.GetValue(
            "AuthenticationType",
            AuthenticationType.Email
        );
        ClaimType = configurationSection.GetValue("ClaimType", ClaimTypes.Email);
    }

    protected ExternalCallbackProcessingInfo() { }
}

public class GoogleLogin : ExternalCallbackProcessingInfo
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }

    public GoogleLogin(IConfigurationSection configurationSection)
        : base(configurationSection) { }
}

public class MicrosoftLogin : ExternalCallbackProcessingInfo
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }

    public MicrosoftLogin(IConfigurationSection configurationSection)
        : base(configurationSection) { }
}

public class AzureAdLogin : ExternalCallbackProcessingInfo
{
    public string ClientId { get; set; }
    public string TenantId { get; set; }

    public AzureAdLogin(IConfigurationSection configurationSection)
        : base(configurationSection) { }
}

public class WindowsLogin : ExternalCallbackProcessingInfo
{
    public WindowsLogin(IConfigurationSection configurationSection)
        : base(configurationSection) { }

    public WindowsLogin()
    {
        AuthenticationType = AuthenticationType.Username;
        ClaimType = "name";
    }
}

public class ClientApplicationTemplates
{
    public MobileClient MobileClient { get; set; }
    public WebClient WebClient { get; set; }
    public ServerClient ServerClient { get; set; }
}
