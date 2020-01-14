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
using Origam.Server.Utils;
using Owin;
using System;
using System.Web;
using System.Net;

public class Startup
{
    private static readonly ILog log = LogManager.GetLogger("OrigamServer");

    public void Configuration(IAppBuilder app)
    {
        // logging
        log4net.Config.XmlConfigurator.Configure();
        try
        {
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
                ExpireTimeSpan = System.TimeSpan.FromMinutes(10)
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
                CreateUserManager);
            // Ajax handlers
            app.Map(new PathString("/AjaxLogin"), (application) =>
            {
                application.Use(typeof(AjaxLogin));
            });
            app.Map(new PathString("/AjaxSignOut"), (application) =>
            {
                application.Use(typeof(AjaxSignOut));
            });
            app.Map(new PathString("/IsInRole"), (application) =>
            {
                application.Use(typeof(IsInRole));
            });
            // handler for triggering commands on work queue
            app.Map(new PathString("/WorkQueueCommand"), (application) =>
            {
                application.Use(typeof(WorkQueueHandler));
            });
            // Init Origam Engine
            OrigamEngine.ConnectRuntime();
            StartupUtils.SubscribeToFileSystemChanges();
        }
        catch (Exception ex)
        {
            log.Error("Failed to initialize ORIGAM Application Server, check server log.", ex);
            app.Run(async context =>
            {
                context.Response.StatusCode 
                    = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "text/plain";
                await context.Response
                    .WriteAsync("Failed to initialize ORIGAM Application Server.")
                    .ConfigureAwait(false);
            });
        }
	}

    private static AbstractUserManager CreateUserManager()
    {
        return NetMembershipUserManager.Create();
    }

    private static AbstractUserManager CreateUserManagerWithDomain()
    {
        NetMembershipUserManager userManager = (NetMembershipUserManager)
            NetMembershipUserManager.Create();
        userManager.InjectDomain = "MYDOMAIN";
        return userManager;
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
        manager.Is2FAUsed = true;
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