using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Packaging;
using OpenIddict.Abstractions;
using Origam.Server.Configuration;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Origam.Server;

public static class OpenIddictSeeder
{
    public static async Task SeedAsync(IServiceProvider sp, IdentityServerConfig cfg)
    {
        using var scope = sp.CreateScope();
        var apps = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var scopes = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

        // Keep the old scope name for minimum churn (IdentityServerConstants.LocalApi.ScopeName == "local_api")
        // Or rename everywhere to "internal_api" if you already switched your policy.
        const string localApiScope = "local_api";
        const string internalApiScope = "internal_api";

        if (await scopes.FindByNameAsync(localApiScope) is null)
        {
            await scopes.CreateAsync(
                new OpenIddictScopeDescriptor
                {
                    Name = localApiScope,
                    Resources = { "origam-local" },
                }
            );
        }
        if (await scopes.FindByNameAsync(internalApiScope) is null)
        {
            await scopes.CreateAsync(
                new OpenIddictScopeDescriptor
                {
                    Name = internalApiScope,
                    Resources = { "internal_api" },
                }
            );
        }

        // ===== serverClient (ROPC, confidential) =====
        if (cfg.ServerClient != null && await apps.FindByClientIdAsync("serverClient") is null)
        {
            await apps.CreateAsync(
                new OpenIddictApplicationDescriptor
                {
                    ClientId = "serverClient",
                    ClientSecret = cfg.ServerClient.ClientSecret, // OpenIddict hashes it automatically
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.Password,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.Prefixes.Scope + "offline_access",
                        Permissions.Prefixes.Scope + localApiScope,
                        Permissions.Prefixes.Scope + internalApiScope,
                    },
                }
            );
        }

        // ===== origamMobileClient (public, code+PKCE) =====
        if (
            cfg.MobileClient != null
            && await apps.FindByClientIdAsync("origamMobileClient") is null
        )
        {
            var d = new OpenIddictApplicationDescriptor
            {
                ClientId = "origamMobileClient",
                DisplayName = "Origam Mobile",
                ConsentType = ConsentTypes.Implicit, // no consent screen
                Requirements = { Requirements.Features.ProofKeyForCodeExchange }, // PKCE
            };

            foreach (var u in cfg.MobileClient.RedirectUris ?? Array.Empty<string>())
            {
                d.RedirectUris.Add(new Uri(u));
            }

            foreach (var u in cfg.MobileClient.PostLogoutRedirectUris ?? Array.Empty<string>())
            {
                d.PostLogoutRedirectUris.Add(new Uri(u));
            }

            d.Permissions.AddRange(
                new[]
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.EndSession,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.Prefixes.Scope + "openid",
                    Permissions.Scopes.Profile,
                    Permissions.Prefixes.Scope + "offline_access",
                    Permissions.Prefixes.Scope + localApiScope,
                    Permissions.Prefixes.Scope + internalApiScope,
                }
            );

            await apps.CreateAsync(d);
        }

        // ===== origamWebClient (public, code+PKCE) =====
        if (cfg.WebClient != null && await apps.FindByClientIdAsync("origamWebClient") is null)
        {
            var d = new OpenIddictApplicationDescriptor
            {
                ClientId = "origamWebClient",
                DisplayName = "Origam Web",
                ConsentType = ConsentTypes.Implicit,
                Requirements = { Requirements.Features.ProofKeyForCodeExchange },
            };

            foreach (var u in cfg.WebClient.RedirectUris ?? Array.Empty<string>())
            {
                d.RedirectUris.Add(new Uri(u));
            }

            foreach (var u in cfg.WebClient.PostLogoutRedirectUris ?? Array.Empty<string>())
            {
                d.PostLogoutRedirectUris.Add(new Uri(u));
            }

            d.Permissions.AddRange(
                new[]
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.EndSession,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.Prefixes.Scope + "openid",
                    Permissions.Scopes.Profile,
                    Permissions.Prefixes.Scope + "offline_access",
                    Permissions.Prefixes.Scope + localApiScope,
                    Permissions.Prefixes.Scope + internalApiScope,
                }
            );

            await apps.CreateAsync(d);
        }
    }
}
