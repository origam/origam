using System;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Mvc;

namespace Origam.Server.IdentityServerGui;

public static class Extensions
{
    /// <summary>
    /// Checks if the redirect URI is for a native client.
    /// </summary>
    /// <returns></returns>
    public static bool IsNativeClient(this AuthorizationRequest context)
    {
        return !context.RedirectUri.StartsWith("https", StringComparison.Ordinal)
            && !context.RedirectUri.StartsWith("http", StringComparison.Ordinal);
    }

    public static IActionResult LoadingPage(
        this Microsoft.AspNetCore.Mvc.Controller controller,
        string viewName,
        string redirectUri
    )
    {
        return new RedirectResult(url: redirectUri);
    }
}
