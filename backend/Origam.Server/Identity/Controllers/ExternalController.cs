using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.Security.Common;
using Origam.Server.Configuration;
using Origam.Service.Core;

namespace Origam.Server.Identity.Controllers;

[AllowAnonymous]
public class ExternalController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly UserManager<IOrigamUser> userManager;
    private readonly SignInManager<IOrigamUser> signInManager;
    private readonly ILogger<ExternalController> logger;
    private readonly IdentityServerConfig identityServerConfig;
    private readonly IAuthenticationPostProcessor authenticationPostProcessor;

    public ExternalController(
        ILogger<ExternalController> logger,
        UserManager<IOrigamUser> userManager,
        SignInManager<IOrigamUser> signInManager,
        IdentityServerConfig identityServerConfig,
        IAuthenticationPostProcessor authenticationPostProcessor
    )
    {
        this.logger = logger;
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.identityServerConfig = identityServerConfig;
        this.authenticationPostProcessor = authenticationPostProcessor;
    }

    /// <summary>
    /// Initiate roundtrip to external authentication provider.
    /// </summary>
    [HttpGet]
    public IActionResult Challenge(string provider, string returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl))
        {
            returnUrl = Url.Content("~/");
        }

        // Only allow local URLs to avoid open redirects.
        if (!Url.IsLocalUrl(returnUrl))
        {
            throw new Exception("Invalid return URL");
        }

        var redirectUrl = Url.Action(nameof(Callback), "External", new { returnUrl });
        var props = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        return Challenge(props, provider);
    }

    /// <summary>
    /// Post-processing of external authentication.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Callback(string returnUrl = null, string remoteError = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!Url.IsLocalUrl(returnUrl))
        {
            returnUrl = Url.Content("~/");
        }

        if (remoteError != null)
        {
            logger.LogWarning("External provider error: {RemoteError}", remoteError);
            return RedirectToAction("Login", "Account", new { returnUrl });
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            logger.LogWarning("GetExternalLoginInfoAsync returned null.");
            return RedirectToAction("Login", "Account", new { returnUrl });
        }

        var (user, provider, providerUserId, claims) = FindUserFromExternalProvider(info);
        if (user == null)
        {
            logger.LogWarning(
                "External login rejected. Provider: {Provider}, ProviderUserId: {ProviderUserId}",
                provider,
                providerUserId
            );

            // Clear external cookie if any.
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return Redirect("/Account/AccessDenied");
        }

        // At this point, the user is accepted – sign in with the normal app cookie.
        await signInManager.SignInAsync(user, isPersistent: false);

        // Clear external cookie explicitly (SignInManager does this in some flows, we are explicit here).
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        return LocalRedirect(returnUrl);
    }

    private (
        IOrigamUser user,
        string provider,
        string providerUserId,
        IEnumerable<Claim> claims
    ) FindUserFromExternalProvider(ExternalLoginInfo info)
    {
        if (info.Principal == null)
        {
            throw new Exception("External principal is null.");
        }

        var externalUser = info.Principal;
        var provider = info.LoginProvider;
        var providerUserId = info.ProviderKey;
        var claims = externalUser.Claims.ToList();

        var externalCallbackProcessingInfo =
            identityServerConfig.GetExternalCallbackProcessingInfo(provider);

        var checkedClaim = externalUser.FindFirst(claim =>
            claim.Type == externalCallbackProcessingInfo.ClaimType
        );

        if (checkedClaim == null)
        {
            logger.LogError(
                "ClaimType {ClaimType} not found in external principal.",
                externalCallbackProcessingInfo.ClaimType
            );
            return (null, provider, providerUserId, claims);
        }

        IOrigamUser user =
            externalCallbackProcessingInfo.AuthenticationType == AuthenticationType.Email
                ? userManager.FindByEmailAsync(checkedClaim.Value).GetAwaiter().GetResult()
                : userManager.FindByNameAsync(checkedClaim.Value).GetAwaiter().GetResult();

        if (user == null)
        {
            // No auto-provision here – behave like the original code.
            return (null, provider, providerUserId, claims);
        }

        var postProcessorResult = authenticationPostProcessor.Validate(
            user.BusinessPartnerId,
            user.UserName,
            provider,
            providerUserId,
            claims
        );

        return postProcessorResult
            ? (user, provider, providerUserId, claims)
            : (null, provider, providerUserId, claims);
    }
}
