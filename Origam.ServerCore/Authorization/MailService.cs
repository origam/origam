#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Origam.Security.Common;
using Origam.ServerCore.Configuration;

namespace Origam.ServerCore.Authorization
{
    class MailService : IMailService
    {
        private AccountMailSender mailSender;

        public MailService(IOptions<AccountConfig> accountConfig, IConfiguration configuration)
        {
            string baseUrl = configuration[WebHostDefaults.ServerUrlsKey].Split(",")[0];
            mailSender = new AccountMailSender(
                fromAddress: accountConfig.Value.FromAddress,
                resetPwdSubject: accountConfig.Value.ResetPasswordMailSubject,
                resetPwdBodyFilename: accountConfig.Value.ResetPasswordMailBodyFileName,
                userUnlockNotificationSubject: accountConfig.Value.UserUnlockNotificationSubject,
                userUnlockNotificationBodyFilename: accountConfig.Value.UserUnlockNotificationBodyFileName,
                registerNewUserSubject: accountConfig.Value.UserRegistrationMailSubject,
                registerNewUserFilename: accountConfig.Value.UserRegistrationMailBodyFileName,
                mailQueueName: accountConfig.Value.MailQueueName,
                portalBaseUrl: baseUrl,
                applicationBasePath: AppContext.BaseDirectory);
        }


        public void SendPasswordResetToken(IOrigamUser user, string token,
            int tokenValidityHours)
        {           
            mailSender.SendPasswordResetToken(
                username: user.UserName,
                name: user.Name,
                email: user.Email,
                languageId: user.LanguageId.ToString(),
                firstName: user.FirstName,
                token: token,
                tokenValidityHours: tokenValidityHours,
                resultMessage: out string _);
        }

        public void SendNewUserToken(IOrigamUser user, string token)
        {
            mailSender.SendNewUserToken(
                userId: user.BusinessPartnerId,
                email: user.Email,
                username: user.UserName,
                name: user.Name,
                firstName: user.FirstName,
                token: token);
        }
    }
}