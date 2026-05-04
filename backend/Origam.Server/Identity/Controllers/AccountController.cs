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

using System;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
using Origam.Server.Identity.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Origam.Server.Identity.Controllers;

[AllowAnonymous]
public class AccountController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly UserManager<IOrigamUser> userManager;
    private readonly SignInManager<IOrigamUser> signInManager;
    private readonly IAuthenticationSchemeProvider schemeProvider;
    private readonly IMailService mailService;
    private readonly UserConfig userConfig;
    private readonly IStringLocalizer<SharedResources> localizer;
    private readonly ILogger<UserManager<IOrigamUser>> logger;
    private readonly IdentityGuiConfig configOptions;
    private readonly RequestLocalizationOptions requestLocalizationOptions;

    public AccountController(
        UserManager<IOrigamUser> userManager,
        SignInManager<IOrigamUser> signInManager,
        IAuthenticationSchemeProvider schemeProvider,
        IMailService mailService,
        IOptions<UserConfig> userConfig,
        IStringLocalizer<SharedResources> localizer,
        IOptions<RequestLocalizationOptions> requestLocalizationOptions,
        IOptions<IdentityGuiConfig> configOptions,
        ILogger<UserManager<IOrigamUser>> logger
    )
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.schemeProvider = schemeProvider;
        this.mailService = mailService;
        this.localizer = localizer;
        this.logger = logger;
        this.configOptions = configOptions.Value;
        this.userConfig = userConfig.Value;
        this.requestLocalizationOptions = requestLocalizationOptions.Value;
    }

    private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
    {
        var schemes = await schemeProvider.GetAllSchemesAsync();
        var externalProviders = schemes
            .Where(predicate: x => x.DisplayName != null)
            .Select(selector: x => new ExternalProvider
            {
                DisplayName = x.DisplayName,
                AuthenticationScheme = x.Name,
            })
            .ToList();
        return new LoginViewModel
        {
            EnableLocalLogin = true,
            ReturnUrl = returnUrl,
            VisibleExternalProviders = externalProviders,
        };
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string returnUrl = null)
    {
        ViewData[index: "ReturnUrl"] = returnUrl;
        LoginViewModel model = await BuildLoginViewModelAsync(returnUrl: returnUrl);
        return View(model: model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        ViewData[index: "ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model: model);
        }

        SignInResult result = await signInManager.PasswordSignInAsync(
            userName: model.UserName,
            password: model.Password,
            isPersistent: model.RememberMe,
            lockoutOnFailure: true
        );

        if (result.Succeeded)
        {
            return RedirectToLocal(returnUrl: returnUrl);
        }

        if (result.RequiresTwoFactor)
        {
            // Get user so we can send the code
            var user = await userManager.FindByNameAsync(userName: model.UserName);
            if (user == null)
            {
                ModelState.AddModelError(
                    key: string.Empty,
                    errorMessage: localizer[name: "InvalidLogin"]
                );
                return View(model: model);
            }

            // Email provider (default ASP.NET Identity email 2FA)
            string code = await userManager.GenerateTwoFactorTokenAsync(
                user: user,
                tokenProvider: TokenOptions.DefaultEmailProvider
            );

            mailService.SendMultiFactorAuthCode(user: user, token: code);

            // Redirect to second-step screen
            return RedirectToAction(
                actionName: nameof(LoginTwoStep),
                routeValues: new { returnUrl, rememberMe = model.RememberMe }
            );
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                key: string.Empty,
                errorMessage: localizer[name: "UserLockedOut"]
            );
            LoginViewModel newModel = await BuildLoginViewModelAsync(returnUrl: returnUrl);
            newModel.UserName = model.UserName;
            return View(model: newModel);
        }

        // Invalid credentials
        LoginViewModel invalidModel = await BuildLoginViewModelAsync(returnUrl: returnUrl);
        invalidModel.UserName = model.UserName;
        ModelState.AddModelError(key: string.Empty, errorMessage: localizer[name: "InvalidLogin"]);
        return View(model: invalidModel);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> LoginTwoStep(string returnUrl = null, bool rememberMe = false)
    {
        // User is stored in the 2FA cookie when result.RequiresTwoFactor == true
        IOrigamUser user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            // 2FA cookie missing/expired
            return RedirectToAction(actionName: nameof(Login), routeValues: new { returnUrl });
        }

        var model = new LoginTwoStepViewModel { ReturnUrl = returnUrl, RememberMe = rememberMe };

        return View(model: model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginTwoStep(LoginTwoStepViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model: model);
        }

        IOrigamUser user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return RedirectToAction(
                actionName: nameof(Login),
                routeValues: new { returnUrl = model.ReturnUrl }
            );
        }

        // Normalize the code (remove spaces/dashes)
        string code = model
            .TwoFactorCode?.Replace(oldValue: " ", newValue: string.Empty)
            ?.Replace(oldValue: "-", newValue: string.Empty);

        SignInResult result = await signInManager.TwoFactorSignInAsync(
            provider: TokenOptions.DefaultEmailProvider,
            code: code,
            isPersistent: model.RememberMe,
            rememberClient: false
        );

        if (result.Succeeded)
        {
            return RedirectToLocal(returnUrl: model.ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                key: string.Empty,
                errorMessage: localizer[name: "UserLockedOut"]
            );
            return View(model: model);
        }

        ModelState.AddModelError(
            key: string.Empty,
            errorMessage: localizer[name: "LoginFailedWrongCode"]
        );
        return View(model: model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword(string returnUrl = null)
    {
        if (!configOptions.AllowPasswordReset)
        {
            return RedirectToAction(actionName: nameof(Login), controllerName: "Account");
        }
        var model = new ForgotPasswordViewModel
        {
            ReturnUrl = ExtractRedirectUriFromReturnUrl(returnUrl: returnUrl),
        };
        return View(model: model);
    }

    private string ExtractRedirectUriFromReturnUrl(string returnUrl)
    {
        if (string.IsNullOrEmpty(value: returnUrl))
        {
            return "/";
        }
        string decodedUrl = Uri.UnescapeDataString(stringToUnescape: returnUrl);
        string pattern = @"redirect_uri=([^&#]+)";
        Match match = Regex.Match(input: decodedUrl, pattern: pattern);
        return match.Success ? match.Groups[groupnum: 1].Value : "/";
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!configOptions.AllowPasswordReset)
        {
            return RedirectToAction(actionName: nameof(Login), controllerName: "Account");
        }
        if (ModelState.IsValid)
        {
            var user = await userManager.FindByEmailAsync(email: model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist or is not confirmed
                logger.LogWarning(
                    message: "ForgotPassword - " + model.Email + " User does not exist."
                );
                return View(viewName: "ForgotPasswordConfirmation");
            }
            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
            // Send an email with this link
            var passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(user: user);
            var callbackUrl = Url.Action(
                action: "ResetPassword",
                controller: "Account",
                values: new { userId = user.BusinessPartnerId, code = passwordResetToken },
                protocol: HttpContext.Request.Scheme
            );
            mailService.SendPasswordResetToken(
                user: user,
                token: passwordResetToken,
                returnUrl: model.ReturnUrl,
                tokenValidityHours: 24
            );
            logger.LogInformation(message: "ForgotPassword - " + model.Email + " Mail was sent.");
            return View(viewName: "ForgotPasswordConfirmation");
        }
        // If we got this far, something failed, redisplay form
        return View(model: model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(
        string code = null,
        string mail = null,
        string returnUrl = null
    )
    {
        if (!configOptions.AllowPasswordReset)
        {
            return RedirectToAction(actionName: nameof(Login), controllerName: "Account");
        }
        if (code == null)
        {
            logger.LogWarning(message: $"Code supplied to {nameof(ResetPassword)} was null");
            return View(viewName: "Error");
        }
        if (mail == null)
        {
            logger.LogWarning(message: $"mail supplied to {nameof(ResetPassword)} was null");
            return View(viewName: "Error");
        }
        var model = new ResetPasswordViewModel
        {
            Email = mail,
            ReturnUrl = string.IsNullOrEmpty(value: returnUrl)
                ? null
                : Uri.UnescapeDataString(stringToUnescape: returnUrl),
        };
        return View(model: model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!configOptions.AllowPasswordReset)
        {
            return RedirectToAction(actionName: nameof(Login), controllerName: "Account");
        }
        if (!ModelState.IsValid)
        {
            return View(model: model);
        }
        var user = await userManager.FindByEmailAsync(email: model.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return RedirectToAction(
                actionName: nameof(ResetPasswordConfirmation),
                controllerName: "Account",
                routeValues: new
                {
                    returnUrl = Uri.EscapeDataString(
                        stringToEscape: model.ReturnUrl ?? "/account/login"
                    ),
                }
            );
        }
        user.EmailConfirmed = true;
        var result = await userManager.ResetPasswordAsync(
            user: user,
            token: model.Code,
            newPassword: model.Password
        );
        if (result.Succeeded)
        {
            return RedirectToAction(
                actionName: nameof(ResetPasswordConfirmation),
                controllerName: "Account",
                routeValues: new
                {
                    returnUrl = Uri.EscapeDataString(
                        stringToEscape: model.ReturnUrl ?? "/account/login"
                    ),
                }
            );
        }
        AddErrors(result: result);
        return View(model: model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation(string returnUrl = null)
    {
        var model = new ResetPasswordConfirmationViewModel
        {
            ReturnUrl = string.IsNullOrEmpty(value: returnUrl)
                ? null
                : Uri.UnescapeDataString(stringToUnescape: returnUrl),
        };
        return View(model: model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterConfirmation()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string returnUrl = null)
    {
        if (!userConfig.UserRegistrationAllowed)
        {
            return View(
                viewName: "Error",
                model: new ErrorViewModel(error: localizer[name: "RegistrationNotAllowed"])
            );
        }
        ViewData[index: "ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!userConfig.UserRegistrationAllowed)
        {
            return View(
                viewName: "Error",
                model: new ErrorViewModel(error: localizer[name: "RegistrationNotAllowed"])
            );
        }
        if (ModelState.IsValid)
        {
            IOrigamUser user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                Name = model.Name,
                RoleId = userConfig.NewUserRoleId,
            };
            IdentityResult result = UserTools.RunCreateUserWorkFlow(
                password: model.Password,
                user: user
            );
            user = await userManager.FindByNameAsync(userName: user.UserName);
            if (result.Succeeded)
            {
                var code = await userManager.GenerateEmailConfirmationTokenAsync(user: user);
                mailService.SendNewUserToken(user: user, token: code);
                return RedirectToAction(
                    actionName: nameof(RegisterConfirmation),
                    controllerName: "Account"
                );
            }
            AddErrors(result: result);
        }
        return View(model: model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterInitialUser()
    {
        if (!UserTools.IsInitialSetupNeeded())
        {
            return View(
                viewName: "Error",
                model: new ErrorViewModel(error: localizer[name: "AlreadySetUp"])
            );
        }
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterInitialUser(RegisterViewModel model)
    {
        if (!UserTools.IsInitialSetupNeeded())
        {
            return View(
                viewName: "Error",
                model: new ErrorViewModel(error: localizer[name: "AlreadySetUp"])
            );
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
            IdentityResult result = UserTools.RunCreateUserWorkFlow(
                password: model.Password,
                user: user
            );
            user = await userManager.FindByNameAsync(userName: user.UserName);
            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user: user, isPersistent: false);
                string emailConfirmToken = await userManager.GenerateEmailConfirmationTokenAsync(
                    user: user
                );
                await userManager.ConfirmEmailAsync(user: user, token: emailConfirmToken);
                UserTools.SetInitialSetupComplete();
                return Redirect(url: "/");
            }
            AddErrors(result: result);
        }
        return View(model: model);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (!userConfig.UserRegistrationAllowed)
        {
            return View(
                viewName: "Error",
                model: new ErrorViewModel(error: localizer[name: "RegistrationNotAllowed"])
            );
        }
        if (userId == null || code == null)
        {
            logger.LogWarning(
                message: $"Invalid confirm email data: userId:\"{userId}\", code:\"{code}\""
            );
            return View(viewName: "Error");
        }
        var user = await userManager.FindByIdAsync(userId: userId);
        if (user == null)
        {
            logger.LogWarning(message: $"User not found: userId:\"{userId}\"");
            return View(viewName: "Error");
        }
        var result = await userManager.ConfirmEmailAsync(user: user, token: code);
        if (result.Succeeded)
        {
            return View(viewName: "EmailConfirmation");
        }
        string errors = string.Join(
            separator: "\n",
            values: result.Errors.Select(selector: error => error.Description)
        );
        logger.LogWarning(message: $"ConfirmEmailAsync failed, errors:\"{errors}\"");

        return View(viewName: "Error");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string returnUrl = null)
    {
        await signInManager.SignOutAsync();
        return !string.IsNullOrEmpty(value: returnUrl)
            ? LocalRedirect(localUrl: returnUrl)
            : RedirectToAction(actionName: nameof(Login));
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string returnUrl = null)
    {
        var redirectUrl = Url.Action(
            action: nameof(ExternalLoginCallback),
            controller: "Account",
            values: new { returnUrl }
        );
        var props = signInManager.ConfigureExternalAuthenticationProperties(
            provider: provider,
            redirectUrl: redirectUrl
        );
        return Challenge(properties: props, authenticationSchemes: provider);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(
        string returnUrl = null,
        string remoteError = null
    )
    {
        if (remoteError != null)
        {
            ModelState.AddModelError(
                key: string.Empty,
                errorMessage: $"External provider error: {remoteError}"
            );
            return RedirectToAction(actionName: nameof(Login), routeValues: new { returnUrl });
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return RedirectToAction(actionName: nameof(Login), routeValues: new { returnUrl });
        }

        var signIn = await signInManager.ExternalLoginSignInAsync(
            loginProvider: info.LoginProvider,
            providerKey: info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true
        );

        if (signIn.Succeeded)
        {
            return RedirectToLocal(returnUrl: returnUrl);
        }

        // Create/link a local user if missing
        var email =
            info.Principal.FindFirstValue(claimType: ClaimTypes.Email)
            ?? info.Principal.FindFirstValue(claimType: "preferred_username");
        var userName = email ?? info.ProviderKey;

        var user = await userManager.FindByNameAsync(userName: userName);
        if (user == null)
        {
            user = new User { UserName = userName, Email = email };
            var createRes = await userManager.CreateAsync(user: user);
            if (!createRes.Succeeded)
            {
                ModelState.AddModelError(
                    key: string.Empty,
                    errorMessage: localizer[name: "CannotCreateUser"]
                );
                return RedirectToAction(actionName: nameof(Login), routeValues: new { returnUrl });
            }
        }

        await userManager.AddLoginAsync(user: user, login: info);
        await signInManager.SignInAsync(user: user, isPersistent: false);
        return RedirectToLocal(returnUrl: returnUrl);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    [HttpPost]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        var cultureProvider = requestLocalizationOptions
            .RequestCultureProviders.OfType<OrigamCookieRequestCultureProvider>()
            .First();
        Response.Cookies.Append(
            key: cultureProvider.CookieName,
            value: cultureProvider.MakeCookieValue(
                requestCulture: new RequestCulture(culture: culture)
            ),
            options: new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(years: 1) }
        );
        return LocalRedirect(localUrl: returnUrl);
    }

    private IActionResult RedirectToLocal(string returnUrl) =>
        Url.IsLocalUrl(url: returnUrl)
            ? LocalRedirect(localUrl: returnUrl)
            : RedirectToAction(actionName: "Index", controllerName: "Home");

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(key: string.Empty, errorMessage: error.Description);
        }
    }
}
