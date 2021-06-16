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
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using IdentityServer4;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Origam.Security.Common;
using Origam.Security.Identity;
using Origam.ServerCore.Authorization;
using Origam.ServerCore.Configuration;
using Origam.ServerCore.Resources;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Origam.ServerCore.Middleware;

namespace Origam.ServerCore
{
    public class Startup
    {
        private readonly StartUpConfiguration startUpConfiguration;
        private IConfiguration Configuration { get; }
        private readonly PasswordConfiguration passwordConfiguration;
        private readonly IdentityServerConfig identityServerConfig;
        private readonly UserLockoutConfig lockoutConfig;
        private readonly LanguageConfig languageConfig;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            startUpConfiguration = new StartUpConfiguration(configuration);
            passwordConfiguration = new PasswordConfiguration(configuration);
            identityServerConfig = new IdentityServerConfig(configuration);
            lockoutConfig = new UserLockoutConfig(configuration);
            languageConfig = new LanguageConfig(configuration);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // If using IIS:
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
                options.AuthenticationDisplayName = "Windows";
                options.AutomaticAuthentication = true;
            });

            services.AddSingleton<IPersistedGrantStore, PersistedGrantStore>();
            var builder = services.AddMvc()
                .AddNewtonsoftJson();
#if DEBUG
            builder.AddRazorRuntimeCompilation();
#endif
            services.Configure<IdentityOptions>(options =>
            {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(lockoutConfig.LockoutTimeMinutes);
                options.Lockout.MaxFailedAccessAttempts = lockoutConfig.MaxFailedAccessAttempts;
            });
            services.TryAddScoped<ILookupNormalizer, Authorization.UpperInvariantLookupNormalizer>();
            services.AddScoped<IManager, CoreManagerAdapter>();
            services.AddSingleton<IMailService, MailService>();
            services.AddSingleton<SearchHandler>();
            services.AddSingleton<SessionObjects, SessionObjects>();
            services.AddSingleton<IPasswordHasher<IOrigamUser>, CorePasswordHasher>();
            services.AddScoped<SignInManager<IOrigamUser>>();
            services.AddScoped<IUserClaimsPrincipalFactory<IOrigamUser>, UserClaimsPrincipalFactory<IOrigamUser>>();
            services.AddScoped<UserManager<IOrigamUser>, CoreUserManager>();
            services.AddTransient<IUserStore<IOrigamUser>, UserStore>();
            services.AddSingleton<LanguageConfig>();
            services.AddLocalization();
            services.AddIdentity<IOrigamUser, Role>()
                .AddDefaultTokenProviders()
                .AddErrorDescriber<MultiLanguageIdentityErrorDescriber>();

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
            services.Configure<ClientSortingAndFilteringConfig>(options => Configuration.GetSection("ClientSortingAndFilteringConfig").Bind(options));
            services.Configure<IdentityGuiConfig>(options => Configuration.GetSection("IdentityGuiConfig").Bind(options));
            services.Configure<CustomAssetsConfig>(options => Configuration.GetSection("CustomAssetsConfig").Bind(options));
            services.Configure<UserLockoutConfig>(options => Configuration.GetSection("UserLockoutConfig").Bind(options));
            services.Configure<ChatConfig>(options => Configuration.GetSection("ChatConfig").Bind(options));
            services.Configure<HtmlClientConfig>(options => Configuration.GetSection("HtmlClientConfig").Bind(options));

           services.AddIdentityServer()
                .AddInMemoryApiResources(Settings.GetIdentityApiResources())
                .AddInMemoryClients(Settings.GetIdentityClients(identityServerConfig))
                .AddInMemoryIdentityResources(Settings.GetIdentityResources())
                .AddAspNetIdentity<IOrigamUser>()
                .AddSigningCredential(new X509Certificate2(
                    identityServerConfig.PathToJwtCertificate,
                    identityServerConfig.PasswordForJwtCertificate))
                .AddInMemoryApiScopes(Settings.GetApiScopes());
            
            services.AddScoped<IProfileService, ProfileService>();
            services.AddMvc(options => options.EnableEndpointRouting = false)
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization(options => {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(SharedResources));
                });
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
            
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = languageConfig.DefaultCulture;
                options.SupportedCultures = languageConfig.AllowedCultures;
                options.SupportedUICultures = languageConfig.AllowedCultures;
                options.RequestCultureProviders.Clear();
                options.RequestCultureProviders.Insert(0, 
                    new OrigamCookieRequestCultureProvider(languageConfig));
            });
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
                app.UseExceptionHandler("/Error/Error");
                app.UseHsts();
            }
            if (Configuration.GetValue<bool>("BehindProxy") == true)
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedProto
                });
            }
            var localizationOptions = app.ApplicationServices
                .GetService<IOptions<RequestLocalizationOptions>>().Value;
            app.UseRequestLocalization(localizationOptions);
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
            app.UseStaticFiles(new StaticFileOptions() {
                FileProvider =  new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "assets")),
                RequestPath = new PathString("/assets")
            });
            if (startUpConfiguration.HasCustomAssets)
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(startUpConfiguration.PathToCustomAssetsFolder),
                    RequestPath = new PathString(startUpConfiguration.RouteToCustomAssetsFolder)
                });                
            }

            if(!string.IsNullOrEmpty(startUpConfiguration.PathToChatApp))
            {
                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider(startUpConfiguration.PathToChatApp),
                    RequestPath = new PathString("/chatrooms")
                });
            }
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(startUpConfiguration.PathToClientApp)
            });
            app.UseCors(builder => 
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller}/{action=Index}/{id?}");
            });
            app.UseCustomSpa(startUpConfiguration.PathToClientApp);
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
    }
}
