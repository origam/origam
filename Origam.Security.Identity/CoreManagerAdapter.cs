using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Origam.Security.Common;

namespace Origam.Security.Identity
{
    public class CoreManagerAdapter:IManager
    {
        private readonly CoreUserManager coreUserManager;
        private readonly Action<IOrigamUser> verificationTokenMailSender;
        private readonly InternalUserManager internalUserManager;

        public CoreManagerAdapter(CoreUserManager coreUserManager)
        {
            this.coreUserManager = coreUserManager;
            internalUserManager = new InternalUserManager(
                userName => new User(userName),
                numberOfInvalidPasswordAttempts: 3,
                frameworkSpecificManager: coreUserManager,
                mailTemplateDirectoryPath: "",
                mailQueueName: "",
                portalBaseUrl:"",
                registerNewUserFilename:"",
                fromAddress:"",
                registerNewUserSubject: "",
                resetPwdSubject: "",
                resetPwdBodyFilename: "",
                userUnlockNotificationBodyFilename: "",
                userUnlockNotificationSubject:""
            );
        }

        public async Task<IOrigamUser> FindByNameAsync(string name)
        {
           return await coreUserManager.FindByNameAsync(name);
        }

        public Task<bool> ChangePasswordQuestionAndAnswerAsync(string userName, string password,
            string question, string answer)
        {
            return Task.FromResult(false);
        }

        public async Task<bool> IsLockedOutAsync(string userId)
        {
            var user = await coreUserManager.FindByIdAsync(userId);
            return await coreUserManager.IsLockedOutAsync(user);
        }

        public async Task<bool> GetTwoFactorEnabledAsync(string userId)
        {
            var user = await coreUserManager.FindByIdAsync(userId);
            return await coreUserManager.GetTwoFactorEnabledAsync(user);
        }

        public async Task<bool> SetTwoFactorEnabledAsync(string userId, bool enabled)
        {
           var user = await coreUserManager.FindByIdAsync(userId);
           return (await coreUserManager.SetTwoFactorEnabledAsync(user, enabled))
               .Succeeded;
        }

        public async Task<bool> IsEmailConfirmedAsync(string userId)
        {
            var user = await coreUserManager.FindByIdAsync(userId);
            return await coreUserManager.IsEmailConfirmedAsync(user);
        }

        public async Task<bool> UnlockUserAsync(string userName)
        {
            var user = await coreUserManager.FindByIdAsync(userName);
            return (await coreUserManager.SetLockoutEndDateAsync( user,DateTimeOffset.MinValue))
                .Succeeded;
        }

        public async Task<InternalIdentityResult> ConfirmEmailAsync(string userId)
        {
            var user = await coreUserManager.FindByIdAsync(userId);
            string token = await coreUserManager.GenerateEmailConfirmationTokenAsync(user);
            IdentityResult coreIdentityResult =
                await coreUserManager.ConfirmEmailAsync(user, token);
            return ToInternalIdentityResult(coreIdentityResult);
        }

        public async Task<InternalIdentityResult> ConfirmEmailAsync(string userId, string token)
        {
            var user = await coreUserManager.FindByIdAsync(userId);
            IdentityResult coreIdentityResult = 
                await coreUserManager.ConfirmEmailAsync(user, token);
            return ToInternalIdentityResult(coreIdentityResult);
        }

        public async Task<InternalIdentityResult> ChangePasswordAsync(string userId, string currentPassword,
            string newPassword)
        {
            var user = await coreUserManager.FindByIdAsync(userId);
            IdentityResult coreIdentityResult = 
                await coreUserManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return ToInternalIdentityResult(coreIdentityResult);
        }

        public async Task<InternalIdentityResult> ResetPasswordFromUsernameAsync(string userName, string token,
            string newPassword)
        {
            var user = await coreUserManager.FindByIdAsync(userName);
            IdentityResult coreIdentityResult = await coreUserManager.ResetPasswordAsync(user, token, newPassword);
            return ToInternalIdentityResult(coreIdentityResult);
        }

        public async Task<InternalIdentityResult> DeleteAsync(IOrigamUser user)
        {
            IdentityResult coreIdentityResult = 
                await coreUserManager.DeleteAsync(user);
            return ToInternalIdentityResult(coreIdentityResult);
        }

        public async Task<InternalIdentityResult> UpdateAsync(IOrigamUser user)
        {
            IdentityResult coreIdentityResult = await coreUserManager.UpdateAsync( user);
            return ToInternalIdentityResult(coreIdentityResult);
        }

        public async void SendNewUserToken(string userName)
        {
            var user = await coreUserManager.FindByIdAsync(userName);
            verificationTokenMailSender(user);
        }

        public async Task<InternalIdentityResult> CreateAsync(IOrigamUser user, string password)
        {
            user.PasswordHash = password;
            coreUserManager.CreateOrigamUser(user);
            return InternalIdentityResult.Success;
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
        {
            var user = await coreUserManager.FindByIdAsync(userId);
            return await coreUserManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<TokenResult> GetPasswordResetTokenFromEmailAsync(string email)
        {
            Maybe<Tuple<Guid,string>> maybeUserInfo = internalUserManager.UserNameAndIdFromMail(email);
            if (maybeUserInfo.HasNoValue)
            {
                return new TokenResult
                { Token = "", UserName = "",
                    ErrorMessage = Resources.EmailInvalid,
                    TokenValidityHours = 0};
            }

            (Guid userId, string userName) = maybeUserInfo.Value;
            string token = await GenerateEmailConfirmationTokenAsync(userId.ToString());
            
            return new TokenResult
            { Token = token,
                UserName = userName, ErrorMessage = "",
                TokenValidityHours = 24};
        }

        public async Task<string> GeneratePasswordResetTokenAsync1(string userId)
        {
            return await GenerateEmailConfirmationTokenAsync(userId);
        }

        public Task<XmlDocument> GetPasswordAttributesAsync()
        {
            return Task.FromResult(new XmlDocument());
        }
        
        
        private static InternalIdentityResult ToInternalIdentityResult(IdentityResult result)
        {
            string[] errors = result.Errors
                .Select(error => error.Description)
                .ToArray();
            return result.Succeeded 
                ? InternalIdentityResult.Success 
                : InternalIdentityResult.Failed(errors);
        }
    }
}