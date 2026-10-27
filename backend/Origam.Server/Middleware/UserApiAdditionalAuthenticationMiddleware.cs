#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Validation.AspNetCore;
using Origam.Server.Configuration;

namespace Origam.Server.Middleware;

public class UserApiAdditionalAuthenticationMiddleware(
    RequestDelegate next,
    OpenIddictConfig configuration
)
{
    public async Task Invoke(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            foreach (
                AuthenticationMethod method in configuration.GetPrivateApiAuthenticationMethods(
                    context.Request.Path.Value
                )
            )
            {
                if (method == configuration.PrivateApiAuthenticationMethod)
                {
                    continue;
                }

                string scheme =
                    method == AuthenticationMethod.Cookie
                        ? IdentityConstants.ApplicationScheme
                        : OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                AuthenticateResult result = await context.AuthenticateAsync(scheme);
                if (result.Succeeded)
                {
                    context.User = result.Principal;
                    var features = new OrigamAuthenticationFeatures(result);
                    context.Features.Set<IHttpAuthenticationFeature>(features);
                    context.Features.Set<IAuthenticateResultFeature>(features);
                    break;
                }
            }
        }
        await next(context);
    }
}
