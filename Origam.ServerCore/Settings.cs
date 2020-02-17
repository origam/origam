using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;
using Origam.ServerCore.Configuration;

namespace Origam.ServerCore
{
    static class Settings
    {
        internal static ApiResource[] GetIdentityApiResources()
        {
            return new[]
            {
                new ApiResource(IdentityServerConstants.LocalApi.ScopeName)
            };
        }

        internal static IdentityResource[] GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        internal static Client[] GetIdentityClients(
            IdentityServerConfig identityServerConfig)
        {
            return new[]
            {
                new Client
                {
                    ClientId = "VsCodeTest",
                    ClientSecrets = new[] {new Secret("bla".Sha256())},
                    AllowedGrantTypes = GrantTypes
                        .ResourceOwnerPasswordAndClientCredentials,
                    AllowedScopes = new List<string> {"testApi"},
                },
                new Client
                {
                    ClientId = "xamarin",
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    ClientSecrets =
                    {
                        new Secret(identityServerConfig.ClientSecret.Sha256())
                    },
                    RedirectUris = {"https://localhost:3000/#origamClientCallback/", 
                        "http://localhost:3000/#origamClientCallback/",
                        "http://localhost/xamarincallback"},
                    RequireConsent = false,
                    RequirePkce = true,
                    PostLogoutRedirectUris = identityServerConfig.PostLogoutRedirectUris,
                    // PostLogoutRedirectUris = { $"{clientsUrl["Xamarin"]}/Account/Redirecting" },
                    // AllowedCorsOrigins = { "http://eshopxamarin" },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.LocalApi.ScopeName,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                    },
                    AllowOfflineAccess = true,
                    AllowAccessTokensViaBrowser = true,
                    AccessTokenType = AccessTokenType.Reference
                },
            };
        }
    }
}