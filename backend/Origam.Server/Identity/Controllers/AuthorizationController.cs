using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Origam.Server.Identity.Controllers;

[AllowAnonymous]
public class AuthorizationController : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpGet("~/connect/authorize")]
    public IActionResult AuthorizeGet()
    {
        // If the user is not signed in, send them to your login UI.
        if (!(User?.Identity?.IsAuthenticated ?? false))
        {
            return Challenge(IdentityConstants.ApplicationScheme);
        }

        // If signed-in, let the POST handler process the request (optional),
        // or complete directly here by issuing the code:
        return HandleAuthorize();
    }

    [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
    public IActionResult AuthorizePost() => HandleAuthorize();

    private IActionResult HandleAuthorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request is null)
        {
            return BadRequest();
        }

        // Build a claims principal for the current user.
        var principal = new ClaimsPrincipal(User.Identity);

        // Include scopes/resources requested by the client:
        principal.SetScopes(request.GetScopes());
        principal.SetResources("origam-local", "api"); // adapt to your resources

        // Issue the authorization code response:
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
