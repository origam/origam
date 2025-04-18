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
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using MoreLinq;
using Origam.Security.Common;
using Origam.Security.Identity;
using Origam.Server.Authorization;
using Origam.Server.ClientAuthentication;
using Origam.Server.Configuration;
using Origam.Server.Middleware;
using Origam.Service.Core;
using SoapCore;


namespace Origam.Server;
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
        ServicePointManager.SecurityProtocol 
            = startUpConfiguration.SecurityProtocol;
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
        // remove limit for multipart body length
        services.Configure<FormOptions>(options =>
        {
            options.ValueLengthLimit = 
                startUpConfiguration.ValueLengthLimit;
            options.MultipartBodyLengthLimit = 
                startUpConfiguration.MultipartBodyLengthLimit;
            options.MultipartHeadersLengthLimit =
                startUpConfiguration.MultipartHeadersLengthLimit;
        });
        services.AddSingleton<IPersistedGrantStore, PersistedGrantStore>();
        var builder = services.AddMvc().AddNewtonsoftJson();
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
        services.AddScoped<CoreUserManager<IOrigamUser>, CoreUserManager<IOrigamUser>>();
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
            var userConfig = new UserConfig();
            Configuration.GetSection("UserConfig").Bind(userConfig);
            if (!string.IsNullOrEmpty(userConfig.AllowedUserNameCharacters))
            {
                options.User.AllowedUserNameCharacters = userConfig.AllowedUserNameCharacters;                    
            }
        });
        
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddTransient<IPrincipal>(provider 
            => provider.GetService<IHttpContextAccessor>()
                .HttpContext?.User);
        services.Configure<UserConfig>(options 
            => Configuration.GetSection("UserConfig").Bind(options));
        services.Configure<ClientFilteringConfig>(options 
            => Configuration.GetSection("ClientFilteringConfig")
                .Bind(options));
        services.Configure<IdentityGuiConfig>(options 
            => Configuration.GetSection("IdentityGuiConfig")
                .Bind(options));
        services.Configure<CustomAssetsConfig>(options 
            => Configuration.GetSection("CustomAssetsConfig")
                .Bind(options));
        services.Configure<UserLockoutConfig>(options 
            => Configuration.GetSection("UserLockoutConfig")
                .Bind(options));
        services.Configure<ChatConfig>(options 
            => Configuration.GetSection("ChatConfig").Bind(options));
        services.Configure<HtmlClientConfig>(options 
            => Configuration.GetSection("HtmlClientConfig")
                .Bind(options));
        services.AddIdentityServer()
            .AddInMemoryApiResources(Settings.GetIdentityApiResources())
            .AddInMemoryClients(Settings.GetIdentityClients(identityServerConfig))
            .AddInMemoryIdentityResources(Settings.GetIdentityResources())
            .AddAspNetIdentity<IOrigamUser>()
            .AddSigningCredential(new X509Certificate2(
                identityServerConfig.PathToJwtCertificate,
                identityServerConfig.PasswordForJwtCertificate))
            .AddInMemoryApiScopes(Settings.GetApiScopes())
            .AddResourceOwnerValidator<OrigamResourceOwnerPasswordValidator>();
        
        if (identityServerConfig.PrivateApiAuthentication == AuthenticationMethod.Cookie)
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.ExpireTimeSpan = 
                    TimeSpan.FromMinutes(identityServerConfig.CookieExpirationMinutes);
                options.SlidingExpiration = 
                    identityServerConfig.CookieSlidingExpiration;
            });
        }
        services.AddSoapCore();
        services.AddSingleton<DataServiceSoap>();
        services.AddSingleton<WorkflowServiceSoap>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddMvc(options => options.EnableEndpointRouting = false)
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization(options => {
                options.DataAnnotationLocalizerProvider = (type, factory) =>
                    factory.Create(typeof(SharedResources));
            });
        
        ConfigureAuthentication(services);
        
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = languageConfig.DefaultCulture;
            options.SupportedCultures = languageConfig.AllowedCultures;
            options.SupportedUICultures = languageConfig.AllowedCultures;
            options.RequestCultureProviders.Clear();
            options.RequestCultureProviders.Insert(0, 
                new OrigamCookieRequestCultureProvider(languageConfig));
        });
        foreach (var controllerDllName in startUpConfiguration.ExtensionDlls)
        {
            var customControllerAssembly = Assembly.LoadFrom(
                controllerDllName);
            services.AddControllers()
                .AddApplicationPart(customControllerAssembly);
        }
        var providerFactory = 
            LoadClientAuthenticationProviders(Configuration, startUpConfiguration);
        services.AddSingleton(providerFactory);
    }
    private static ClientAuthenticationProviderContainer LoadClientAuthenticationProviders( 
        IConfiguration configuration,  StartUpConfiguration startUpConfiguration)
    {
        var providerFactory = new ClientAuthenticationProviderContainer();
        foreach (var providerDllName in startUpConfiguration.ExtensionDlls)
        {
            var providerAssembly = Assembly.LoadFrom(providerDllName);
            providerAssembly
                .GetTypes()
                .Where(type => typeof(IClientAuthenticationProvider).IsAssignableFrom(type))
                .Select(type => (IClientAuthenticationProvider)Activator.CreateInstance(type))
                .Append(new ResourceOwnerPasswordAuthenticationProvider())
                .ForEach(provider =>
                {
                    provider.Configure(configuration);
                    providerFactory.Register(provider);
                });
        }
        return providerFactory;
    }  
    private void ConfigureAuthentication(IServiceCollection services)
    {
        var authenticationBuilder = services
            .AddLocalApiAuthentication()
            .AddAuthentication();
        if(identityServerConfig.GoogleLogin != null)
        {
            authenticationBuilder.AddGoogle(
                GoogleDefaults.AuthenticationScheme,
                "SignInWithGoogleAccount",
                options =>
                {
                    options.ClientId =
                        identityServerConfig.GoogleLogin.ClientId;
                    options.ClientSecret = identityServerConfig.GoogleLogin
                        .ClientSecret;
                    options.SignInScheme = IdentityServer4
                        .IdentityServerConstants
                        .ExternalCookieAuthenticationScheme;
                });
        }
        if(identityServerConfig.MicrosoftLogin != null)
        {
            authenticationBuilder.AddMicrosoftAccount(
                MicrosoftAccountDefaults.AuthenticationScheme,
                "SignInWithMicrosoftAccount",
                microsoftOptions =>
                {
                    microsoftOptions.ClientId = identityServerConfig
                        .MicrosoftLogin.ClientId;
                    microsoftOptions.ClientSecret = identityServerConfig
                        .MicrosoftLogin.ClientSecret;
                    microsoftOptions.SignInScheme = IdentityServer4
                        .IdentityServerConstants
                        .ExternalCookieAuthenticationScheme;
                });
        }
        if(identityServerConfig.AzureAdLogin != null)
        {
            authenticationBuilder.AddOpenIdConnect(
                IdentityServerDefaults.AzureAdScheme,
                "SignInWithAzureAd",
                options =>
                {
                    options.ClientId = identityServerConfig.AzureAdLogin
                        .ClientId;
                    options.Authority
                        = $@"https://login.microsoftonline.com/{identityServerConfig.AzureAdLogin.TenantId}/";
                    options.CallbackPath = "/signin-oidc";
                    options.SaveTokens = true;
                    options.SignInScheme = IdentityServer4
                        .IdentityServerConstants
                        .ExternalCookieAuthenticationScheme;
                    options.TokenValidationParameters
                        = new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidAudience =
                                $"{identityServerConfig.AzureAdLogin.ClientId}"
                        };
                });
        }
        // ExternalController needs to tap the info for the callback
        // resolution
        services.AddSingleton(identityServerConfig);
        // Setup authentication post processor
        if (string.IsNullOrEmpty(
               identityServerConfig.AuthenticationPostProcessor))
        {
            services.AddSingleton<IAuthenticationPostProcessor, 
                    AlwaysValidAuthenticationPostProcessor>();
        }
        else
        {
            var classpath = identityServerConfig
                .AuthenticationPostProcessor.Split(',');
            var authenticationPostProcessor 
                = Reflector.ResolveTypeFromAssembly(
                    classpath[0], classpath[1]);
            services.AddSingleton(typeof(IAuthenticationPostProcessor),
                authenticationPostProcessor);
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
            app.UseExceptionHandler("/Error/Error");
            app.UseHsts();
        }
        if (Configuration.GetValue<bool>("BehindProxy"))
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders 
                    = ForwardedHeaders.XForwardedProto 
                    | ForwardedHeaders.XForwardedHost
            });
        }
        var localizationOptions = app.ApplicationServices
            .GetService<IOptions<RequestLocalizationOptions>>().Value;
        app.UseRequestLocalization(localizationOptions);
        app.UseIdentityServer();
        app.UseMiddleware<FatalErrorMiddleware>();
        app.UseUserApi(startUpConfiguration, identityServerConfig);
        app.UseWorkQueueApi();
        app.UseAuthentication();
        app.UseHttpsRedirection();
        if (startUpConfiguration.EnableSoapInterface)
        {
            app.UseSoapApi(
                startUpConfiguration.SoapInterfaceRequiresAuthentication,
                startUpConfiguration.ExpectAndReturnOldDotNetAssemblyReferences);
        }
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
        app.UseCustomWebAppExtenders(Configuration, startUpConfiguration);
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
        HttpTools.SetDIServiceProvider(app.ApplicationServices);
        OrigamUtils.ConnectOrigamRuntime(loggerFactory, startUpConfiguration.ReloadModelWhenFilesChangesDetected);
        OrigamUtils.CleanUpDatabase();
    }
}
