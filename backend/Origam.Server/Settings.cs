#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;
using Origam.Server.Configuration;
using IdentityServerConstants = IdentityServer4.IdentityServerConstants;

namespace Origam.Server;

static class Settings
{
    internal static ApiResource[] GetIdentityApiResources()
    {
        return new[] { new ApiResource(IdentityServerConstants.LocalApi.ScopeName) };
    }

    internal static IdentityResource[] GetIdentityResources()
    {
        return new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };
    }

    internal static Client[] GetIdentityClients(IdentityServerConfig identityServerConfig)
    {
        List<Client> clients = new List<Client>();
        if (identityServerConfig.ServerClient != null)
        {
            Client serverClient = new Client
            {
                ClientId = "serverClient",
                ClientSecrets = new[]
                {
                    new Secret(identityServerConfig.ServerClient.ClientSecret.Sha256()),
                },
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                AllowedScopes = new List<string> { IdentityServerConstants.LocalApi.ScopeName },
                AccessTokenType = AccessTokenType.Reference,
            };
            clients.Add(serverClient);
        }
        if (identityServerConfig.MobileClient != null)
        {
            var mobileClient = new Client
            {
                ClientId = "origamMobileClient",
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                RedirectUris = identityServerConfig.MobileClient.RedirectUris,
                RequireConsent = false,
                RequirePkce = true,
                PostLogoutRedirectUris = identityServerConfig.MobileClient.PostLogoutRedirectUris,
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.LocalApi.ScopeName,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                },
                AllowOfflineAccess = true,
                AllowAccessTokensViaBrowser = true,
                AccessTokenType = AccessTokenType.Reference,
            };
            clients.Add(mobileClient);
        }
        if (identityServerConfig.WebClient != null)
        {
            var webClient = new Client
            {
                ClientId = "origamWebClient",
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                RedirectUris = identityServerConfig.WebClient.RedirectUris,
                RequireConsent = false,
                RequirePkce = true,
                PostLogoutRedirectUris = identityServerConfig.WebClient.PostLogoutRedirectUris,
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.LocalApi.ScopeName,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                },
                AllowOfflineAccess = true,
                AllowAccessTokensViaBrowser = true,
                AccessTokenType = AccessTokenType.Reference,
                AllowedCorsOrigins = identityServerConfig.WebClient.AllowedCorsOrigins,
            };
            clients.Add(webClient);
        }
        return clients.ToArray();
    }

    public static IEnumerable<ApiScope> GetApiScopes()
    {
        return new List<ApiScope> { new ApiScope(IdentityServerConstants.LocalApi.ScopeName) };
    }
}
