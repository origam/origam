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
        var openIddictSection = configuration.GetSectionOrThrow(key: "OpenIddictConfig");
        CookieSlidingExpiration = openIddictSection.GetValue(
            key: "CookieSlidingExpiration",
            defaultValue: true
        );
        PrivateApiAuthentication = openIddictSection.GetValue(
            key: "PrivateApiAuthentication",
            defaultValue: AuthenticationMethod.Cookie
        );
        CookieExpirationMinutes = openIddictSection.GetValue(
            key: "CookieExpirationMinutes",
            defaultValue: 60
        );
        GoogleLogin = ConfigureGoogleLogin(identityServerSection: openIddictSection);
        MicrosoftLogin = ConfigureMicrosoftLogin(identityServerSection: openIddictSection);
        AzureAdLogin = ConfigureAzureAdLogin(identityServerSection: openIddictSection);
        var clientSection = openIddictSection.GetSectionOrThrow(key: "ClientApplicationTemplates");
        ClientApplicationTemplates = new ClientApplicationTemplates
        {
            WebClient = ConfigureWebClient(identityServerSection: clientSection),
            MobileClient = ConfigureMobileClient(identityServerSection: clientSection),
            ServerClient = ConfigureServerClient(identityServerSection: clientSection),
        };
        AuthenticationPostProcessor = openIddictSection.GetValue(
            key: "AuthenticationPostProcessor",
            defaultValue: ""
        );
        AccessTokenIssuer = openIddictSection.GetValue(key: "AccessTokenIssuer", defaultValue: "");
    }

    private ServerClient ConfigureServerClient(IConfigurationSection identityServerSection)
    {
        var serverClientSection = identityServerSection.GetSection(key: "ServerClient");
        if (serverClientSection.Exists())
        {
            return new ServerClient
            {
                ClientSecret = serverClientSection.GetStringOrThrow(key: "ClientSecret"),
            };
        }
        return null;
    }

    private MobileClient ConfigureMobileClient(IConfigurationSection identityServerSection)
    {
        var mobileClientSection = identityServerSection.GetSection(key: "MobileClient");
        if (mobileClientSection.GetChildren().Any())
        {
            return new MobileClient
            {
                PostLogoutRedirectUris = mobileClientSection
                    .GetSectionOrThrow(key: "PostLogoutRedirectUris")
                    .GetStringArrayOrThrow(),
                RedirectUris = mobileClientSection
                    .GetSectionOrThrow(key: "RedirectUris")
                    .GetStringArrayOrThrow(),
            };
        }
        return null;
    }

    private WebClient ConfigureWebClient(IConfigurationSection identityServerSection)
    {
        var webClientSection = identityServerSection.GetSection(key: "WebClient");
        if (webClientSection.GetChildren().Any())
        {
            return new WebClient
            {
                PostLogoutRedirectUris = webClientSection
                    .GetSectionOrThrow(key: "PostLogoutRedirectUris")
                    .GetStringArrayOrThrow(),
                RedirectUris = webClientSection
                    .GetSectionOrThrow(key: "RedirectUris")
                    .GetStringArrayOrThrow()
                    .Select(selector: x => x.Replace(oldValue: "#", newValue: ""))
                    .ToArray(),
                AllowedCorsOrigins =
                    webClientSection.GetSection(key: "AllowedCorsOrigins")?.Get<string[]>() ?? [],
            };
        }
        return null;
    }

    private GoogleLogin ConfigureGoogleLogin(IConfigurationSection identityServerSection)
    {
        var googleLoginSection = identityServerSection.GetSection(key: "GoogleLogin");
        if (googleLoginSection.GetChildren().Any())
        {
            return new GoogleLogin(configurationSection: googleLoginSection)
            {
                ClientId = googleLoginSection.GetStringOrThrow(key: "ClientId"),
                ClientSecret = googleLoginSection.GetStringOrThrow(key: "ClientSecret"),
            };
        }
        return null;
    }

    private MicrosoftLogin ConfigureMicrosoftLogin(IConfigurationSection identityServerSection)
    {
        var microsoftLoginSection = identityServerSection.GetSection(key: "MicrosoftLogin");
        if (microsoftLoginSection.GetChildren().Any())
        {
            return new MicrosoftLogin(configurationSection: microsoftLoginSection)
            {
                ClientId = microsoftLoginSection.GetStringOrThrow(key: "ClientId"),
                ClientSecret = microsoftLoginSection.GetStringOrThrow(key: "ClientSecret"),
            };
        }
        return null;
    }

    private AzureAdLogin ConfigureAzureAdLogin(IConfigurationSection identityServerSection)
    {
        var azureAdLoginSection = identityServerSection.GetSection(key: "AzureAdLogin");
        if (azureAdLoginSection.GetChildren().Any())
        {
            return new AzureAdLogin(configurationSection: azureAdLoginSection)
            {
                ClientId = azureAdLoginSection.GetStringOrThrow(key: "ClientId"),
                TenantId = azureAdLoginSection.GetStringOrThrow(key: "TenantId"),
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
                    paramName: nameof(authenticationScheme),
                    message: $@"Invalid authentication scheme {authenticationScheme}"
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
            key: "AuthenticationType",
            defaultValue: AuthenticationType.Email
        );
        ClaimType = configurationSection.GetValue(key: "ClaimType", defaultValue: ClaimTypes.Email);
    }

    protected ExternalCallbackProcessingInfo() { }
}

public class GoogleLogin : ExternalCallbackProcessingInfo
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }

    public GoogleLogin(IConfigurationSection configurationSection)
        : base(configurationSection: configurationSection) { }
}

public class MicrosoftLogin : ExternalCallbackProcessingInfo
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }

    public MicrosoftLogin(IConfigurationSection configurationSection)
        : base(configurationSection: configurationSection) { }
}

public class AzureAdLogin : ExternalCallbackProcessingInfo
{
    public string ClientId { get; set; }
    public string TenantId { get; set; }

    public AzureAdLogin(IConfigurationSection configurationSection)
        : base(configurationSection: configurationSection) { }
}

public class WindowsLogin : ExternalCallbackProcessingInfo
{
    public WindowsLogin(IConfigurationSection configurationSection)
        : base(configurationSection: configurationSection) { }

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
