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
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Origam.Security.Common;
using Origam.Server.Configuration;

namespace Origam.Server.Authorization;

class MailService : IMailService
{
    private AccountMailSender mailSender;
    private LanguageConfig languageConfig;

    public MailService(
        IOptions<UserConfig> userConfig,
        LanguageConfig languageConfig,
        IConfiguration configuration
    )
    {
        this.languageConfig = languageConfig;
        string baseUrl =
            configuration[WebHostDefaults.ServerUrlsKey]
                ?.Replace(";", ",")
                ?.Split(",")
                ?.FirstOrDefault(url => url.StartsWith("https"))
            ?? throw new ArgumentException("Could not find server's https url");
        mailSender = new AccountMailSender(
            portalBaseUrl: baseUrl,
            registerNewUserFilename: userConfig.Value.UserRegistrationMailBodyFileName,
            fromAddress: userConfig.Value.FromAddress,
            registerNewUserSubject: userConfig.Value.UserRegistrationMailSubject,
            userUnlockNotificationBodyFilename: userConfig.Value.UserUnlockNotificationBodyFileName,
            userUnlockNotificationSubject: userConfig.Value.UserUnlockNotificationSubject,
            resetPwdBodyFilename: GetDefaultResetPasswordFileName(),
            resetPwdSubject: GetDefaultResetPasswordSubject(),
            applicationBasePath: AppContext.BaseDirectory,
            mfaTemplateFileName: userConfig.Value.MultiFactorMailBodyFileName,
            mfaSubject: userConfig.Value.MultiFactorMailSubject,
            mailQueueName: userConfig.Value.MailQueueName
        );
    }

    private string GetDefaultResetPasswordSubject()
    {
        return languageConfig
            .CultureItems.Where(cultname =>
                cultname.CultureName.Equals(Thread.CurrentThread.CurrentUICulture.Name)
            )
            .Select(cultname =>
            {
                return cultname.ResetPasswordMailSubject;
            })
            .FirstOrDefault();
    }

    private string GetDefaultResetPasswordFileName()
    {
        return languageConfig
            .CultureItems.Where(cultname =>
                cultname.CultureName.Equals(Thread.CurrentThread.CurrentUICulture.Name)
            )
            .Select(cultname =>
            {
                return cultname.ResetPasswordMailBodyFileName;
            })
            .FirstOrDefault();
    }

    public void SendPasswordResetToken(
        IOrigamUser user,
        string token,
        string returnUrl,
        int tokenValidityHours
    )
    {
        SetResetPasswordItems();
        mailSender.SendPasswordResetToken(
            username: user.UserName,
            name: user.Name,
            email: user.Email,
            languageId: user.LanguageId.ToString(),
            firstName: user.FirstName,
            returnUrl: returnUrl,
            token: token,
            tokenValidityHours: tokenValidityHours,
            resultMessage: out string _
        );
    }

    private void SetResetPasswordItems()
    {
        mailSender.ResetPasswordSubject = GetDefaultResetPasswordSubject();
        mailSender.ResetPasswordBodyFilename = GetDefaultResetPasswordFileName();
    }

    public void SendNewUserToken(IOrigamUser user, string token)
    {
        SetResetPasswordItems();
        mailSender.SendNewUserToken(
            userId: user.BusinessPartnerId,
            email: user.Email,
            username: user.UserName,
            name: user.Name,
            firstName: user.FirstName,
            token: token
        );
    }

    public void SendMultiFactorAuthCode(IOrigamUser user, string token)
    {
        mailSender.SendMultiFactorAuthCode(email: user.Email, code: token);
    }

    public void SendUserUnlockedMessage(IOrigamUser user)
    {
        mailSender.SendUserUnlockingNotification(
            username: user.UserName,
            email: user.Email,
            languageId: user.LanguageId.ToString(),
            firstNameAndName: user.FirstName + " " + user.Name
        );
    }
}
