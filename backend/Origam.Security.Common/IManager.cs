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

using System.Threading.Tasks;
using System.Xml;
using Origam.Security.Common;

namespace Origam.Security.Identity;

public interface IManager
{
    Task<IOrigamUser> FindByNameAsync(string name, string transactionId);
    Task<bool> ChangePasswordQuestionAndAnswerAsync(
        string userName,
        string password,
        string question,
        string answer
    );
    Task<bool> IsLockedOutAsync(string userId);
    Task<bool> GetTwoFactorEnabledAsync(string userId);
    Task<bool> SetTwoFactorEnabledAsync(string userId, bool enabled);
    Task<bool> IsEmailConfirmedAsync(string userId);
    Task<bool> UnlockUserAsync(string userName);
    Task<InternalIdentityResult> ConfirmEmailAsync(string userId);
    Task<InternalIdentityResult> ConfirmEmailAsync(string userId, string token);
    Task<InternalIdentityResult> ChangePasswordAsync(
        string userId,
        string currentPassword,
        string newPassword
    );
    Task<InternalIdentityResult> ResetPasswordFromUsernameAsync(
        string userName,
        string token,
        string newPassword
    );
    Task<InternalIdentityResult> DeleteAsync(IOrigamUser user);
    Task<InternalIdentityResult> UpdateAsync(IOrigamUser user);
    void SendNewUserToken(string userName);
    Task<InternalIdentityResult> CreateAsync(IOrigamUser user, string password);
    Task<string> GenerateEmailConfirmationTokenAsync(string userId);
    Task<TokenResult> GetPasswordResetTokenFromEmailAsync(string email);
    Task<string> GeneratePasswordResetTokenAsync(string userId);
    Task<XmlDocument> GetPasswordAttributesAsync();
    IOrigamUser CreateUserObject(string userName);
}
