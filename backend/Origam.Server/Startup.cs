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
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Origam.Security.Common;
using Origam.Security.Identity;
using Origam.Server.Authorization;
using Origam.Server.ClientAuthentication;
using Origam.Server.Configuration;
using Origam.Server.Middleware;
using Origam.Service.Core;
using SoapCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults;

namespace Origam.Server;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.UseOpenIddict();
    }
}

public class Startup
{
    private readonly StartUpConfiguration startUpConfiguration;
    private IConfiguration Configuration { get; }
    private readonly PasswordConfiguration passwordConfiguration;
    private readonly OpenIddictConfig openIddictConfig;
    private readonly UserLockoutConfig lockoutConfig;
    private readonly LanguageConfig languageConfig;
    private readonly ChatConfig chatConfig;

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        startUpConfiguration = new StartUpConfiguration(configuration);
        passwordConfiguration = new PasswordConfiguration(configuration);
        openIddictConfig = new OpenIddictConfig(configuration);
        lockoutConfig = new UserLockoutConfig(configuration);
        languageConfig = new LanguageConfig(configuration);
        chatConfig = new ChatConfig(configuration);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        ServicePointManager.SecurityProtocol = startUpConfiguration.SecurityProtocol;
        services.AddSingleton(startUpConfiguration);
        services.Configure<KestrelServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
        });
        services.Configure<IISServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
            options.AuthenticationDisplayName = "Windows";
            options.AutomaticAuthentication = true;
        });

        services.Configure<FormOptions>(options =>
        {
            options.ValueLengthLimit = startUpConfiguration.ValueLengthLimit;
            options.MultipartBodyLengthLimit = startUpConfiguration.MultipartBodyLengthLimit;
            options.MultipartHeadersLengthLimit = startUpConfiguration.MultipartHeadersLengthLimit;
        });

        services.Configure<IdentityOptions>(options =>
        {
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(
                lockoutConfig.LockoutTimeMinutes
            );
            options.Lockout.MaxFailedAccessAttempts = lockoutConfig.MaxFailedAccessAttempts;
            options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
            options.ClaimsIdentity.UserNameClaimType = Claims.Name;
            options.ClaimsIdentity.RoleClaimType = Claims.Role;
        });

        services.TryAddScoped<ILookupNormalizer, Authorization.UpperInvariantLookupNormalizer>();
        services.AddScoped<IManager, CoreManagerAdapter>();
        services.AddSingleton<IMailService, MailService>();
        services.AddSingleton<SearchHandler>();
        services.AddSingleton<SessionObjects, SessionObjects>();

        services.AddSingleton<IPasswordHasher<IOrigamUser>, CorePasswordHasher>();
        services.AddScoped<SignInManager<IOrigamUser>>();
        services.AddScoped<IUserClaimsPrincipalFactory<IOrigamUser>, ClaimsFactory>();
        services.AddScoped<CoreUserManager<IOrigamUser>, CoreUserManager<IOrigamUser>>();
        services.AddTransient<IUserStore<IOrigamUser>, UserStore>();

        services.AddSingleton<LanguageConfig>();
        services.AddLocalization();

        services
            .AddIdentity<IOrigamUser, Role>()
            .AddDefaultTokenProviders()
            .AddErrorDescriber<MultiLanguageIdentityErrorDescriber>();

        services.Configure<RazorViewEngineOptions>(opts =>
        {
            opts.ViewLocationFormats.Add("/Identity/Views/{1}/{0}.cshtml");
            opts.ViewLocationFormats.Add("/Identity/Views/Shared/{0}.cshtml");
        });

        services.ConfigureApplicationCookie(o =>
        {
            o.LoginPath = "/Account/Login";
            o.LogoutPath = "/Account/Logout";
            o.AccessDeniedPath = "/Account/AccessDenied";
            o.ExpireTimeSpan = TimeSpan.FromMinutes(openIddictConfig.CookieExpirationMinutes);
            o.SlidingExpiration = openIddictConfig.CookieSlidingExpiration;
        });

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
        services.AddTransient<IPrincipal>(provider =>
            provider.GetService<IHttpContextAccessor>().HttpContext?.User
        );

        services.Configure<UserConfig>(options =>
            Configuration.GetSection("UserConfig").Bind(options)
        );
        services.Configure<ClientFilteringConfig>(options =>
            Configuration.GetSection("ClientFilteringConfig").Bind(options)
        );
        services.Configure<IdentityGuiConfig>(options =>
            Configuration.GetSection("IdentityGuiConfig").Bind(options)
        );
        services.Configure<CustomAssetsConfig>(options =>
            Configuration.GetSection("CustomAssetsConfig").Bind(options)
        );
        services.Configure<UserLockoutConfig>(options =>
            Configuration.GetSection("UserLockoutConfig").Bind(options)
        );
        services.Configure<ChatConfig>(options =>
            Configuration.GetSection("ChatConfig").Bind(options)
        );
        services.Configure<HtmlClientConfig>(options =>
            Configuration.GetSection("HtmlClientConfig").Bind(options)
        );

        services.AddDbContext<AuthDbContext>(opts =>
        {
            string connectionString = ConfigurationManager
                .GetActiveConfiguration()
                .DataConnectionString;
            opts.UseSqlServer(connectionString);
            opts.UseOpenIddict();
        });

        services
            .AddOpenIddict()
            .AddCore(opt =>
            {
                opt.UseEntityFrameworkCore().UseDbContext<AuthDbContext>();
            })
            .AddServer(opt =>
            {
                opt.SetAuthorizationEndpointUris("/connect/authorize")
                    .SetTokenEndpointUris("/connect/token")
                    .SetIntrospectionEndpointUris("/connect/introspect")
                    .SetEndSessionEndpointUris("/connect/logout");
                opt.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();
                opt.AllowPasswordFlow();
                opt.AllowRefreshTokenFlow();
                opt.AllowClientCredentialsFlow();
                opt.SetAccessTokenLifetime(TimeSpan.FromHours(1));
                opt.SetRefreshTokenLifetime(TimeSpan.FromDays(30));
                opt.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough();
                opt.AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate();
                opt.RegisterScopes(
                    Scopes.OpenId,
                    Scopes.Profile,
                    Scopes.OfflineAccess,
                    "internal_api",
                    "local_api"
                );
            })
            .AddValidation(opt =>
            {
                opt.UseLocalServer();
                opt.UseAspNetCore();
                opt.Configure(o =>
                {
                    o.TokenValidationParameters.NameClaimType = Claims.Name;
                    o.TokenValidationParameters.RoleClaimType = Claims.Role;
                });
            });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme; // cookie
            options.DefaultSignInScheme = IdentityConstants.ApplicationScheme; // cookie
            options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme; // cookie
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                "InternalApi",
                policy =>
                {
                    policy.AddAuthenticationSchemes(AuthenticationScheme);
                    policy.RequireAuthenticatedUser();

                    policy.RequireAssertion(ctx =>
                    {
                        var scopes = ctx
                            .User.FindAll(Claims.Scope)
                            .SelectMany(c =>
                                c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            );

                        return scopes.Contains("internal_api");
                    });
                }
            );
        });

        services.AddSoapCore();
        services.AddSingleton<DataServiceSoap>();
        services.AddSingleton<WorkflowServiceSoap>();

        services
            .AddControllersWithViews()
            .AddNewtonsoftJson()
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization(options =>
            {
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
            options.RequestCultureProviders.Insert(
                0,
                new OrigamCookieRequestCultureProvider(languageConfig)
            );
        });

        foreach (var controllerDllName in startUpConfiguration.ExtensionDlls)
        {
            var customControllerAssembly = Assembly.LoadFrom(controllerDllName);
            services.AddControllers().AddApplicationPart(customControllerAssembly);
        }

        var providerFactory = LoadClientAuthenticationProviders(
            Configuration,
            startUpConfiguration
        );
        services.AddSingleton(providerFactory);

        if (startUpConfiguration.EnableMiniProfiler)
        {
            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";
                options.PopupDecimalPlaces = 1;
                options.ResultsAuthorize = request =>
                    SecurityManager
                        .GetAuthorizationProvider()
                        .Authorize(SecurityManager.CurrentPrincipal, "SYS_ViewMiniProfilerResults");
            });
        }

        var allowedCorsOrigins = openIddictConfig.ClientApplicationTemplates.WebClient.AllowedCorsOrigins;
        services.AddCors(options =>
        {
            options.AddPolicy(
                "OrigamCorsPolicy",
                builder =>
                {
                    builder
                        .WithOrigins(allowedCorsOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); // required for cookie auth and OIDC
                }
            );
        });
    }

    private static ClientAuthenticationProviderContainer LoadClientAuthenticationProviders(
        IConfiguration configuration,
        StartUpConfiguration startUpConfiguration
    )
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
                .ToList()
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
        var auth = services.AddAuthentication();

        if (openIddictConfig.GoogleLogin != null)
        {
            auth.AddGoogle(
                GoogleDefaults.AuthenticationScheme,
                "SignInWithGoogleAccount",
                options =>
                {
                    options.ClientId = openIddictConfig.GoogleLogin.ClientId;
                    options.ClientSecret = openIddictConfig.GoogleLogin.ClientSecret;
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                }
            );
        }

        if (openIddictConfig.MicrosoftLogin != null)
        {
            auth.AddMicrosoftAccount(
                MicrosoftAccountDefaults.AuthenticationScheme,
                "SignInWithMicrosoftAccount",
                microsoftOptions =>
                {
                    microsoftOptions.ClientId = openIddictConfig.MicrosoftLogin.ClientId;
                    microsoftOptions.ClientSecret = openIddictConfig.MicrosoftLogin.ClientSecret;
                    microsoftOptions.SignInScheme = IdentityConstants.ExternalScheme;
                }
            );
        }

        if (openIddictConfig.AzureAdLogin != null)
        {
            auth.AddOpenIdConnect(
                "AzureAdOIDC",
                "SignInWithAzureAd",
                options =>
                {
                    options.ClientId = openIddictConfig.AzureAdLogin.ClientId;
                    options.Authority =
                        $@"https://login.microsoftonline.com/{openIddictConfig.AzureAdLogin.TenantId}/";
                    options.CallbackPath = "/signin-oidc";
                    options.SaveTokens = true;
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidAudience = openIddictConfig.AzureAdLogin.ClientId,
                    };
                }
            );
        }

        services.AddSingleton(openIddictConfig);

        if (string.IsNullOrEmpty(openIddictConfig.AuthenticationPostProcessor))
        {
            services.AddSingleton<
                IAuthenticationPostProcessor,
                AlwaysValidAuthenticationPostProcessor
            >();
        }
        else
        {
            var classpath = openIddictConfig.AuthenticationPostProcessor.Split(',');
            var authenticationPostProcessor = Reflector.ResolveTypeFromAssembly(
                classpath[0],
                classpath[1]
            );
            services.AddSingleton(
                typeof(IAuthenticationPostProcessor),
                authenticationPostProcessor
            );
        }
    }

    public void Configure(
        IApplicationBuilder app,
        IWebHostEnvironment env,
        ILoggerFactory loggerFactory
    )
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
            var forwardedHeadersOptions = new ForwardedHeadersOptions()
            {
                ForwardedHeaders =
                    ForwardedHeaders.XForwardedProto
                    | ForwardedHeaders.XForwardedHost
                    | ForwardedHeaders.XForwardedFor,
            };
            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeadersOptions);
        }

        var localizationOptions = app
            .ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>()
            .Value;
        app.UseRequestLocalization(localizationOptions);

        app.UseMiddleware<FatalErrorMiddleware>();

        app.UseUserApi(startUpConfiguration, openIddictConfig);
        app.UseWorkQueueApi();

        app.UseRouting();
        app.UseCors("OrigamCorsPolicy");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            // conventional routes (lets /Account/Login hit AccountController.Login)
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );
        });

        app.UseHttpsRedirection();

        if (startUpConfiguration.EnableSoapInterface)
        {
            app.UseSoapApi(
                startUpConfiguration.SoapInterfaceRequiresAuthentication,
                startUpConfiguration.ExpectAndReturnOldDotNetAssemblyReferences
            );
        }

        app.UseStaticFiles(
            new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "assets")
                ),
                RequestPath = new PathString("/assets"),
            }
        );

        if (startUpConfiguration.HasCustomAssets)
        {
            app.UseStaticFiles(
                new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                        startUpConfiguration.PathToCustomAssetsFolder
                    ),
                    RequestPath = new PathString(startUpConfiguration.RouteToCustomAssetsFolder),
                }
            );
        }

        app.UseCustomWebAppExtenders(Configuration, startUpConfiguration);

        app.UseStaticFiles(
            new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(startUpConfiguration.PathToClientApp),
            }
        );

        if (!string.IsNullOrWhiteSpace(chatConfig.PathToChatApp))
        {
            app.UseStaticFiles(
                new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(chatConfig.PathToChatApp!),
                    RequestPath = new PathString("/chatrooms"),
                    OnPrepareResponse = ctx =>
                    {
                        if (ctx.File.Name == "index.html")
                        {
                            ctx.Context.Response.Headers.Append(
                                "Cache-Control",
                                $"no-store, max-age=0"
                            );
                        }
                    },
                }
            );
            app.UseStaticFiles(
                new StaticFileOptions
                {
                    RequestPath = new PathString("/chatAssets"),
                    FileProvider = new PhysicalFileProvider(
                        Path.Combine(chatConfig.PathToChatApp, "chatAssets")
                    ),
                }
            );
        }

        if (startUpConfiguration.EnableMiniProfiler)
        {
            app.UseMiniProfiler();
        }

        app.UseCustomSpa(startUpConfiguration.PathToClientApp);

        SecurityManager.SetDIServiceProvider(app.ApplicationServices);
        HttpTools.SetDIServiceProvider(app.ApplicationServices);
    }
}
