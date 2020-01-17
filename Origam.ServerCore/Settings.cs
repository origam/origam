using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;

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

        internal static Client[] GetIdentityClients()
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
                    ClientName = "eShop Xamarin OpenId Client",
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    ClientSecrets =
                    {
                        new Secret("bla".Sha256())
                    },
                    RedirectUris = {"http://192.168.0.80:45455/xamarincallback"},
                    RequireConsent = false,
                    RequirePkce = true,
                    // PostLogoutRedirectUris = { $"{clientsUrl["Xamarin"]}/Account/Redirecting" },
                    // AllowedCorsOrigins = { "http://eshopxamarin" },
                    AllowedScopes = new List<string>
                    {
                        "openid", "profile",
                        IdentityServerConstants.LocalApi.ScopeName
                    },
                    AllowOfflineAccess = true,
                    AllowAccessTokensViaBrowser = true
                },
            };
        }
    }
}