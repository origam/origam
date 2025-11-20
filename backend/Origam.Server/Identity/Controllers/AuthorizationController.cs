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

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Origam.Security.Common;

namespace Origam.Server.Identity.Controllers;

[AllowAnonymous]
public class AuthorizationController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly SignInManager<IOrigamUser> _signInManager;
    private readonly UserManager<IOrigamUser> _userManager;

    public AuthorizationController(
        SignInManager<IOrigamUser> signInManager,
        UserManager<IOrigamUser> userManager
    )
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet("~/connect/authorize")]
    public async Task<IActionResult> AuthorizeGet()
    {
        // Authenticate the Identity cookie explicitly (important if default scheme != cookie)
        var cookieAuth = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!cookieAuth.Succeeded)
        {
            return Challenge(IdentityConstants.ApplicationScheme); // -> /Account/Login
        }

        return await HandleAuthorizeAsync(cookieAuth.Principal!);
    }

    [ValidateAntiForgeryToken]
    [HttpPost("~/connect/authorize")]
    public async Task<IActionResult> AuthorizePost()
    {
        var cookieAuth = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!cookieAuth.Succeeded)
        {
            return Challenge(IdentityConstants.ApplicationScheme);
        }

        return await HandleAuthorizeAsync(cookieAuth.Principal!);
    }

    private async Task<IActionResult> HandleAuthorizeAsync(ClaimsPrincipal cookiePrincipal)
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request is null)
        {
            return BadRequest();
        }

        // Rebuild a full Identity principal (brings in claims from your factory, including "sub")
        var user = await _userManager.GetUserAsync(cookiePrincipal);
        if (user is null)
        {
            return Challenge(IdentityConstants.ApplicationScheme);
        }

        var principal = await _signInManager.CreateUserPrincipalAsync(user);

        if (!principal.HasClaim(c => c.Type == OpenIddictConstants.Claims.Subject))
        {
            principal.SetClaim(OpenIddictConstants.Claims.Subject, user.BusinessPartnerId);
        }
        principal.SetClaim(OpenIddictConstants.Claims.Name, user.UserName);

        SetScopes(principal, request);

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/connect/token")]
    [AllowAnonymous]
    public async Task<IActionResult> Token()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request is null)
        {
            return BadRequest();
        }

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal from the authorization code/refresh token
            var result = await HttpContext.AuthenticateAsync(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            );

            var user = await _userManager.GetUserAsync(result.Principal);
            if (user == null)
            {
                return Forbid(
                    properties: new AuthenticationProperties(
                        new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                                OpenIddictConstants.Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "The token is no longer valid.",
                        }
                    ),
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                );
            }

            // Recreate the principal
            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            // Ensure the subject claim exists
            if (!principal.HasClaim(c => c.Type == OpenIddictConstants.Claims.Subject))
            {
                principal.SetClaim(OpenIddictConstants.Claims.Subject, user.BusinessPartnerId);
            }

            SetScopes(principal, request);

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsPasswordGrantType())
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true
            );
            if (!result.Succeeded)
            {
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            ClaimsPrincipal principal = await _signInManager.CreateUserPrincipalAsync(user);
            if (!principal.HasClaim(c => c.Type == OpenIddictConstants.Claims.Subject))
            {
                principal.SetClaim(OpenIddictConstants.Claims.Subject, user.BusinessPartnerId);
            }

            SetScopes(principal, request);
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return BadRequest();
    }

    private static void SetScopes(ClaimsPrincipal principal, OpenIddictRequest request)
    {
        principal.SetScopes(request.GetScopes());
        principal.SetResources("origam-local", "api");
        principal.SetDestinations(claim =>
            claim.Type switch
            {
                OpenIddictConstants.Claims.Name => new[]
                {
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken,
                },

                OpenIddictConstants.Claims.Subject => new[]
                {
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken,
                },

                _ => new[] { OpenIddictConstants.Destinations.AccessToken },
            }
        );
    }

    [HttpGet("~/connect/logout")]
    [HttpPost("~/connect/logout")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Logout()
    {
        OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest();
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        return SignOut(
            properties: new AuthenticationProperties
            {
                RedirectUri = request?.PostLogoutRedirectUri ?? Url.Content("~/"),
            },
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
        );
    }
}
