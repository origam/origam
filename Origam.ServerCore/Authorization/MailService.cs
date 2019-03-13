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
                registerNewUserSubject: accountConfig.Value.userRegistrationMailSubject,
                registerNewUserFilename: accountConfig.Value.userRegistrationMailBodyFileName,
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