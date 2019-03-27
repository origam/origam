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
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Origam.Security.Common;
using Origam.Security.Identity;
using Origam.ServerCore.Controllers;

namespace Origam.ServerCore.Authorization
{
    public class CoreManagerAdapter: IManager
    {
        private readonly CoreUserManager coreUserManager;
        private readonly IMailService mailService;

        public CoreManagerAdapter(CoreUserManager coreUserManager,
            IMailService mailService)
        {
            this.coreUserManager = coreUserManager;
            this.mailService = mailService;
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
            IOrigamUser user = await coreUserManager.FindByIdAsync(userName);
            string token =
                await coreUserManager.GenerateEmailConfirmationTokenAsync(user);
            mailService.SendNewUserToken(user,token);
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
            IOrigamUser user = await coreUserManager.FindByEmailAsync(email);
            
            if (user == null)
            {
                return new TokenResult
                { Token = "", UserName = "",
                    ErrorMessage = Resources.EmailInvalid,
                    TokenValidityHours = 0};
            }

            string token = await GenerateEmailConfirmationTokenAsync(user.BusinessPartnerId);
            
            return new TokenResult
            { Token = token,
                UserName = user.UserName, ErrorMessage = "",
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