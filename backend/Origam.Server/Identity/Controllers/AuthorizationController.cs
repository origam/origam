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

        // Safety net: ensure "sub" exists (Option B should already provide it)
        if (!principal.HasClaim(c => c.Type == OpenIddictConstants.Claims.Subject))
        {
            // pick your canonical id: BusinessPartnerId or user.Id
            principal.SetClaim(OpenIddictConstants.Claims.Subject, user.BusinessPartnerId);
        }

        // Scopes/resources requested by the client
        principal.SetScopes(request.GetScopes());
        principal.SetResources("origam-local", "api"); // adjust to your resources

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

            // Set the scopes and resources
            principal.SetScopes(request.GetScopes());
            principal.SetResources("origam-local", "api");

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

            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            if (!principal.HasClaim(c => c.Type == OpenIddictConstants.Claims.Subject))
            {
                principal.SetClaim(OpenIddictConstants.Claims.Subject, user.BusinessPartnerId);
            }

            principal.SetScopes(request.GetScopes());
            principal.SetResources("origam-local", "api");

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return BadRequest();
    }
}
