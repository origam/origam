using Origam.OrigamEngine;
using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.StaticFiles;
using Origam.Security.Identity;
using Origam.Server.Handlers;
using Owin;
using System;
using System.Web;

public class Startup
{
    private static readonly ILog log = LogManager.GetLogger("OrigamServer");

    public void Configuration(IAppBuilder app)
    {
        // logging
        log4net.Config.XmlConfigurator.Configure();
        log.Info("Initializing OrigamServer...");
        // locale enforcement
        app.Use(typeof(LocaleEnforcement));
        app.UseStageMarker(PipelineStage.PostAuthenticate);
        // default url
        app.UseDefaultFiles(new DefaultFilesOptions
        {
            DefaultFileNames = new[] {"Portal"}
        });
        // authentication type
        app.UseCookieAuthentication(new CookieAuthenticationOptions
        {
            AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
            LoginPath = new PathString("/Login"),
            ExpireTimeSpan = System.TimeSpan.FromMinutes(60)
        });
        // 2nd factor
        //app.UseTwoFactorSignInCookie(
        //    DefaultAuthenticationTypes.TwoFactorCookie, 
        //    TimeSpan.FromMinutes(5));
        // fluorine filter
        app.Use(typeof(FluorineAuthenticationFilter));
        app.UseStageMarker(PipelineStage.PostAuthenticate);
        // user management
        AbstractUserManager.RegisterCreateUserManagerCallback(
            CreateUserManagerWithPasswordSettings);
        // Ajax handlers
        app.Map(new PathString("/AjaxLogin"), (application) =>
        {
            application.Use(typeof(AjaxLogin));
        });
        app.Map(new PathString("/AjaxSignOut"), (application) =>
        {
            application.Use(typeof(AjaxSignOut));
        });
        // Init Origam Engine
        OrigamEngine.ConnectRuntime();
    }

    private static AbstractUserManager CreateUserManager()
    {
        return NetMembershipUserManager.Create();
    }

    private static AbstractUserManager CreateOrigamModelUserManagerWithEmailConfirmation()
    {
        AbstractUserManager manager = OrigamModelUserManager.Create();
        DpapiDataProtectionProvider protectionProvider 
            = new DpapiDataProtectionProvider("Origam");
        manager.UserTokenProvider = new OrigamTokenProvider(
            protectionProvider.Create("Confirmation"));
        return manager;
    }

    private static AbstractUserManager CreateTwoFactorOrigamModelUserManager()
    {
        AbstractUserManager manager = OrigamModelUserManager.Create();
        manager.RegisterTwoFactorProvider(
            "EmailCode", new EmailTokenProvider<OrigamUser>()
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}."
            });
        manager.EmailService = new IdentityEmailService("admin@origam.com"); 
        return manager;
    }

    private static AbstractUserManager CreateTwoFactorNetMembershipUserManager()
    {
        AbstractUserManager manager = NetMembershipUserManager.Create();
        manager.RegisterTwoFactorProvider(
            "EmailCode", new EmailTokenProvider<OrigamUser>()
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}."
            });
        manager.EmailService = new IdentityEmailService("admin@origam.com"); 
        return manager;
    }

    private static AbstractUserManager CreateUserManagerWithPasswordSettings()
    {
        OrigamModelUserManager manager = (OrigamModelUserManager)OrigamModelUserManager.Create();
        manager.MinimumPasswordLength = 4;
        manager.NumberOfRequiredNonAlphanumericCharsInPassword = 0;
        manager.NumberOfInvalidPasswordAttempts = 3;
        return manager;
    }
}