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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using log4net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Origam.Security.Common;
using Origam.Security.Identity;

namespace Origam.Server.Authorization;

public class CoreManagerAdapter : IManager
{
    private readonly CoreUserManager<IOrigamUser> coreUserManager;
    private readonly IMailService mailService;
    protected static readonly ILog log = LogManager.GetLogger(type: typeof(CoreManagerAdapter));
    private readonly IStringLocalizer<SharedResources> localizer;

    public CoreManagerAdapter(
        CoreUserManager<IOrigamUser> coreUserManager,
        IMailService mailService,
        IStringLocalizer<SharedResources> localizer
    )
    {
        this.coreUserManager = coreUserManager;
        this.mailService = mailService;
        this.localizer = localizer;
    }

    public async Task<IOrigamUser> FindByNameAsync(string name, string transaction = null)
    {
        return await coreUserManager.FindByNameAsync(name: name, transactionId: transaction);
    }

    private async Task<IOrigamUser> FindByIdAsync(string userId)
    {
        var user = await coreUserManager.FindByIdAsync(userId: userId);
        if (log.IsDebugEnabled)
        {
            log.DebugFormat(format: "User not found...");
        }
        return user;
    }

    public Task<bool> ChangePasswordQuestionAndAnswerAsync(
        string userName,
        string password,
        string question,
        string answer
    )
    {
        return Task.FromResult(result: false);
    }

    public async Task<bool> IsLockedOutAsync(string userId)
    {
        var user = await FindByIdAsync(userId: userId);
        if (user == null)
        {
            return false;
        }
        return await coreUserManager.IsLockedOutAsync(user: user);
    }

    public async Task<bool> GetTwoFactorEnabledAsync(string userId)
    {
        var user = await FindByIdAsync(userId: userId);
        if (user == null)
        {
            return false;
        }
        return await coreUserManager.GetTwoFactorEnabledAsync(user: user);
    }

    public async Task<bool> SetTwoFactorEnabledAsync(string userId, bool enabled)
    {
        var user = await FindByIdAsync(userId: userId);
        if (user == null)
        {
            return false;
        }
        return (
            await coreUserManager.SetTwoFactorEnabledAsync(user: user, enabled: enabled)
        ).Succeeded;
    }

    public async Task<bool> IsEmailConfirmedAsync(string userId)
    {
        var user = await FindByIdAsync(userId: userId);
        if (user == null)
        {
            return false;
        }
        return await coreUserManager.IsEmailConfirmedAsync(user: user);
    }

    public async Task<bool> UnlockUserAsync(string userName)
    {
        var user = await FindByNameAsync(name: userName);
        if (user == null)
        {
            return false;
        }
        Task<IdentityResult> unlockTask = coreUserManager.SetLockoutEndDateAsync(
            user: user,
            lockoutEnd: null
        );
        bool success = (await unlockTask).Succeeded;
        if (success)
        {
            mailService.SendUserUnlockedMessage(user: user);
        }
        return success;
    }

    public async Task<InternalIdentityResult> ConfirmEmailAsync(string userId)
    {
        var user = await FindByIdAsync(userId: userId);
        string token = await coreUserManager.GenerateEmailConfirmationTokenAsync(user: user);
        IdentityResult coreIdentityResult = await coreUserManager.ConfirmEmailAsync(
            user: user,
            token: token
        );
        return ToInternalIdentityResult(result: coreIdentityResult);
    }

    public async Task<InternalIdentityResult> ConfirmEmailAsync(string userId, string token)
    {
        var user = await FindByIdAsync(userId: userId);
        IdentityResult coreIdentityResult = await coreUserManager.ConfirmEmailAsync(
            user: user,
            token: token
        );
        return ToInternalIdentityResult(result: coreIdentityResult);
    }

    public async Task<InternalIdentityResult> ChangePasswordAsync(
        string userId,
        string currentPassword,
        string newPassword
    )
    {
        var user = await FindByIdAsync(userId: userId);
        IdentityResult coreIdentityResult = await coreUserManager.ChangePasswordAsync(
            user: user,
            currentPassword: currentPassword,
            newPassword: newPassword
        );
        return ToInternalIdentityResult(result: coreIdentityResult);
    }

    public async Task<InternalIdentityResult> ResetPasswordFromUsernameAsync(
        string userName,
        string token,
        string newPassword
    )
    {
        var user = await FindByNameAsync(name: userName);
        IdentityResult coreIdentityResult = await coreUserManager.ResetPasswordAsync(
            user: user,
            token: token,
            newPassword: newPassword
        );
        return ToInternalIdentityResult(result: coreIdentityResult);
    }

    public async Task<InternalIdentityResult> DeleteAsync(IOrigamUser user)
    {
        IdentityResult coreIdentityResult = await coreUserManager.DeleteAsync(user: user);
        return ToInternalIdentityResult(result: coreIdentityResult);
    }

    public async Task<InternalIdentityResult> UpdateAsync(IOrigamUser user)
    {
        IdentityResult coreIdentityResult = await coreUserManager.UpdateAsync(user: user);
        return ToInternalIdentityResult(result: coreIdentityResult);
    }

    public async void SendNewUserToken(string userName)
    {
        IOrigamUser user = await FindByIdAsync(userId: userName);
        string token = await coreUserManager.GenerateEmailConfirmationTokenAsync(user: user);
        mailService.SendNewUserToken(user: user, token: token);
    }

    public Task<InternalIdentityResult> CreateAsync(IOrigamUser user, string password)
    {
        Task<IdentityResult> task = coreUserManager.CreateAsync(user: user, password: password);
        IdentityResult identity = task.Result;
        List<string> errors = identity
            .Errors.Select(selector: error =>
            {
                return error.Description;
            })
            .ToList();
        if (errors.Count > 0)
        {
            return Task.FromResult(result: new InternalIdentityResult(errors: errors));
        }
        return Task.FromResult(result: InternalIdentityResult.Success);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
    {
        var user = await FindByIdAsync(userId: userId);
        return await coreUserManager.GenerateEmailConfirmationTokenAsync(user: user);
    }

    public async Task<TokenResult> GetPasswordResetTokenFromEmailAsync(string email)
    {
        IOrigamUser user = await coreUserManager.FindByEmailAsync(email: email);

        if (user == null)
        {
            return new TokenResult
            {
                Token = "",
                UserName = "",
                ErrorMessage = localizer[name: "EmailInvalid"],
                TokenValidityHours = 0,
            };
        }
        string token = await GeneratePasswordResetTokenAsync(userId: user.BusinessPartnerId);

        return new TokenResult
        {
            Token = token,
            UserName = user.UserName,
            ErrorMessage = "",
            TokenValidityHours = 24,
        };
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string userId)
    {
        var user = await FindByIdAsync(userId: userId);
        return await coreUserManager.GeneratePasswordResetTokenAsync(user: user);
    }

    public Task<XmlDocument> GetPasswordAttributesAsync()
    {
        return Task.FromResult(result: new XmlDocument());
    }

    private static InternalIdentityResult ToInternalIdentityResult(IdentityResult result)
    {
        string[] errors = result.Errors.Select(selector: error => error.Description).ToArray();
        return result.Succeeded
            ? InternalIdentityResult.Success
            : InternalIdentityResult.Failed(errors: errors);
    }

    public IOrigamUser CreateUserObject(string userName)
    {
        return new User(userName: userName);
    }
}
