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

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Packaging;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using Origam.Server.Configuration;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Origam.Server;

public static class OpenIddictConfigManager
{
    private static readonly string InternalApiScope = "internal_api";

    public static async Task CreateOrUpdateAsync(
        IServiceProvider serviceProvider,
        OpenIddictConfig config
    )
    {
        using var scope = serviceProvider.CreateScope();
        var apps = scope.ServiceProvider.GetRequiredService<
            OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication>
        >();
        var scopes = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

        if (await scopes.FindByNameAsync(InternalApiScope) is null)
        {
            await scopes.CreateAsync(
                new OpenIddictScopeDescriptor
                {
                    Name = InternalApiScope,
                    Resources = { "internal_api" },
                }
            );
        }

        var applicationConfigs = config.ClientApplicationTemplates;
        // ===== serverClient (ROPC, confidential) =====
        await CreateOrUpdatreServerClient(applicationConfigs, apps);

        // ===== origamMobileClient (public, code+PKCE) =====
        await CreateOrUpdateMobileClient(applicationConfigs, apps);

        // ===== origamWebClient (public, code+PKCE) =====
        await CreateOrUpdateWebClient(applicationConfigs, apps);
    }

    private static async Task CreateOrUpdateWebClient(
        ClientApplicationTemplates config,
        IOpenIddictApplicationManager apps
    )
    {
        if (config.WebClient == null)
        {
            return;
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "origamWebClient",
            DisplayName = "Origam Web",
            ClientType = ClientTypes.Public,
            ConsentType = ConsentTypes.Implicit,
            Requirements = { Requirements.Features.ProofKeyForCodeExchange },
        };

        foreach (var u in config.WebClient.RedirectUris ?? Array.Empty<string>())
        {
            descriptor.RedirectUris.Add(new Uri(u));
        }

        foreach (var u in config.WebClient.PostLogoutRedirectUris ?? Array.Empty<string>())
        {
            descriptor.PostLogoutRedirectUris.Add(new Uri(u));
        }

        descriptor.Permissions.AddRange(
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
                Permissions.Prefixes.Scope + InternalApiScope,
            }
        );

        var result = await apps.FindByClientIdAsync("origamWebClient");
        if (result is not OpenIddictEntityFrameworkCoreApplication application)
        {
            await apps.CreateAsync(descriptor);
        }
        else
        {
            await apps.UpdateAsync(application, descriptor);
        }
    }

    private static async Task CreateOrUpdateMobileClient(
        ClientApplicationTemplates config,
        IOpenIddictApplicationManager apps
    )
    {
        if (config.MobileClient == null)
        {
            return;
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "origamMobileClient",
            DisplayName = "Origam Mobile",
            ClientType = ClientTypes.Public,
            ConsentType = ConsentTypes.Implicit, // no consent screen
            Requirements = { Requirements.Features.ProofKeyForCodeExchange }, // PKCE
        };

        foreach (var u in config.MobileClient.RedirectUris ?? Array.Empty<string>())
        {
            descriptor.RedirectUris.Add(new Uri(u));
        }

        foreach (var u in config.MobileClient.PostLogoutRedirectUris ?? Array.Empty<string>())
        {
            descriptor.PostLogoutRedirectUris.Add(new Uri(u));
        }

        descriptor.Permissions.AddRange(
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
                Permissions.Prefixes.Scope + InternalApiScope,
            }
        );

        var result = await apps.FindByClientIdAsync("origamMobileClient");
        if (result is not OpenIddictEntityFrameworkCoreApplication application)
        {
            await apps.CreateAsync(descriptor);
        }
        else
        {
            await apps.UpdateAsync(application, descriptor);
        }
    }

    private static async Task CreateOrUpdatreServerClient(
        ClientApplicationTemplates config,
        IOpenIddictApplicationManager apps
    )
    {
        if (config.ServerClient == null)
        {
            return;
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "serverClient",
            ClientSecret = config.ServerClient.ClientSecret, // OpenIddict hashes it automatically
            ClientType = ClientTypes.Confidential,
            Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.Password,
                Permissions.GrantTypes.RefreshToken,
                Permissions.Prefixes.Scope + "offline_access",
                Permissions.Prefixes.Scope + InternalApiScope,
            },
        };

        var result = await apps.FindByClientIdAsync("serverClient");
        if (result is not OpenIddictEntityFrameworkCoreApplication application)
        {
            await apps.CreateAsync(descriptor);
        }
        else
        {
            await apps.UpdateAsync(application, descriptor);
        }
    }
}
