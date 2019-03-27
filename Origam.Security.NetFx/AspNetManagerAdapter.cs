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
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNet.Identity;
using Origam.Security.Common;

namespace Origam.Security.Identity
{
    public class AspNetManagerAdapter: IManager
    {
        private readonly AbstractUserManager aspNetUserManager;

        public AspNetManagerAdapter(AbstractUserManager aspNetUserManager)
        {
            this.aspNetUserManager = aspNetUserManager;
        }

        public async  Task<IOrigamUser> FindByNameAsync(string name)
        {
           return await aspNetUserManager.FindByNameAsync(name);
        }

        //
        public async Task<bool> ChangePasswordQuestionAndAnswerAsync(string userName, string password,
            string question, string answer)
        {
            return await aspNetUserManager.ChangePasswordQuestionAndAnswerAsync(
                userName, password,question, answer);
        }

        public async Task<bool> IsLockedOutAsync(string userId)
        {
            return await aspNetUserManager.IsLockedOutAsync(userId);
        }

        public async Task<bool> GetTwoFactorEnabledAsync(string userId)
        {
            return await aspNetUserManager.GetTwoFactorEnabledAsync(userId);
        }

        public async Task<bool> SetTwoFactorEnabledAsync(string userId, bool enabled)
        {
            return (await aspNetUserManager.SetTwoFactorEnabledAsync(userId, enabled))
                .Succeeded;
        }

        public async Task<bool> IsEmailConfirmedAsync(string userId)
        {
            return await aspNetUserManager.IsEmailConfirmedAsync(userId);
        }

        public async Task<bool> UnlockUserAsync(string userName)
        {
            return await aspNetUserManager.UnlockUserAsync(userName);
        }

        public async Task<InternalIdentityResult> ConfirmEmailAsync(string userId)
        {
            return (await aspNetUserManager.ConfirmEmailAsync(userId))
                .ToInternalIdentityResult();
        }

        public async Task<InternalIdentityResult> ConfirmEmailAsync(string userId, string token)
        {
            return (await aspNetUserManager.ConfirmEmailAsync(userId, token))
                .ToInternalIdentityResult();
        }

        public async Task<InternalIdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            return (await aspNetUserManager.ChangePasswordAsync(userId, currentPassword,
                newPassword))
                .ToInternalIdentityResult();
        }

        public async Task<InternalIdentityResult> ResetPasswordFromUsernameAsync(string userName, string token, string newPassword)
        {
            return (await aspNetUserManager.ResetPasswordFromUsernameAsync(
                userName, token, newPassword))
                .ToInternalIdentityResult();
        }

        public async Task<InternalIdentityResult> DeleteAsync(IOrigamUser user)
        {
            return (await aspNetUserManager.DeleteAsync((OrigamUser) user))
                .ToInternalIdentityResult();
        }

        public async Task<InternalIdentityResult> UpdateAsync(IOrigamUser user)
        {
            return (await aspNetUserManager.UpdateAsync((OrigamUser)user))
                .ToInternalIdentityResult();
        }

        public void SendNewUserToken(string userName)
        {
            aspNetUserManager.SendNewUserToken(userName);
        }

        public async Task<InternalIdentityResult> CreateAsync(IOrigamUser user, string password)
        {
            return (await aspNetUserManager.CreateAsync((OrigamUser) user,
                password)).ToInternalIdentityResult();
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
        {
            return await aspNetUserManager.GenerateEmailConfirmationTokenAsync(
                userId);
        }

        public async Task<TokenResult> GetPasswordResetTokenFromEmailAsync(string email)
        {
            return await aspNetUserManager
                .GetPasswordResetTokenFromEmailAsync(email);
        }

        public async Task<string> GeneratePasswordResetTokenAsync1(string userId)
        {
            return await aspNetUserManager.GeneratePasswordResetTokenAsync(
                userId);
        }

        public async Task<XmlDocument> GetPasswordAttributesAsync()
        {
            return await aspNetUserManager.GetPasswordAttributesAsync();
        }
    }
}