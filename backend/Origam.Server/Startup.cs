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
using Origam.DA.Service;
using Origam.Mail;
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
        : base(options: options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(modelBuilder: builder);
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
        startUpConfiguration = new StartUpConfiguration(configuration: configuration);
        passwordConfiguration = new PasswordConfiguration(configuration: configuration);
        openIddictConfig = new OpenIddictConfig(configuration: configuration);
        lockoutConfig = new UserLockoutConfig(configuration: configuration);
        languageConfig = new LanguageConfig(configuration: configuration);
        chatConfig = new ChatConfig(configuration: configuration);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        ServicePointManager.SecurityProtocol = startUpConfiguration.SecurityProtocol;
        services.AddSingleton(implementationInstance: startUpConfiguration);
        services.Configure<KestrelServerOptions>(configureOptions: options =>
        {
            options.AllowSynchronousIO = true;
        });
        services.Configure<IISServerOptions>(configureOptions: options =>
        {
            options.AllowSynchronousIO = true;
            options.AuthenticationDisplayName = "Windows";
            options.AutomaticAuthentication = true;
        });

        services.Configure<FormOptions>(configureOptions: options =>
        {
            options.ValueLengthLimit = startUpConfiguration.ValueLengthLimit;
            options.MultipartBodyLengthLimit = startUpConfiguration.MultipartBodyLengthLimit;
            options.MultipartHeadersLengthLimit = startUpConfiguration.MultipartHeadersLengthLimit;
        });

        services.Configure<IdentityOptions>(configureOptions: options =>
        {
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(
                value: lockoutConfig.LockoutTimeMinutes
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

        services.Configure<RazorViewEngineOptions>(configureOptions: options =>
        {
            options.ViewLocationFormats.Add(item: "/Identity/Views/{1}/{0}.cshtml");
            options.ViewLocationFormats.Add(item: "/Identity/Views/Shared/{0}.cshtml");
        });

        services.ConfigureApplicationCookie(configure: options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(
                value: openIddictConfig.CookieExpirationMinutes
            );
            options.SlidingExpiration = openIddictConfig.CookieSlidingExpiration;
        });

        services.Configure<IdentityOptions>(configureOptions: options =>
        {
            options.Password.RequireDigit = passwordConfiguration.RequireDigit;
            options.Password.RequiredLength = passwordConfiguration.RequiredLength;
            options.Password.RequireNonAlphanumeric = passwordConfiguration.RequireNonAlphanumeric;
            options.Password.RequireUppercase = passwordConfiguration.RequireUppercase;
            options.Password.RequireLowercase = passwordConfiguration.RequireLowercase;

            var userConfig = new UserConfig();
            Configuration.GetSection(key: "UserConfig").Bind(instance: userConfig);
            if (!string.IsNullOrEmpty(value: userConfig.AllowedUserNameCharacters))
            {
                options.User.AllowedUserNameCharacters = userConfig.AllowedUserNameCharacters;
            }
        });

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddTransient<IPrincipal>(implementationFactory: provider =>
            provider.GetService<IHttpContextAccessor>().HttpContext?.User
        );

        services.Configure<UserConfig>(configureOptions: options =>
            Configuration.GetSection(key: "UserConfig").Bind(instance: options)
        );
        services.Configure<ClientFilteringConfig>(configureOptions: options =>
            Configuration.GetSection(key: "ClientFilteringConfig").Bind(instance: options)
        );
        services.Configure<IdentityGuiConfig>(configureOptions: options =>
            Configuration.GetSection(key: "IdentityGuiConfig").Bind(instance: options)
        );
        services.Configure<CustomAssetsConfig>(configureOptions: options =>
            Configuration.GetSection(key: "CustomAssetsConfig").Bind(instance: options)
        );
        services.Configure<UserLockoutConfig>(configureOptions: options =>
            Configuration.GetSection(key: "UserLockoutConfig").Bind(instance: options)
        );
        services.Configure<ChatConfig>(configureOptions: options =>
            Configuration.GetSection(key: "ChatConfig").Bind(instance: options)
        );
        services.Configure<HtmlClientConfig>(configureOptions: options =>
            Configuration.GetSection(key: "HtmlClientConfig").Bind(instance: options)
        );

        services.AddDbContext<AuthDbContext>(optionsAction: options =>
        {
            OrigamSettings origamSettings = ConfigurationManager.GetActiveConfiguration();
            if (origamSettings.DataDataService.Contains(value: nameof(MsSqlDataService)))
            {
                options.UseSqlServer(connectionString: origamSettings.DataConnectionString);
            }
            else if (origamSettings.DataDataService.Contains(value: nameof(PgSqlDataService)))
            {
                options.UseNpgsql(connectionString: origamSettings.DataConnectionString);
            }
            else
            {
                throw new Exception(
                    message: "Unknown data service: " + origamSettings.DataDataService
                );
            }
            options.UseOpenIddict();
        });

        services
            .AddOpenIddict()
            .AddCore(configuration: options =>
            {
                options.UseEntityFrameworkCore().UseDbContext<AuthDbContext>();
                options
                    .UseQuartz()
                    .SetMinimumTokenLifespan(lifespan: TimeSpan.FromDays(value: 7))
                    .SetMinimumAuthorizationLifespan(lifespan: TimeSpan.FromDays(value: 7));
            })
            .AddServer(configuration: options =>
            {
                string accessTokenIssuer = openIddictConfig.AccessTokenIssuer;
                if (!string.IsNullOrWhiteSpace(value: accessTokenIssuer))
                {
                    options.SetIssuer(uri: new Uri(uriString: accessTokenIssuer));
                }

                options
                    .SetAuthorizationEndpointUris(uris: "/connect/authorize")
                    .SetTokenEndpointUris(uris: "/connect/token")
                    .SetIntrospectionEndpointUris(uris: "/connect/introspect")
                    .SetEndSessionEndpointUris(uris: "/connect/logout");
                options.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();
                options.AllowPasswordFlow();
                options.AllowRefreshTokenFlow();
                options.AllowClientCredentialsFlow();
                options.SetAccessTokenLifetime(lifetime: TimeSpan.FromHours(value: 1));
                options.SetRefreshTokenLifetime(lifetime: TimeSpan.FromDays(value: 30));
                options
                    .UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough();
                options.AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate();
                options.RegisterScopes(
                    scopes:
                    [
                        Scopes.OpenId,
                        Scopes.Profile,
                        Scopes.OfflineAccess,
                        "internal_api",
                        "local_api",
                    ]
                );
            })
            .AddValidation(configuration: options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
                options.Configure(configuration: opts =>
                {
                    opts.TokenValidationParameters.NameClaimType = Claims.Name;
                    opts.TokenValidationParameters.RoleClaimType = Claims.Role;
                });
            });

        services.AddAuthentication(configureOptions: options =>
        {
            options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme; // cookie
            options.DefaultSignInScheme = IdentityConstants.ApplicationScheme; // cookie
            options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme; // cookie
        });

        services.AddAuthorization(configure: options =>
        {
            options.AddPolicy(
                name: "InternalApi",
                configurePolicy: policy =>
                {
                    policy.AddAuthenticationSchemes(schemes: AuthenticationScheme);
                    policy.RequireAuthenticatedUser();

                    policy.RequireAssertion(handler: ctx =>
                    {
                        var scopes = ctx
                            .User.FindAll(type: Claims.Scope)
                            .SelectMany(selector: c =>
                                c.Value.Split(
                                    separator: ' ',
                                    options: StringSplitOptions.RemoveEmptyEntries
                                )
                            );

                        return scopes.Contains(value: "internal_api");
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
            .AddViewLocalization(format: LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization(setupAction: options =>
            {
                options.DataAnnotationLocalizerProvider = (type, factory) =>
                    factory.Create(resourceSource: typeof(SharedResources));
            });

        ConfigureAuthentication(services: services);

        services.Configure<RequestLocalizationOptions>(configureOptions: options =>
        {
            options.DefaultRequestCulture = languageConfig.DefaultCulture;
            options.SupportedCultures = languageConfig.AllowedCultures;
            options.SupportedUICultures = languageConfig.AllowedCultures;
            options.RequestCultureProviders.Clear();
            options.RequestCultureProviders.Insert(
                index: 0,
                item: new OrigamCookieRequestCultureProvider(languageConfig: languageConfig)
            );
        });

        foreach (var controllerDllName in startUpConfiguration.ExtensionDlls)
        {
            var customControllerAssembly = Assembly.LoadFrom(assemblyFile: controllerDllName);
            services.AddControllers().AddApplicationPart(assembly: customControllerAssembly);
        }

        var providerFactory = LoadClientAuthenticationProviders(
            configuration: Configuration,
            startUpConfiguration: startUpConfiguration
        );
        services.AddSingleton(implementationInstance: providerFactory);

        if (startUpConfiguration.EnableMiniProfiler)
        {
            services.AddMiniProfiler(configureOptions: options =>
            {
                options.RouteBasePath = "/profiler";
                options.PopupDecimalPlaces = 1;
                options.ResultsAuthorize = request =>
                    SecurityManager
                        .GetAuthorizationProvider()
                        .Authorize(
                            principal: SecurityManager.CurrentPrincipal,
                            context: "SYS_ViewMiniProfilerResults"
                        );
                options.ShouldProfile = request =>
                    ShouldProfileRequest(
                        request: request,
                        startUpConfiguration: startUpConfiguration,
                        chatConfig: chatConfig
                    );
            });
        }

        var allowedCorsOrigins = openIddictConfig
            .ClientApplicationTemplates
            .WebClient
            .AllowedCorsOrigins;
        services.AddCors(setupAction: options =>
        {
            options.AddPolicy(
                name: "OrigamCorsPolicy",
                configurePolicy: builder =>
                {
                    builder
                        .WithOrigins(origins: allowedCorsOrigins)
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
            var providerAssembly = Assembly.LoadFrom(assemblyFile: providerDllName);
            providerAssembly
                .GetTypes()
                .Where(predicate: type =>
                    typeof(IClientAuthenticationProvider).IsAssignableFrom(c: type)
                )
                .Select(selector: type =>
                    (IClientAuthenticationProvider)Activator.CreateInstance(type: type)
                )
                .Append(element: new ResourceOwnerPasswordAuthenticationProvider())
                .ToList()
                .ForEach(action: provider =>
                {
                    provider.Configure(configuration: configuration);
                    providerFactory.Register(provider: provider);
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
                authenticationScheme: GoogleDefaults.AuthenticationScheme,
                displayName: "SignInWithGoogleAccount",
                configureOptions: options =>
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
                authenticationScheme: MicrosoftAccountDefaults.AuthenticationScheme,
                displayName: "SignInWithMicrosoftAccount",
                configureOptions: microsoftOptions =>
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
                authenticationScheme: "AzureAdOIDC",
                displayName: "SignInWithAzureAd",
                configureOptions: options =>
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

        services.AddSingleton(implementationInstance: openIddictConfig);

        if (string.IsNullOrEmpty(value: openIddictConfig.AuthenticationPostProcessor))
        {
            services.AddSingleton<
                IAuthenticationPostProcessor,
                AlwaysValidAuthenticationPostProcessor
            >();
        }
        else
        {
            var classpath = openIddictConfig.AuthenticationPostProcessor.Split(separator: ',');
            var authenticationPostProcessor = Reflector.ResolveTypeFromAssembly(
                classname: classpath[0],
                assemblyName: classpath[1]
            );
            services.AddSingleton(
                serviceType: typeof(IAuthenticationPostProcessor),
                implementationType: authenticationPostProcessor
            );
        }
    }

    private static bool ShouldProfileRequest(
        HttpRequest request,
        StartUpConfiguration startUpConfiguration,
        ChatConfig chatConfig
    )
    {
        if (request == null)
        {
            return false;
        }

        string path = request.Path.Value;
        if (string.IsNullOrEmpty(value: path))
        {
            return true;
        }

        if (Path.HasExtension(path: path))
        {
            return false;
        }

        if (path.StartsWith(value: "/assets", comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (
            startUpConfiguration.HasCustomAssets
            && path.StartsWith(
                value: startUpConfiguration.RouteToCustomAssetsFolder,
                comparisonType: StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(value: chatConfig.PathToChatApp))
        {
            if (
                path.StartsWith(
                    value: "/chatrooms",
                    comparisonType: StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return false;
            }

            if (
                path.StartsWith(
                    value: "/chatAssets",
                    comparisonType: StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return false;
            }
        }

        return true;
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
            app.UseExceptionHandler(errorHandlingPath: "/Error");
            app.UseHsts();
        }

        if (Configuration.GetValue<bool>(key: "BehindProxy"))
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
            app.UseForwardedHeaders(options: forwardedHeadersOptions);
        }

        var localizationOptions = app
            .ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>()
            .Value;
        app.UseRequestLocalization(options: localizationOptions);

        app.UseMiddleware<FatalErrorMiddleware>();
        app.UseMiddleware<OrigamErrorHandlingMiddleware>();

        app.UseUserApi(
            startUpConfiguration: startUpConfiguration,
            openIddictConfig: openIddictConfig
        );
        app.UseWorkQueueApi();

        app.UseRouting();
        app.UseCors(policyName: "OrigamCorsPolicy");
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseStaticFiles(
            options: new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    root: Path.Combine(path1: Directory.GetCurrentDirectory(), path2: "assets")
                ),
                RequestPath = new PathString(value: "/assets"),
            }
        );

        if (startUpConfiguration.HasCustomAssets)
        {
            app.UseStaticFiles(
                options: new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                        root: startUpConfiguration.PathToCustomAssetsFolder
                    ),
                    RequestPath = new PathString(
                        value: startUpConfiguration.RouteToCustomAssetsFolder
                    ),
                }
            );
        }

        if (startUpConfiguration.EnableMiniProfiler)
        {
            app.UseWhen(
                predicate: context =>
                    ShouldProfileRequest(
                        request: context.Request,
                        startUpConfiguration: startUpConfiguration,
                        chatConfig: chatConfig
                    ),
                configuration: profilerBranch => profilerBranch.UseMiniProfiler()
            );
        }

        app.UseEndpoints(configure: endpoints =>
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
                authenticationRequired: startUpConfiguration.SoapInterfaceRequiresAuthentication,
                expectAndReturnOldDotNetAssemblyReferences: startUpConfiguration.ExpectAndReturnOldDotNetAssemblyReferences
            );
        }

        app.UseCustomWebAppExtenders(
            configuration: Configuration,
            startUpConfiguration: startUpConfiguration
        );

        app.UseStaticFiles(
            options: new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(root: startUpConfiguration.PathToClientApp),
            }
        );

        if (!string.IsNullOrWhiteSpace(value: chatConfig.PathToChatApp))
        {
            app.UseStaticFiles(
                options: new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(root: chatConfig.PathToChatApp!),
                    RequestPath = new PathString(value: "/chatrooms"),
                    OnPrepareResponse = ctx =>
                    {
                        if (ctx.File.Name == "index.html")
                        {
                            ctx.Context.Response.Headers.Append(
                                key: "Cache-Control",
                                value: $"no-store, max-age=0"
                            );
                        }
                    },
                }
            );
            app.UseStaticFiles(
                options: new StaticFileOptions
                {
                    RequestPath = new PathString(value: "/chatAssets"),
                    FileProvider = new PhysicalFileProvider(
                        root: Path.Combine(path1: chatConfig.PathToChatApp, path2: "chatAssets")
                    ),
                }
            );
        }

        app.UseCustomSpa(pathToClientApp: startUpConfiguration.PathToClientApp);

        SecurityManager.SetDIServiceProvider(diServiceProvider: app.ApplicationServices);
        HttpTools.SetDIServiceProvider(serviceProvider: app.ApplicationServices);
        MailServiceFactory.SetDIServiceProvider(diServiceProvider: app.ApplicationServices);
    }
}
