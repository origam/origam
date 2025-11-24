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
            .Where(x => x.DisplayName != null)
            .Select(x => new ExternalProvider
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
        ViewData["ReturnUrl"] = returnUrl;
        LoginViewModel model = await BuildLoginViewModelAsync(returnUrl);
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        SignInResult result = await signInManager.PasswordSignInAsync(
            model.UserName,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true
        );

        if (result.Succeeded)
        {
            return RedirectToLocal(returnUrl);
        }

        if (result.RequiresTwoFactor)
        {
            // Get user so we can send the code
            var user = await userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login.");
                return View(model);
            }

            // Email provider (default ASP.NET Identity email 2FA)
            string code = await userManager.GenerateTwoFactorTokenAsync(
                user,
                TokenOptions.DefaultEmailProvider
            );

            mailService.SendMultiFactorAuthCode(user, code);

            // Redirect to second-step screen
            return RedirectToAction(
                nameof(LoginTwoStep),
                new { returnUrl, rememberMe = model.RememberMe }
            );
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account locked.");
            LoginViewModel newModel = await BuildLoginViewModelAsync(returnUrl);
            newModel.UserName = model.UserName;
            return View(newModel);
        }

        // Invalid credentials
        LoginViewModel invalidModel = await BuildLoginViewModelAsync(returnUrl);
        invalidModel.UserName = model.UserName;
        ModelState.AddModelError(string.Empty, "Invalid login.");
        return View(invalidModel);
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
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var model = new LoginTwoStepViewModel { ReturnUrl = returnUrl, RememberMe = rememberMe };

        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginTwoStep(LoginTwoStepViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        IOrigamUser user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return RedirectToAction(nameof(Login), new { returnUrl = model.ReturnUrl });
        }

        // Normalize the code (remove spaces/dashes)
        string code = model.TwoFactorCode?.Replace(" ", string.Empty)?.Replace("-", string.Empty);

        SignInResult result = await signInManager.TwoFactorSignInAsync(
            TokenOptions.DefaultEmailProvider,
            code,
            isPersistent: model.RememberMe,
            rememberClient: false
        );

        if (result.Succeeded)
        {
            return RedirectToLocal(model.ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account locked.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Invalid authentication code.");
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword(string returnUrl = null)
    {
        if (!configOptions.AllowPasswordReset)
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

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!configOptions.AllowPasswordReset)
        {
            return RedirectToAction(nameof(Login), "Account");
        }
        if (ModelState.IsValid)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist or is not confirmed
                logger.LogWarning("ForgotPassword - " + model.Email + " User does not exist.");
                return View("ForgotPasswordConfirmation");
            }
            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
            // Send an email with this link
            var passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(
                "ResetPassword",
                "Account",
                new { userId = user.BusinessPartnerId, code = passwordResetToken },
                protocol: HttpContext.Request.Scheme
            );
            mailService.SendPasswordResetToken(
                user,
                passwordResetToken,
                model.ReturnUrl,
                tokenValidityHours: 24
            );
            logger.LogInformation("ForgotPassword - " + model.Email + " Mail was sent.");
            return View("ForgotPasswordConfirmation");
        }
        // If we got this far, something failed, redisplay form
        return View(model);
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
            return RedirectToAction(nameof(Login), "Account");
        }
        if (code == null)
        {
            logger.LogWarning($"Code supplied to {nameof(ResetPassword)} was null");
            return View("Error");
        }
        if (mail == null)
        {
            logger.LogWarning($"mail supplied to {nameof(ResetPassword)} was null");
            return View("Error");
        }
        var model = new ResetPasswordViewModel
        {
            Email = mail,
            ReturnUrl = string.IsNullOrEmpty(returnUrl) ? null : Uri.UnescapeDataString(returnUrl),
        };
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!configOptions.AllowPasswordReset)
        {
            return RedirectToAction(nameof(Login), "Account");
        }
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var user = await userManager.FindByEmailAsync(model.Email);
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
        var result = await userManager.ResetPasswordAsync(user, model.Code, model.Password);
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
        return View(model);
    }

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
            return View("Error", new ErrorViewModel(localizer["RegistrationNotAllowed"]));
        }
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!userConfig.UserRegistrationAllowed)
        {
            return View("Error", new ErrorViewModel(localizer["RegistrationNotAllowed"]));
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
            IdentityResult result = UserTools.RunCreateUserWorkFlow(model.Password, user);
            user = await userManager.FindByNameAsync(user.UserName);
            if (result.Succeeded)
            {
                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                mailService.SendNewUserToken(user, code);
                return RedirectToAction(nameof(RegisterConfirmation), "Account");
            }
            AddErrors(result);
        }
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterInitialUser()
    {
        if (!UserTools.IsInitialSetupNeeded())
        {
            return View("Error", new ErrorViewModel(localizer["AlreadySetUp"]));
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
            return View("Error", new ErrorViewModel(localizer["AlreadySetUp"]));
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
            user = await userManager.FindByNameAsync(user.UserName);
            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, false);
                string emailConfirmToken = await userManager.GenerateEmailConfirmationTokenAsync(
                    user
                );
                await userManager.ConfirmEmailAsync(user, emailConfirmToken);
                UserTools.SetInitialSetupComplete();
                return Redirect("/");
            }
            AddErrors(result);
        }
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (!userConfig.UserRegistrationAllowed)
        {
            return View("Error", new ErrorViewModel(localizer["RegistrationNotAllowed"]));
        }
        if (userId == null || code == null)
        {
            logger.LogWarning($"Invalid confirm email data: userId:\"{userId}\", code:\"{code}\"");
            return View("Error");
        }
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            logger.LogWarning($"User not found: userId:\"{userId}\"");
            return View("Error");
        }
        var result = await userManager.ConfirmEmailAsync(user, code);
        if (result.Succeeded)
        {
            return View("EmailConfirmation");
        }
        string errors = string.Join("\n", result.Errors.Select(error => error.Description));
        logger.LogWarning($"ConfirmEmailAsync failed, errors:\"{errors}\"");

        return View("Error");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string returnUrl = null)
    {
        await signInManager.SignOutAsync();
        return !string.IsNullOrEmpty(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var props = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(props, provider);
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
            ModelState.AddModelError(string.Empty, $"External provider error: {remoteError}");
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var signIn = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true
        );

        if (signIn.Succeeded)
        {
            return RedirectToLocal(returnUrl);
        }

        // Create/link a local user if missing
        var email =
            info.Principal.FindFirstValue(ClaimTypes.Email)
            ?? info.Principal.FindFirstValue("preferred_username");
        var userName = email ?? info.ProviderKey;

        var user = await userManager.FindByNameAsync(userName);
        if (user == null)
        {
            // implement to fit your schema; or inline a constructor
            user = new User { UserName = userName, Email = email };
            var createRes = await userManager.CreateAsync(user);
            if (!createRes.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Cannot create local user.");
                return RedirectToAction(nameof(Login), new { returnUrl });
            }
        }

        await userManager.AddLoginAsync(user, info);
        await signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToLocal(returnUrl);
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
            cultureProvider.CookieName,
            cultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );
        return LocalRedirect(returnUrl);
    }

    private IActionResult RedirectToLocal(string returnUrl) =>
        Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Home");

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
