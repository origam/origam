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
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using MoreLinq.Extensions;
using Origam.Server.Configuration;
using Origam.Server.Middleware;
using Origam.Service.Core;
using SoapCore;

namespace Origam.Server;

public static class IApplicationBuilderExtensions
{
    public static void UseCustomSpa(this IApplicationBuilder app, string pathToClientApp)
    {
        app.Use(
            (context, next) =>
            {
                if (context.GetEndpoint() != null)
                {
                    return next();
                }
                // If it's a GET to a non-file path, serve index.html (classic SPA fallback)
                if (
                    context.Request.Method == "GET"
                    && !Path.HasExtension(context.Request.Path.Value)
                    && !context.Request.Path.Value.StartsWith("/assets")
                )
                {
                    context.Request.Path = "/index.html";
                }
                return next();
            }
        );
        app.UseStaticFiles(
            new StaticFileOptions { FileProvider = new PhysicalFileProvider(pathToClientApp) }
        );
    }

    public static void UseCustomWebAppExtenders(
        this IApplicationBuilder app,
        IConfiguration configuration,
        StartUpConfiguration startUpConfiguration
    )
    {
        foreach (var controllerDllName in startUpConfiguration.ExtensionDlls)
        {
            var customControllerAssembly = Assembly.LoadFrom(controllerDllName);
            customControllerAssembly
                .GetTypes()
                .Where(type => typeof(IWebApplicationExtender).IsAssignableFrom(type))
                .Select(type => (IWebApplicationExtender)Activator.CreateInstance(type))
                .ForEach(extender => extender.Extend(app, configuration));
        }
    }

    public static void UseUserApi(
        this IApplicationBuilder app,
        StartUpConfiguration startUpConfiguration,
        OpenIddictConfig openIddictConfig
    )
    {
        app.MapWhen(
            context => IsPublicUserApiRoute(startUpConfiguration, context),
            publicBranch =>
            {
                publicBranch.UseUserApiAuthentication(openIddictConfig);
                publicBranch.UseMiddleware<UserApiMiddleware>();
            }
        );
        app.MapWhen(
            context => IsRestrictedUserApiRoute(startUpConfiguration, context),
            privateBranch =>
            {
                privateBranch.UseUserApiAuthentication(openIddictConfig);
                privateBranch.Use(
                    async (context, next) =>
                    {
                        // Authentication middleware doesn't short-circuit the request itself
                        // we must do that here.
                        if (!context.User.Identity.IsAuthenticated)
                        {
                            context.Response.StatusCode = 401;
                            return;
                        }
                        await next.Invoke();
                    }
                );
                privateBranch.UseMiddleware<UserApiMiddleware>();
            }
        );
    }

    private static void UseUserApiAuthentication(
        this IApplicationBuilder app,
        OpenIddictConfig openIddictConfig
    )
    {
        if (openIddictConfig.PrivateApiAuthentication == AuthenticationMethod.Token)
        {
            app.UseMiddleware<UserApiTokenAuthenticationMiddleware>();
        }
        else
        {
            app.UseAuthentication();
        }
    }

    public static void UseWorkQueueApi(this IApplicationBuilder app)
    {
        app.MapWhen(
            context => context.Request.Path.ToString().StartsWith("/workQueue"),
            apiBranch =>
            {
                apiBranch.UseMiddleware<UserApiTokenAuthenticationMiddleware>();
                apiBranch.UseRouting();
                apiBranch.UseAuthentication();
                apiBranch.UseAuthorization();

                apiBranch.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(
                        name: "workQueue",
                        pattern: "{controller=WorkQueue}/{action=Index}/{id?}"
                    );
                });
            }
        );
    }

    private static bool IsRestrictedUserApiRoute(
        StartUpConfiguration startUpConfiguration,
        HttpContext context
    )
    {
        return startUpConfiguration.UserApiRestrictedRoutes.Any(route =>
            context.Request.Path.ToString().StartsWith(route)
        );
    }

    private static bool IsPublicUserApiRoute(
        StartUpConfiguration startUpConfiguration,
        HttpContext context
    )
    {
        return startUpConfiguration.UserApiPublicRoutes.Any(route =>
            context.Request.Path.ToString().StartsWith(route)
        );
    }

    public static void UseSoapApi(
        this IApplicationBuilder app,
        bool authenticationRequired,
        bool expectAndReturnOldDotNetAssemblyReferences
    )
    {
        app.MapWhen(
            IsSoapApiRoute,
            apiBranch =>
            {
                apiBranch.Use(
                    async (context, next) =>
                    {
                        // Authentication middleware doesn't short-circuit the request itself
                        // we must do that here.
                        if (authenticationRequired && !context.User.Identity.IsAuthenticated)
                        {
                            context.Response.StatusCode = 401;
                            return;
                        }
                        await next.Invoke();
                    }
                );
                if (expectAndReturnOldDotNetAssemblyReferences)
                {
                    apiBranch.UseMiddleware<ReturnOldDotNetAssemblyReferencesInSoapMiddleware>();
                }
                apiBranch.UseSoapEndpoint<DataServiceSoap>(
                    "/soap/DataService",
                    new SoapEncoderOptions(),
                    SoapSerializer.XmlSerializer
                );
                apiBranch.UseSoapEndpoint<WorkflowServiceSoap>(
                    "/soap/WorkflowService",
                    new SoapEncoderOptions(),
                    SoapSerializer.XmlSerializer
                );
            }
        );
    }

    private static bool IsSoapApiRoute(HttpContext context)
    {
        return context.Request.Path.ToString().StartsWith("/soap");
    }
}
