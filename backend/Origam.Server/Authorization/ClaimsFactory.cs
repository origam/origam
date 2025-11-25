#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using Origam.Security.Common;

namespace Origam.Server.Authorization;

public class ClaimsFactory : UserClaimsPrincipalFactory<IOrigamUser>
{
    public ClaimsFactory(UserManager<IOrigamUser> userManager, IOptions<IdentityOptions> options)
        : base(userManager, options) { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IOrigamUser user)
    {
        var id = await base.GenerateClaimsAsync(user);
        id.RemoveClaim(id.FindFirst(OpenIddictConstants.Claims.Subject));
        id.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, user.BusinessPartnerId));
        id.AddClaim(new Claim(OpenIddictConstants.Claims.Name, user.UserName ?? ""));
        if (!string.IsNullOrEmpty(user.Email))
        {
            id.AddClaim(new Claim(OpenIddictConstants.Claims.Email, user.Email));
        }

        return id;
    }
}
