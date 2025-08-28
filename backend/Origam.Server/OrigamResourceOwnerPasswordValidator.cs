#region license
/*
Copyright 2005 - 2023 Advantage Solutions, s. r. o.

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
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using Origam.Security.Common;

namespace Origam.Server;

class OrigamResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
{
    private readonly UserManager<IOrigamUser> userManager;
    private readonly SignInManager<IOrigamUser> signInManager;

    public OrigamResourceOwnerPasswordValidator(
        SignInManager<IOrigamUser> signInManager,
        UserManager<IOrigamUser> userManager
    )
    {
        this.signInManager = signInManager;
        this.userManager = userManager;
    }

    public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        var user = await userManager.FindByNameAsync(context.UserName);
        if (user == null)
        {
            context.Result = new GrantValidationResult(
                TokenRequestErrors.UnauthorizedClient,
                "User not found"
            );
            return;
        }
        if (!await userManager.IsEmailConfirmedAsync(user))
        {
            context.Result = new GrantValidationResult(
                TokenRequestErrors.UnauthorizedClient,
                "Mail not confirmed"
            );
            return;
        }
        SignInResult result = await signInManager.PasswordSignInAsync(
            userName: context.UserName,
            password: context.Password,
            isPersistent: false,
            lockoutOnFailure: true
        );
        if (result.Succeeded)
        {
            context.Result = new GrantValidationResult(
                user.BusinessPartnerId,
                GrantType.ResourceOwnerPassword
            );
        }
        else
        {
            string errorDescription = "";
            if (result.IsLockedOut)
            {
                errorDescription = "User locked out";
            }
            else if (result.IsNotAllowed)
            {
                errorDescription = "Not allowed";
            }
            else if (result.RequiresTwoFactor)
            {
                errorDescription = "This user requires two factor authentication";
            }
            context.Result = new GrantValidationResult(
                error: TokenRequestErrors.UnauthorizedClient,
                errorDescription: errorDescription
            );
        }
    }
}
