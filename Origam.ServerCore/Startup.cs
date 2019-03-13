using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Origam.Security.Common;
using Origam.Security.Identity;
using Origam.ServerCore.Authorization;
using Origam.ServerCore.Configuration;

namespace Origam.ServerCore
{
    public class Startup
    {
        private readonly StartUpConfiguration startUpConfiguration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            startUpConfiguration = new StartUpConfiguration(configuration);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
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
            services.AddScoped<IUserClaimsPrincipalFactory<IOrigamUser>,UserClaimsPrincipalFactory<IOrigamUser>>();
            services.AddScoped<CoreUserManager>();
            services.AddScoped<UserManager<IOrigamUser>>(x =>
                x.GetRequiredService<CoreUserManager>());
            
            services.AddIdentity<IOrigamUser, Role>()
                .AddDefaultTokenProviders();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Jwt";
                options.DefaultChallengeScheme = "Jwt";
            }).AddJwtBearer("Jwt", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,  //ValidAudience = "the audience you want to validate",
                    ValidateIssuer = false,  //ValidIssuer = "the user you want to validate",
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(startUpConfiguration.SecurityKey)),
                    ValidateLifetime = true, //validate the expiration and not before values in the token
                    ClockSkew = TimeSpan.FromMinutes(5) // 5 minute tolerance for the expiration date
                };
            });
            services.Configure<AccountConfig>(options => Configuration.GetSection("AccountConfig").Bind(options));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddLog4Net();
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.MapWhen(
                IsPublicUserApiRoute,
                apiBranch => {
                    apiBranch.UseResponseBuffering();
                    apiBranch.UseMiddleware<UserApiMiddleWare>();
                });
            app.MapWhen(
                IsRestrictedUserApiRoute,
                apiBranch => {
                    apiBranch.UseAuthentication();
                    apiBranch.UseMiddleware<SetCurrentPrincipalMiddleWare>();
                    apiBranch.UseResponseBuffering();
                    apiBranch.UseMiddleware<UserApiMiddleWare>();
                });
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMiddleware<SetCurrentPrincipalMiddleWare>();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseMvc();
            app.UseSpa(spa => {});
            
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
