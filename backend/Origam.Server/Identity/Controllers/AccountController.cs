using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Origam.Security.Common;
using Origam.Server.Authorization;
using Origam.Server.Identity.Models;

namespace Origam.Server.Identity.Controllers;

[AllowAnonymous]
public class AccountController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly SignInManager<IOrigamUser> _signInManager;
    private readonly CoreUserManager<IOrigamUser> _userManager;

    public AccountController(
        SignInManager<IOrigamUser> signInManager,
        CoreUserManager<IOrigamUser> userManager
    )
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.UserName,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true
        );
        if (result.Succeeded)
        {
            return RedirectToLocal(returnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account locked.");
            return View();
        }

        ModelState.AddModelError(string.Empty, "Invalid login.");
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        return !string.IsNullOrEmpty(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(Login));
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var props = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(props, provider);
    }

    [HttpGet, AllowAnonymous]
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

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var signIn = await _signInManager.ExternalLoginSignInAsync(
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

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            // implement to fit your schema; or inline a constructor
            user = new User { UserName = userName, Email = email };
            var createRes = await _userManager.CreateAsync(user);
            if (!createRes.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Cannot create local user.");
                return RedirectToAction(nameof(Login), new { returnUrl });
            }
        }

        await _userManager.AddLoginAsync(user, info);
        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToLocal(returnUrl);
    }

    [HttpGet, AllowAnonymous]
    public IActionResult AccessDenied() => View();

    private IActionResult RedirectToLocal(string returnUrl) =>
        Url.IsLocalUrl(returnUrl)
            ? (IActionResult)LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Home");
}
