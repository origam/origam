// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Origam.Security.Common;
using Origam.Server.Authorization;
using Origam.Server.Configuration;
using Origam.Server.IdentityServerGui.Home;

namespace Origam.Server.IdentityServerGui.Account;

[SecurityHeaders]
[AllowAnonymous]
[Route("/account/[action]")]
public class AccountController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly UserManager<IOrigamUser> _userManager;
    private readonly SignInManager<IOrigamUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IClientStore _clientStore;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IEventService _events;
    private readonly IMailService _mailService;
    private readonly UserConfig _userConfig;
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly IPersistedGrantStore _persistedGrantStore;
    private readonly SessionObjects _sessionObjects;
    private readonly ILogger<UserManager<IOrigamUser>> _logger;
    private readonly IdentityGuiConfig _configOptions;
    private readonly RequestLocalizationOptions _requestLocalizationOptions;
    private readonly ResourceManager resourceManager = new ResourceManager(
        "Origam.Server.SharedResources",
        Assembly.GetExecutingAssembly()
    );

    public AccountController(
        UserManager<IOrigamUser> userManager,
        SignInManager<IOrigamUser> signInManager,
        IIdentityServerInteractionService interaction,
        IClientStore clientStore,
        IAuthenticationSchemeProvider schemeProvider,
        IEventService events,
        IMailService mailService,
        IOptions<UserConfig> userConfig,
        IStringLocalizer<SharedResources> localizer,
        IPersistedGrantStore persistedGrantStore,
        SessionObjects sessionObjects,
        IOptions<RequestLocalizationOptions> requestLocalizationOptions,
        IOptions<IdentityGuiConfig> configOptions,
        ILogger<UserManager<IOrigamUser>> logger
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _interaction = interaction;
        _clientStore = clientStore;
        _schemeProvider = schemeProvider;
        _events = events;
        _mailService = mailService;
        _localizer = localizer;
        _persistedGrantStore = persistedGrantStore;
        _sessionObjects = sessionObjects;
        _logger = logger;
        _configOptions = configOptions.Value;
        _userConfig = userConfig.Value;
        _requestLocalizationOptions = requestLocalizationOptions.Value;
    }

    /// <summary>
    /// Entry point into the login workflow
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Login(string returnUrl)
    {
        // build a model so we know what to show on the login page
        var vm = await BuildLoginViewModelAsync(returnUrl);
        if (vm.IsExternalLoginOnly)
        {
            // we only have one option for logging in and it's an external provider
            return RedirectToAction(
                "Challenge",
                "External",
                new { provider = vm.ExternalLoginScheme, returnUrl }
            );
        }
        return View(vm);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword(string returnUrl = null)
    {
        if (!_configOptions.AllowPasswordReset)
        {
            return RedirectToAction(nameof(Login), "Account");
        }
        var model = new ForgotPasswordViewModel
        {
            ReturnUrl = ExtractRedirectUriFromReturnUrl(returnUrl),
        };
        return View(model);
    }

    private string ExtractRedirectUriFromReturnUrl(string returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl))
        {
            return "/";
        }
        string decodedUrl = Uri.UnescapeDataString(returnUrl);
        string pattern = @"redirect_uri=([^&#]+)";
        Match match = Regex.Match(decodedUrl, pattern);
        return match.Success ? match.Groups[1].Value : "/";
    }

    // POST: /Account/ForgotPassword
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!_configOptions.AllowPasswordReset)
        {
            return RedirectToAction(nameof(Login), "Account");
        }
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist or is not confirmed
                _logger.LogWarning("ForgotPassword - " + model.Email + " User does not exist.");
                return View("ForgotPasswordConfirmation");
            }
            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
            // Send an email with this link
            var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(
                "ResetPassword",
                "Account",
                new { userId = user.BusinessPartnerId, code = passwordResetToken },
                protocol: HttpContext.Request.Scheme
            );
            _mailService.SendPasswordResetToken(
                user,
                passwordResetToken,
                model.ReturnUrl,
                tokenValidityHours: 24
            );
            _logger.LogInformation("ForgotPassword - " + model.Email + " Mail was sent.");
            return View("ForgotPasswordConfirmation");
        }
        // If we got this far, something failed, redisplay form
        return View(model);
    }

    //
    // GET: /Account/Register
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string returnUrl = null)
    {
        if (!_userConfig.UserRegistrationAllowed)
        {
            return View("Error", new ErrorViewModel(_localizer["RegistrationNotAllowed"]));
        }
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    //
    // POST: /Account/Register
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!_userConfig.UserRegistrationAllowed)
        {
            return View("Error", new ErrorViewModel(_localizer["RegistrationNotAllowed"]));
        }
        if (ModelState.IsValid)
        {
            IOrigamUser user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                Name = model.Name,
                RoleId = _userConfig.NewUserRoleId,
            };
            IdentityResult result = UserTools.RunCreateUserWorkFlow(model.Password, user);
            user = await _userManager.FindByNameAsync(user.UserName);
            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                _mailService.SendNewUserToken(user, code);
                return RedirectToAction(nameof(RegisterConfirmation), "Account");
            }
            AddErrors(result);
        }
        return View(model);
    }

    //
    // GET: /Account/RegisterInitialUser
    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterInitialUser()
    {
        if (!UserTools.IsInitialSetupNeeded())
        {
            return View("Error", new ErrorViewModel(_localizer["AlreadySetUp"]));
        }
        return View();
    }

    //
    // POST: /Account/RegisterInitialUser
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterInitialUser(RegisterViewModel model)
    {
        if (!UserTools.IsInitialSetupNeeded())
        {
            return View("Error", new ErrorViewModel(_localizer["AlreadySetUp"]));
        }

        if (ModelState.IsValid)
        {
            IOrigamUser user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                Name = model.Name,
                RoleId = SecurityManager.BUILTIN_SUPER_USER_ROLE,
                SecurityStamp = "",
            };
            IdentityResult result = UserTools.RunCreateUserWorkFlow(model.Password, user);
            user = await _userManager.FindByNameAsync(user.UserName);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                string emailConfirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(
                    user
                );
                await _userManager.ConfirmEmailAsync(user, emailConfirmToken);
                UserTools.SetInitialSetupComplete();
                return Redirect("/");
            }
            AddErrors(result);
        }
        return View(model);
    }

    // GET: /Account/ConfirmEmail
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (!_userConfig.UserRegistrationAllowed)
        {
            return View("Error", new ErrorViewModel(_localizer["RegistrationNotAllowed"]));
        }
        if (userId == null || code == null)
        {
            _logger.LogWarning($"Invalid confirm email data: userId:\"{userId}\", code:\"{code}\"");
            return View("Error");
        }
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning($"User not found: userId:\"{userId}\"");
            return View("Error");
        }
        var result = await _userManager.ConfirmEmailAsync(user, code);
        if (result.Succeeded)
        {
            return View("EmailConfirmation");
        }

        string errors = string.Join("\n", result.Errors.Select(error => error.Description));
        _logger.LogWarning($"ConfirmEmailAsync failed, errors:\"{errors}\"");
        return View("Error");
    }

    // GET: /Account/RegisterConfirmation
    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterConfirmation()
    {
        return View();
    }

    // GET: /Account/ResetPassword
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(
        string code = null,
        string mail = null,
        string returnUrl = null
    )
    {
        if (!_configOptions.AllowPasswordReset)
        {
            return RedirectToAction(nameof(Login), "Account");
        }
        if (code == null)
        {
            _logger.LogWarning($"Code supplied to {nameof(ResetPassword)} was null");
            return View("Error");
        }
        if (mail == null)
        {
            _logger.LogWarning($"mail supplied to {nameof(ResetPassword)} was null");
            return View("Error");
        }
        var model = new ResetPasswordViewModel
        {
            Email = mail,
            ReturnUrl = string.IsNullOrEmpty(returnUrl) ? null : Uri.UnescapeDataString(returnUrl),
        };
        return View(model);
    }

    // POST: /Account/ResetPassword
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!_configOptions.AllowPasswordReset)
        {
            return RedirectToAction(nameof(Login), "Account");
        }
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return RedirectToAction(
                nameof(ResetPasswordConfirmation),
                controllerName: "Account",
                routeValues: new
                {
                    returnUrl = Uri.EscapeDataString(model.ReturnUrl ?? "/account/login"),
                }
            );
        }
        user.EmailConfirmed = true;
        var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
        if (result.Succeeded)
        {
            return RedirectToAction(
                nameof(ResetPasswordConfirmation),
                controllerName: "Account",
                routeValues: new
                {
                    returnUrl = Uri.EscapeDataString(model.ReturnUrl ?? "/account/login"),
                }
            );
        }
        AddErrors(result);
        return View();
    }

    // GET: /Account/ResetPasswordConfirmation
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation(string returnUrl = null)
    {
        var model = new ResetPasswordConfirmationViewModel
        {
            ReturnUrl = string.IsNullOrEmpty(returnUrl) ? null : Uri.UnescapeDataString(returnUrl),
        };
        return View(model);
    }

    /// <summary>
    /// Handle postback from username/password login
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginInputModel model, string button)
    {
        // check if we are in the context of an authorization request
        var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
        // the user clicked the "cancel" button
        if (button != "login")
        {
            if (context != null)
            {
                // if the user cancels, send a result back into IdentityServer as if they
                // denied the consent (even if this client does not require consent).
                // this will send back an access denied OIDC error response to the client.
                await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);
                // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                if (context.IsNativeClient())
                {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage("Redirect", model.ReturnUrl);
                }
                return Redirect(model.ReturnUrl);
            }

            // since we don't have a valid context, then we just go back to the home page
            return Redirect("~/");
        }
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
            {
                return View("EmailNotConfirmed");
            }
            var result = await _signInManager.PasswordSignInAsync(
                model.Username,
                model.Password,
                false,
                lockoutOnFailure: true
            );
            if (result.Succeeded && user != null)
            {
                await _events.RaiseAsync(
                    new UserLoginSuccessEvent(
                        user.UserName,
                        user.UserName,
                        user.Name,
                        clientId: context?.Client.ClientId
                    )
                );

                if (context != null)
                {
                    if (context.IsNativeClient())
                    {
                        // The client is native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage("Redirect", model.ReturnUrl);
                    }
                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    return Redirect(model.ReturnUrl);
                }
                // request for a local page
                if (Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                if (string.IsNullOrEmpty(model.ReturnUrl))
                {
                    return Redirect("~/");
                }

                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");
            }

            if (result.IsLockedOut)
            {
                await _events.RaiseAsync(
                    new UserLoginFailureEvent(
                        model.Username,
                        "invalid credentials",
                        clientId: context?.Client.ClientId
                    )
                );
                ModelState.AddModelError(string.Empty, _localizer["UserLockedOut"]);
            }
            else
            {
                await _events.RaiseAsync(
                    new UserLoginFailureEvent(
                        model.Username,
                        "invalid credentials",
                        clientId: context?.Client.ClientId
                    )
                );
                ModelState.AddModelError(string.Empty, _localizer["InvalidLogin"]);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(
                    nameof(LoginTwoStep),
                    new { user.UserName, model.ReturnUrl }
                );
            }
        }
        // something went wrong, show form with error
        var vm = await BuildLoginViewModelAsync(model);
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> LoginTwoStep(
        string userName,
        bool rememberLogin,
        string returnUrl = null
    )
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return View(
                "Error",
                new ErrorViewModel(resourceManager.GetString("ErrorUserNotFound"))
            );
        }
        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            return View("EmailNotConfirmed");
        }
        var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
        if (!providers.Contains("Email"))
        {
            return View("Error", new ErrorViewModel("Email provider not found."));
        }
        var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
        _mailService.SendMultiFactorAuthCode(user, token);
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["RememberLogin"] = rememberLogin;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginTwoStep(TwoStepModel twoStepModel)
    {
        if (!ModelState.IsValid)
        {
            return View(twoStepModel);
        }
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return View("Error", new ErrorViewModel(_localizer["LoginFailedUnknown"]));
        }
        var result = await _signInManager.TwoFactorSignInAsync(
            "Email",
            twoStepModel.TwoFactorCode,
            false,
            rememberClient: false
        );
        if (result.Succeeded)
        {
            if (Url.IsLocalUrl(twoStepModel.ReturnUrl))
            {
                return Redirect(twoStepModel.ReturnUrl);
            }
            if (string.IsNullOrEmpty(twoStepModel.ReturnUrl))
            {
                return Redirect("~/");
            }
            throw new Exception("invalid return URL");
        }

        if (result.IsLockedOut)
        {
            //Same logic as in the Login action
            ModelState.AddModelError("", resourceManager.GetString("UserLockedOut"));
            return View();
        }

        ModelState.AddModelError("", resourceManager.GetString("LoginFailedWrongCode"));
        return View();
    }

    /// <summary>
    /// Show logout page
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Logout(string logoutId)
    {
        // build a model so the logout page knows what to display
        var vm = await BuildLogoutViewModelAsync(logoutId);
        if (vm.ShowLogoutPrompt == false)
        {
            // if the request for logout was properly authenticated from IdentityServer, then
            // we don't need to show the prompt and can just log the user out directly.
            return await Logout(vm);
        }
        return View(vm);
    }

    /// <summary>
    /// Handle logout page postback
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(LogoutInputModel model)
    {
        // build a model so the logged out page knows what to display
        var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);
        if (User?.Identity.IsAuthenticated == true)
        {
            // delete local authentication cookie
            await _signInManager.SignOutAsync();
            _sessionObjects.UIService.Logout();

            var subjectId = HttpContext.User.Identity.GetSubjectId();
            var filter = new PersistedGrantFilter
            {
                SubjectId = subjectId,
                ClientId = vm.ClientName,
            };
            await _persistedGrantStore.RemoveAllAsync(filter);
            // raise the logout event
            await _events.RaiseAsync(
                new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName())
            );
        }
        // check if we need to trigger sign-out at an upstream identity provider
        if (vm.TriggerExternalSignout)
        {
            // build a return URL so the upstream provider will redirect back
            // to us after the user has logged out. this allows us to then
            // complete our single sign-out processing.
            string url = Url.Action("Logout", new { logoutId = vm.LogoutId });
            // this triggers a redirect to the external provider for sign-out
            return SignOut(
                new AuthenticationProperties { RedirectUri = url },
                vm.ExternalAuthenticationScheme
            );
        }
        return View("LoggedOut", vm);
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    /*****************************************/
    /* helper APIs for the AccountController */
    /*****************************************/
    private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
    {
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
        {
            var local = context.IdP == IdentityServerConstants.LocalIdentityProvider;
            // this is meant to short circuit the UI and only trigger the one external IdP
            var vm = new LoginViewModel
            {
                EnableLocalLogin = local,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
            };
            if (!local)
            {
                vm.ExternalProviders = new[]
                {
                    new ExternalProvider { AuthenticationScheme = context.IdP },
                };
            }
            return vm;
        }
        var schemes = await _schemeProvider.GetAllSchemesAsync();
        var providers = schemes
            .Where(x =>
                x.DisplayName != null
                || (
                    x.Name.Equals(
                        AccountOptions.WindowsAuthenticationSchemeName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
            )
            .Select(x => new ExternalProvider
            {
                DisplayName = x.DisplayName,
                AuthenticationScheme = x.Name,
            })
            .ToList();
        var allowLocal = true;
        if (context?.Client.ClientId != null)
        {
            var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
            if (client != null)
            {
                allowLocal = client.EnableLocalLogin;
                if (
                    client.IdentityProviderRestrictions != null
                    && client.IdentityProviderRestrictions.Any()
                )
                {
                    providers = providers
                        .Where(provider =>
                            client.IdentityProviderRestrictions.Contains(
                                provider.AuthenticationScheme
                            )
                        )
                        .ToList();
                }
            }
        }
        return new LoginViewModel
        {
            EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
            ReturnUrl = returnUrl,
            Username = context?.LoginHint,
            ExternalProviders = providers.ToArray(),
        };
    }

    private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
    {
        var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
        vm.Username = model.Username;
        return vm;
    }

    private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
    {
        var vm = new LogoutViewModel
        {
            LogoutId = logoutId,
            ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt,
        };
        if (User?.Identity.IsAuthenticated != true)
        {
            // if the user is not authenticated, then just show logged out page
            vm.ShowLogoutPrompt = false;
            return vm;
        }
        var context = await _interaction.GetLogoutContextAsync(logoutId);
        if (context?.ShowSignoutPrompt == false)
        {
            // it's safe to automatically sign-out
            vm.ShowLogoutPrompt = false;
            return vm;
        }
        // show the logout prompt. this prevents attacks where the user
        // is automatically signed out by another malicious web page.
        return vm;
    }

    private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
    {
        // get context information (client name, post logout redirect URI and iframe for federated signout)
        var logout = await _interaction.GetLogoutContextAsync(logoutId);
        var vm = new LoggedOutViewModel
        {
            AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
            PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
            ClientName = string.IsNullOrEmpty(logout?.ClientName)
                ? logout?.ClientId
                : logout?.ClientName,
            SignOutIframeUrl = logout?.SignOutIFrameUrl,
            LogoutId = logoutId,
        };
        if (User?.Identity.IsAuthenticated == true)
        {
            var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
            if (idp != null && idp != IdentityServerConstants.LocalIdentityProvider)
            {
                var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                if (providerSupportsSignout)
                {
                    if (vm.LogoutId == null)
                    {
                        // if there's no current logout context, we need to create one
                        // this captures necessary info from the current logged in user
                        // before we signout and redirect away to the external IdP for signout
                        vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                    }
                    vm.ExternalAuthenticationScheme = idp;
                }
            }
        }
        return vm;
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    [HttpPost]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        var cultureProvider = _requestLocalizationOptions
            .RequestCultureProviders.OfType<OrigamCookieRequestCultureProvider>()
            .First();
        Response.Cookies.Append(
            cultureProvider.CookieName,
            cultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );
        return LocalRedirect(returnUrl);
    }
}
