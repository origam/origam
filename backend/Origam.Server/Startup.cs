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
using OpenIddict.Abstractions;
using Origam.Security.Common;
using Origam.Security.Identity;
using Origam.Server.Authorization;
using Origam.Server.Configuration;
using Origam.Server.Middleware;
using Origam.Service.Core;
using SoapCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults;

namespace Origam.Server
{
    // [ADDED] Minimal EF Core DbContext to host OpenIddict entities.
    // If you prefer custom stores, you can remove this and register your own IOpenIddict*Store<> impls.
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
        private readonly IdentityServerConfig identityServerConfig; // keep for existing config surface (clients/redirects, etc.)
        private readonly UserLockoutConfig lockoutConfig;
        private readonly LanguageConfig languageConfig;
        private readonly ChatConfig chatConfig;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            startUpConfiguration = new StartUpConfiguration(configuration);
            passwordConfiguration = new PasswordConfiguration(configuration);
            identityServerConfig = new IdentityServerConfig(configuration);
            lockoutConfig = new UserLockoutConfig(configuration);
            languageConfig = new LanguageConfig(configuration);
            chatConfig = new ChatConfig(configuration);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ServicePointManager.SecurityProtocol = startUpConfiguration.SecurityProtocol;

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

            // remove limit for multipart body length
            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = startUpConfiguration.ValueLengthLimit;
                options.MultipartBodyLengthLimit = startUpConfiguration.MultipartBodyLengthLimit;
                options.MultipartHeadersLengthLimit =
                    startUpConfiguration.MultipartHeadersLengthLimit;
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

            services.TryAddScoped<
                ILookupNormalizer,
                Authorization.UpperInvariantLookupNormalizer
            >();
            services.AddScoped<IManager, CoreManagerAdapter>();
            services.AddSingleton<IMailService, MailService>();
            services.AddSingleton<SearchHandler>();
            services.AddSingleton<SessionObjects, SessionObjects>();

            // Your custom Identity stack (unchanged)
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
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = passwordConfiguration.RequireDigit;
                options.Password.RequiredLength = passwordConfiguration.RequiredLength;
                options.Password.RequireNonAlphanumeric =
                    passwordConfiguration.RequireNonAlphanumeric;
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

            // [CHANGED] OpenIddict: use EF Core stores for apps/authorizations/scopes/tokens.
            services.AddDbContext<AuthDbContext>(opts =>
            {
                // Reuse an existing connection string or add a dedicated "AuthDb" entry.
                // Example: opts.UseSqlServer(Configuration.GetConnectionString("AuthDb"));
                // If you want to co-host in the main DB, point to that CS here.
                var cs =
                    Configuration.GetConnectionString("AuthDb")
                    ?? Configuration.GetConnectionString("DefaultConnection");
                opts.UseSqlServer(cs);
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
                    // Endpoints
                    opt.SetAuthorizationEndpointUris("/connect/authorize")
                        .SetTokenEndpointUris("/connect/token")
                        .SetIntrospectionEndpointUris("/connect/introspect")
                        .SetEndSessionEndpointUris("/connect/logout");

                    // Flows (align with previous usage: code + ROPC for serverClient)
                    opt.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();
                    opt.AllowPasswordFlow(); // for "serverClient"-style flows
                    opt.AllowRefreshTokenFlow();
                    opt.AllowClientCredentialsFlow(); // if your backends need it

                    // Token settings
                    opt.SetAccessTokenLifetime(TimeSpan.FromHours(1));
                    opt.SetRefreshTokenLifetime(TimeSpan.FromDays(30));

                    // Host integration
                    opt.UseAspNetCore()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableTokenEndpointPassthrough()
                        .EnableEndSessionEndpointPassthrough();

                    // Signing/encryption (replace with real certs in prod)
                    opt.AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate();

                    // Scopes similar to prior setup
                    opt.RegisterScopes(
                        Scopes.OpenId,
                        Scopes.Profile,
                        Scopes.OfflineAccess,
                        "internal_api",
                        "local_api"
                    ); // "internal_api" replaces LocalApi
                })
                .AddValidation(opt =>
                {
                    opt.UseLocalServer();
                    opt.UseAspNetCore();
                    opt.Configure(o =>
                    {
                        o.TokenValidationParameters.NameClaimType = OpenIddictConstants
                            .Claims
                            .Subject;
                    });
                });

            // [CHANGED] Authentication defaults — use OpenIddict validation
            // services.AddAuthentication(options =>
            // {
            //     options.DefaultAuthenticateScheme =
            //         OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            //     options.DefaultChallengeScheme =
            //         OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            // });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme; // cookie
                options.DefaultSignInScheme = IdentityConstants.ApplicationScheme; // cookie
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme; // cookie
            });

            // [CHANGED] Authorization policy replacing IdentityServer's LocalApi
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
                
                options.AddPolicy(
                    "LocalApi",
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

                            return scopes.Contains("local_api");
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

            // Load extension controllers (unchanged)
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
                            .Authorize(
                                SecurityManager.CurrentPrincipal,
                                "SYS_ViewMiniProfilerResults"
                            );
                });
            }
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
                    // .Append(new ResourceOwnerPasswordAuthenticationProvider())
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
            // [CHANGED] Remove AddLocalApiAuthentication() (IdentityServer) and keep external providers.
            var auth = services.AddAuthentication();

            if (identityServerConfig.GoogleLogin != null)
            {
                auth.AddGoogle(
                    GoogleDefaults.AuthenticationScheme,
                    "SignInWithGoogleAccount",
                    options =>
                    {
                        options.ClientId = identityServerConfig.GoogleLogin.ClientId;
                        options.ClientSecret = identityServerConfig.GoogleLogin.ClientSecret;
                        options.SignInScheme = IdentityConstants.ExternalScheme; // [CHANGED]
                    }
                );
            }

            if (identityServerConfig.MicrosoftLogin != null)
            {
                auth.AddMicrosoftAccount(
                    MicrosoftAccountDefaults.AuthenticationScheme,
                    "SignInWithMicrosoftAccount",
                    microsoftOptions =>
                    {
                        microsoftOptions.ClientId = identityServerConfig.MicrosoftLogin.ClientId;
                        microsoftOptions.ClientSecret = identityServerConfig
                            .MicrosoftLogin
                            .ClientSecret;
                        microsoftOptions.SignInScheme = IdentityConstants.ExternalScheme; // [CHANGED]
                    }
                );
            }

            if (identityServerConfig.AzureAdLogin != null)
            {
                auth.AddOpenIdConnect(
                    "AzureAdOIDC",
                    "SignInWithAzureAd",
                    options =>
                    {
                        options.ClientId = identityServerConfig.AzureAdLogin.ClientId;
                        options.Authority =
                            $@"https://login.microsoftonline.com/{identityServerConfig.AzureAdLogin.TenantId}/";
                        options.CallbackPath = "/signin-oidc";
                        options.SaveTokens = true;
                        options.SignInScheme = IdentityConstants.ExternalScheme; // [CHANGED]
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidAudience = identityServerConfig.AzureAdLogin.ClientId,
                        };
                    }
                );
            }

            // ExternalController and post-processor config (unchanged)
            services.AddSingleton(identityServerConfig);

            if (string.IsNullOrEmpty(identityServerConfig.AuthenticationPostProcessor))
            {
                services.AddSingleton<
                    IAuthenticationPostProcessor,
                    AlwaysValidAuthenticationPostProcessor
                >();
            }
            else
            {
                var classpath = identityServerConfig.AuthenticationPostProcessor.Split(',');
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

            // [REMOVED] app.UseIdentityServer();
            app.UseMiddleware<FatalErrorMiddleware>();

            // Keep existing APIs/middleware
            app.UseUserApi(startUpConfiguration, identityServerConfig);
            app.UseWorkQueueApi();

            app.UseRouting();
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
                        RequestPath = new PathString(
                            startUpConfiguration.RouteToCustomAssetsFolder
                        ),
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

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            if (startUpConfiguration.EnableMiniProfiler)
            {
                app.UseMiniProfiler();
            }

            app.UseCustomSpa(startUpConfiguration.PathToClientApp);

            // DI bridge to Origam (unchanged)
            SecurityManager.SetDIServiceProvider(app.ApplicationServices);
            HttpTools.SetDIServiceProvider(app.ApplicationServices);
            OrigamUtils.ConnectOrigamRuntime(
                loggerFactory,
                startUpConfiguration.ReloadModelWhenFilesChangesDetected
            );
            OrigamUtils.CleanUpDatabase();
        }
    }
}
