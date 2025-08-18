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
public class CoreManagerAdapter: IManager
{
    private readonly CoreUserManager<IOrigamUser> coreUserManager;
    private readonly IMailService mailService;
    protected static readonly ILog log
        = LogManager.GetLogger(typeof(CoreManagerAdapter));
    private readonly IStringLocalizer<SharedResources> localizer;
    public CoreManagerAdapter(CoreUserManager<IOrigamUser> coreUserManager,
        IMailService mailService, IStringLocalizer<SharedResources> localizer)
    {
        this.coreUserManager = coreUserManager;
        this.mailService = mailService;
        this.localizer = localizer;
    }
    public async Task<IOrigamUser> FindByNameAsync(string name,string transaction = null)
    {
       return await coreUserManager.FindByNameAsync(name, transaction);
    }
    private async Task<IOrigamUser> FindByIdAsync(string userId)
    {
        var user =  await coreUserManager.FindByIdAsync(userId);
        if(log.IsDebugEnabled)
        {
            log.DebugFormat("User not found...");
        }
        return user;
    }
    public Task<bool> ChangePasswordQuestionAndAnswerAsync(string userName, string password,
        string question, string answer)
    {
        return Task.FromResult(false);
    }
    public async Task<bool> IsLockedOutAsync(string userId)
    {
        var user = await FindByIdAsync(userId);
        if (user == null) { return false; }
        return await coreUserManager.IsLockedOutAsync(user);
    }
    public async Task<bool> GetTwoFactorEnabledAsync(string userId)
    {
        var user = await FindByIdAsync(userId);
        if (user == null) { return false; }
        return await coreUserManager.GetTwoFactorEnabledAsync(user);
    }
    public async Task<bool> SetTwoFactorEnabledAsync(string userId, bool enabled)
    {
       var user = await FindByIdAsync(userId);
        if (user == null) { return false; }
        return (await coreUserManager.SetTwoFactorEnabledAsync(user, enabled))
           .Succeeded;
    }
    public async Task<bool> IsEmailConfirmedAsync(string userId)
    {
        var user = await FindByIdAsync(userId);
        if (user == null) { return false; }
        return await coreUserManager.IsEmailConfirmedAsync(user);
    }
    public async Task<bool> UnlockUserAsync(string userName)
    {
        var user = await FindByNameAsync(userName);
        if (user == null) { return false; }
        Task<IdentityResult> unlockTask = coreUserManager.SetLockoutEndDateAsync( user, null);
        bool success = (await unlockTask).Succeeded;
        if (success)
        {
            mailService.SendUserUnlockedMessage(user);
        }
        return success;
    }
    public async Task<InternalIdentityResult> ConfirmEmailAsync(string userId)
    {
        var user = await FindByIdAsync(userId);
        string token = await coreUserManager.GenerateEmailConfirmationTokenAsync(user);
        IdentityResult coreIdentityResult =
            await coreUserManager.ConfirmEmailAsync(user, token);
        return ToInternalIdentityResult(coreIdentityResult);
    }
    public async Task<InternalIdentityResult> ConfirmEmailAsync(string userId, string token)
    {
        var user = await FindByIdAsync(userId);
        IdentityResult coreIdentityResult = 
            await coreUserManager.ConfirmEmailAsync(user, token);
        return ToInternalIdentityResult(coreIdentityResult);
    }
    public async Task<InternalIdentityResult> ChangePasswordAsync(string userId, string currentPassword,
        string newPassword)
    {
        var user = await FindByIdAsync(userId);
        IdentityResult coreIdentityResult = 
            await coreUserManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return ToInternalIdentityResult(coreIdentityResult);
    }
    public async Task<InternalIdentityResult> ResetPasswordFromUsernameAsync(string userName, string token,
        string newPassword)
    {
        var user = await FindByNameAsync(userName);
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
        IdentityResult coreIdentityResult = await coreUserManager.UpdateAsync(user);
        return ToInternalIdentityResult(coreIdentityResult);
    }
    public async void SendNewUserToken(string userName)
    {
        IOrigamUser user = await FindByIdAsync(userName);
        string token =
            await coreUserManager.GenerateEmailConfirmationTokenAsync(user);
        mailService.SendNewUserToken(user,token);
    }
    public Task<InternalIdentityResult> CreateAsync(IOrigamUser user, string password)
    {
        Task<IdentityResult> task = coreUserManager.CreateAsync(user, password);
        IdentityResult identity = task.Result;
        List<string> errors = identity.Errors.Select(error => { return error.Description; }).ToList();
        if (errors.Count > 0)
        {
            return Task.FromResult(new InternalIdentityResult(errors));
        }
        return Task.FromResult(InternalIdentityResult.Success);
    }
    public async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
    {
        var user = await FindByIdAsync(userId);
        return await coreUserManager.GenerateEmailConfirmationTokenAsync(user);
    }
    public async Task<TokenResult> GetPasswordResetTokenFromEmailAsync(string email)
    {
        IOrigamUser user = await coreUserManager.FindByEmailAsync(email);
        
        if (user == null)
        {
            return new TokenResult
            { Token = "", UserName = "",
                ErrorMessage = localizer["EmailInvalid"], 
                TokenValidityHours = 0};
        }
        string token = await GeneratePasswordResetTokenAsync(user.BusinessPartnerId);
        
        return new TokenResult
        { Token = token,
            UserName = user.UserName, ErrorMessage = "",
            TokenValidityHours = 24};
    }
    public async Task<string> GeneratePasswordResetTokenAsync(string userId)
    {
        var user = await FindByIdAsync(userId);
        return await coreUserManager.GeneratePasswordResetTokenAsync(user);
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
    public IOrigamUser CreateUserObject(string userName)
    {
        return new User(userName);
    }
}
