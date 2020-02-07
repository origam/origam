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
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using IdentityServer4;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Origam.Security.Common;
using Origam.Security.Identity;
using Origam.ServerCore.Authorization;
using Origam.ServerCore.Configuration;

namespace Origam.ServerCore
{
    public class Startup
    {
        private readonly StartUpConfiguration startUpConfiguration;
        private IConfiguration Configuration { get; }
        private readonly PasswordConfiguration passwordConfiguration;
        private readonly IdentityServerConfig identityServerConfig;
        private readonly UserLockoutConfig lockoutConfig;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            startUpConfiguration = new StartUpConfiguration(configuration);
            passwordConfiguration = new PasswordConfiguration(configuration);
            identityServerConfig = new IdentityServerConfig(configuration);
            lockoutConfig = new UserLockoutConfig(configuration);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IPersistedGrantStore, PersistedGrantStore>();
            var builder = services.AddMvc()
                .AddXmlSerializerFormatters()
                .AddNewtonsoftJson();
#if DEBUG
            builder.AddRazorRuntimeCompilation();
#endif
            if (lockoutConfig.AutoUnlockAfterSpecifiedTime)
            {
                services.Configure<IdentityOptions>(options =>
                {
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(lockoutConfig.LockoutTimeMinutes);
                    options.Lockout.MaxFailedAccessAttempts = lockoutConfig.MaxFailedAccessAttempts;
                });
            }

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = startUpConfiguration.PathToClientApp ?? ".";
            });
            services.AddScoped<IManager, CoreManagerAdapter>();
            services.AddSingleton<IMailService, MailService>();
            services.AddSingleton<SessionObjects, SessionObjects>();
            services.AddTransient<IUserStore<IOrigamUser>, UserStore>();
            services.AddSingleton<IPasswordHasher<IOrigamUser>, CorePasswordHasher>();
            services.AddScoped<SignInManager<IOrigamUser>>();
            services.AddScoped<IUserClaimsPrincipalFactory<IOrigamUser>, UserClaimsPrincipalFactory<IOrigamUser>>();
            services.AddScoped<UserManager<IOrigamUser>>();
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddIdentity<IOrigamUser, Role>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = passwordConfiguration.RequireDigit;
                options.Password.RequiredLength = passwordConfiguration.RequiredLength;
                options.Password.RequireNonAlphanumeric = passwordConfiguration.RequireNonAlphanumeric;
                options.Password.RequireUppercase = passwordConfiguration.RequireUppercase;
                options.Password.RequireLowercase = passwordConfiguration.RequireLowercase;
            });
            
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IPrincipal>(
                provider => provider.GetService<IHttpContextAccessor>().HttpContext?.User);
            services.Configure<UserConfig>(options => Configuration.GetSection("UserConfig").Bind(options));
            services.Configure<IdentityGuiConfig>(options => Configuration.GetSection("IdentityGuiConfig").Bind(options));
            services.Configure<UserLockoutConfig>(options => Configuration.GetSection("UserLockoutConfig").Bind(options));
            
            services.AddIdentityServer()
                .AddSigningCredential(new X509Certificate2(
                    identityServerConfig.PathToJwtCertificate,
                    identityServerConfig.PasswordForJwtCertificate))
                .AddInMemoryApiResources(Settings.GetIdentityApiResources())
                .AddInMemoryClients(Settings.GetIdentityClients(startUpConfiguration))
                .AddInMemoryIdentityResources(Settings.GetIdentityResources())
                .AddAspNetIdentity<IOrigamUser>();

            services.AddScoped<IProfileService, ProfileService>();
            services.AddMvc(options => options.EnableEndpointRouting = false);
            var authenticationBuilder = services
                .AddLocalApiAuthentication()
                .AddAuthentication();

            if (identityServerConfig.UseGoogleLogin)
            {
                authenticationBuilder.AddGoogle(options =>
                {
                    options.ClientId = identityServerConfig.GoogleClientId;
                    options.ClientSecret = identityServerConfig.GoogleClientSecret; 
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                });
            }
        }

        public void Configure(
            IApplicationBuilder app, IWebHostEnvironment env, 
            ILoggerFactory loggerFactory)
        {
            loggerFactory.AddLog4Net();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseIdentityServer();
            app.MapWhen(
                IsPublicUserApiRoute,
                apiBranch => {
                apiBranch.UseResponseBuffering();
                apiBranch.UseMiddleware<UserApiMiddleWare>();
            });
            app.MapWhen(IsRestrictedUserApiRoute, apiBranch =>
            {
                apiBranch.UseAuthentication();
                apiBranch.Use(async (context, next) =>
                {
                    // Authentication middleware doesn't short-circuit the request itself
                    // we must do that here.
                    if (!context.User.Identity.IsAuthenticated)
                    {
                        context.Response.StatusCode = 401;
                        return;
                    }
                    await next.Invoke();
                    });
                    apiBranch.UseResponseBuffering();
                    apiBranch.UseMiddleware<UserApiMiddleWare>();
                });
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseRequestLocalization(); 
            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller}/{action=Index}/{id?}");
            });
            app.UseSpa(spa => {});
            // add DI to origam, in order to be able to resolve IPrincipal from
            // https://davidpine.net/blog/principal-architecture-changes/
            // https://docs.microsoft.com/cs-cz/aspnet/core/migration/claimsprincipal-current?view=aspnetcore-3.0
            
            SecurityManager.SetDIServiceProvider(app.ApplicationServices);
            OrigamEngine.OrigamEngine.ConnectRuntime();
        }
        private bool IsRestrictedUserApiRoute(HttpContext context)
        {
            return startUpConfiguration
                .UserApiRestrictedRoutes
                .Any(route => context.Request.Path.ToString().StartsWith(route));
        }
        private bool IsPublicUserApiRoute(HttpContext context)
        {
            return startUpConfiguration
                .UserApiPublicRoutes
                .Any(route => context.Request.Path.ToString().StartsWith(route));
        }
        private bool IsReportRoute(HttpContext context)
        {
            return context.Request.Path.ToString()
                .StartsWith("/internalApi/Report");
        }
    }
}
