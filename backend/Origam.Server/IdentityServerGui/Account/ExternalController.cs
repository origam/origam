using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.Security.Common;
using Origam.Server.Configuration;
using Origam.Service.Core;

namespace Origam.Server.IdentityServerGui.Account;

[SecurityHeaders]
[AllowAnonymous]
public class ExternalController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly UserManager<IOrigamUser> userManager;
    private readonly IIdentityServerInteractionService interaction;
    private readonly ILogger<ExternalController> logger;
    private readonly IEventService events;
    private readonly IdentityServerConfig identityServerConfig;
    private readonly IAuthenticationPostProcessor
        authenticationPostProcessor;

    public ExternalController(
        IIdentityServerInteractionService interaction,
        IEventService events,
        ILogger<ExternalController> logger, 
        UserManager<IOrigamUser> userManager,
        IdentityServerConfig identityServerConfig,
        IAuthenticationPostProcessor authenticationPostProcessor)
    {

        this.interaction = interaction;
        this.events = events;
        this.logger = logger;
        this.userManager = userManager;
        this.identityServerConfig = identityServerConfig;
        this.authenticationPostProcessor = authenticationPostProcessor;
    }

    /// <summary>
    /// initiate roundtrip to external authentication provider
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Challenge(string provider, string returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

        // validate returnUrl - either it is a valid OIDC URL or back to a local page
        if (Url.IsLocalUrl(returnUrl) == false && interaction.IsValidReturnUrl(returnUrl) == false)
        {
            // user might have clicked on a malicious link - should be logged
            throw new Exception("invalid return URL");
        }
        if (AccountOptions.WindowsAuthenticationSchemeName == provider)
        {
            // windows authentication needs special handling
            return await ProcessWindowsLoginAsync(returnUrl);
        }
        else
        {
            // start challenge and roundtrip the return URL and scheme 
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback)),
                Items =
                {
                    {"returnUrl", returnUrl},
                    {"scheme", provider},
                }
            };
            return Challenge(props, provider);
        }
    }
    private async Task<IActionResult> ProcessWindowsLoginAsync(string returnUrl)
    {
        // see if windows auth has already been requested and succeeded
        var result = await HttpContext.AuthenticateAsync(AccountOptions.WindowsAuthenticationSchemeName);
        if (result?.Principal is WindowsPrincipal wp)
        {
            // we will issue the external cookie and then redirect the
            // user back to the external callback, in essence, treating windows
            // auth the same as any other external authentication mechanism
            var props = new AuthenticationProperties()
            {
                RedirectUri = Url.Action("Callback"),
                Items =
                {
                    { "returnUrl", returnUrl },
                    { "scheme", AccountOptions.WindowsAuthenticationSchemeName },
                }
            };

            var id = new ClaimsIdentity(AccountOptions.WindowsAuthenticationSchemeName);
            id.AddClaim(new Claim(JwtClaimTypes.Subject, wp.FindFirst(ClaimTypes.PrimarySid).Value));
            id.AddClaim(new Claim(JwtClaimTypes.Name, wp.Identity.Name));

            // add the groups as claims -- be careful if the number of groups is too large
            if (AccountOptions.IncludeWindowsGroups)
            {
                var wi = wp.Identity as WindowsIdentity;
                var groups = wi.Groups.Translate(typeof(NTAccount));
                var roles = groups.Select(x => new Claim(JwtClaimTypes.Role, x.Value));
                id.AddClaims(roles);
            }

            await HttpContext.SignInAsync(
                IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme,
                new ClaimsPrincipal(id),
                props);
            return Redirect(props.RedirectUri);
        }
        else
        {
            // trigger windows auth
            // since windows auth don't support the redirect uri,
            // this URL is re-triggered when we call challenge
            return Challenge(AccountOptions.WindowsAuthenticationSchemeName);
        }
    }
        
    /// <summary>
    /// Post processing of external authentication
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Callback()
    {
        // read external identity from the temporary cookie
        var result = await HttpContext.AuthenticateAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);
        if (result?.Succeeded != true)
        {
            throw new Exception("External authentication error");
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            var externalClaims = result.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
            logger.LogDebug("External claims: {@claims}", externalClaims);
        }

        // lookup our user and external provider info
        var (user, provider, providerUserId, claims) = FindUserFromExternalProvider(result);
        if (user == null)
        {
            // this might be where you might initiate a custom workflow for user registration
            // in this sample we don't show how that would be done, as our sample implementation
            // simply auto-provisions new external user
            //user = AutoProvisionUser(provider, providerUserId, claims);
            return Redirect("/Account/AccessDenied");
        }

        // this allows us to collect any additional claims or properties
        // for the specific protocols used and store them in the local auth cookie.
        // this is typically used to store data needed for signout from those protocols.
        var additionalLocalClaims = new List<Claim>();
        var localSignInProps = new AuthenticationProperties();
        ProcessLoginCallback(result, additionalLocalClaims, localSignInProps);

        // issue authentication cookie for user
        var isuser = new IdentityServerUser(user.BusinessPartnerId)
        {
            DisplayName = user.UserName,
            IdentityProvider = provider,
            AdditionalClaims = additionalLocalClaims
        };

        await HttpContext.SignInAsync(isuser, localSignInProps);

        // delete temporary cookie used during external authentication
        await HttpContext.SignOutAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);

        // retrieve return URL
        var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

        // check if external login is in the context of an OIDC request
        var context = await interaction.GetAuthorizationContextAsync(returnUrl);
        await events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.BusinessPartnerId, user.UserName, true, context?.Client.ClientId));

        if (context != null)
        {
            if (context.IsNativeClient())
            {
                // if the client is PKCE then we assume it's native, so this change in how to
                // return the response is for better UX for the end user.
                return this.LoadingPage("Redirect", returnUrl);
            }
        }

        return Redirect(returnUrl);
    }
        
    private 
        (IOrigamUser user, 
        string provider, 
        string providerUserId, 
        IEnumerable<Claim> claims) FindUserFromExternalProvider(
            AuthenticateResult result)
    {
        var externalUser = result.Principal;
        // try to determine the unique id of the external user
        // (issued by the provider)
        // the most common claim type for that are the sub claim and
        // the NameIdentifier depending on the external provider,
        // some other claim types might be used
        var userIdClaim 
            = externalUser.FindFirst(JwtClaimTypes.Subject) 
              ?? externalUser.FindFirst(ClaimTypes.NameIdentifier) 
              ?? throw new Exception(
                  $"User identifier claim wasn't found. Checked claims " 
                  + $"were {JwtClaimTypes.Subject} " 
                  + $"and {ClaimTypes.NameIdentifier}");
        // remove the user id claim so we don't include it as an extra claim
        // if/when we provision the user
        var claims = externalUser.Claims.ToList();
        claims.Remove(userIdClaim);
        var provider = result.Properties.Items["scheme"];
        var providerUserId = userIdClaim.Value;
        var externalCallbackProcessingInfo 
            = identityServerConfig.GetExternalCallbackProcessingInfo(
                provider);
        var checkedClaim = externalUser.FindFirst(claim 
            => claim.Type == externalCallbackProcessingInfo.ClaimType);
        if (checkedClaim == null)
        {
            logger.LogError("ClaimType {0} not found.", 
                externalCallbackProcessingInfo.ClaimType);
            throw new Exception("Failed to locate checked claim.");
        }
        var user = externalCallbackProcessingInfo.AuthenticationType
                   == AuthenticationType.Email 
            ? userManager.FindByEmailAsync(checkedClaim.Value).Result 
            : userManager.FindByNameAsync(checkedClaim.Value).Result;
        if (user == null)
        {
            return (null, provider, providerUserId, claims);
        }
        // time for authentication post-processing
        // the default post processor returns always true 
        var postProcessorResult = authenticationPostProcessor.Validate(
            user.BusinessPartnerId, user.UserName, provider, 
            providerUserId, claims);
        return (postProcessorResult ? user : null, provider, providerUserId, 
            claims);
    }

    private IOrigamUser AutoProvisionUser(string provider, string providerUserId, IEnumerable<Claim> claims)
    {
        //var user = _users.AutoProvisionUser(provider, providerUserId, claims.ToList());
        // TODO: implement AutoProvisionUser using _userManager
        throw new NotImplementedException();
    }

    private void ProcessLoginCallbackForOidc(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
    {
        // if the external system sent a session id claim, copy it over
        // so we can use it for single sign-out
        var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
        if (sid != null)
        {
            localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
        }

        // if the external provider issued an id_token, we'll keep it for signout
        var id_token = externalResult.Properties.GetTokenValue("id_token");
        if (id_token != null)
        {
            localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = id_token } });
        }
    }
        
    // if the external login is OIDC-based, there are certain things we need to preserve to make logout work
    // this will be different for WS-Fed, SAML2p or other protocols
    private void ProcessLoginCallback(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
    {
        // if the external system sent a session id claim, copy it over
        // so we can use it for single sign-out
        var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
        if (sid != null)
        {
            localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
        }

        // if the external provider issued an id_token, we'll keep it for signout
        var idToken = externalResult.Properties.GetTokenValue("id_token");
        if (idToken != null)
        {
            localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = idToken } });
        }
    }
}