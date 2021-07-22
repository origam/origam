using System;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Mvc;
using Origam.ServerCore.IdentityServerGui.Account;

namespace Origam.ServerCore.IdentityServerGui
{
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
        
        public static IActionResult LoadingPage(this Microsoft.AspNetCore.Mvc.Controller controller, string viewName, string redirectUri)
        {
            controller.HttpContext.Response.StatusCode = 200;
            controller.HttpContext.Response.Headers["Location"] = "";
            
            return controller.View(viewName, new RedirectViewModel { RedirectUrl = redirectUri });
        }
    }
}
