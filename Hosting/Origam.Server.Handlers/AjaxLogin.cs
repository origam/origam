using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Origam.Security.Identity;
using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Origam.Server.Utils;
using Origam.Server.Handlers;
using Newtonsoft.Json;
using System.IO;

namespace Origam.Server.Handlers
{
	public class AjaxLogin : OwinMiddleware
	{
		public AjaxLogin(OwinMiddleware next) : base(next)
		{
		}

		override public async Task Invoke(IOwinContext context)
		{
			context.Response.ContentType = "application/json; charset=utf-8";
			Dictionary<string, string> parameters
				= OwinContextHelper.GetRequestParameters(context.Request);
			string password = parameters.ContainsKey("password")
				? parameters["password"] : "";
			string username = parameters.ContainsKey("username")
				? parameters["username"] : "";
			try
			{
				AbstractUserManager userManager
					= AbstractUserManager.GetUserManager();
				Task<OrigamUser> task = userManager.FindAsync(
					username, password.TrimEnd());

				if (task.IsFaulted)
				{
					//log.Error("Failed to identify user.",
					//    task.Exception.InnerException);
					context.Authentication.SignOut();
					context.Response.Write(
						"{\"Status\":204,\"Message\":"
						+ JsonConvert.ToString(task.Exception.InnerException.Message)
						+ "}");
				}
				else
				{
					OrigamUser user = task.Result;
					if (user == null)
					{
						context.Authentication.SignOut();
						context.Response.Write(
							"{\"Status\":204,\"Message\":"
							+ JsonConvert.ToString(Origam.Security.Identity.Resources.InvalidUsernameOrPassword)
							+ "}");
					}
					else
					{
						var claims = new List<Claim>();
						claims.Add(new Claim(ClaimTypes.Name, user.UserName));
						var id = new ClaimsIdentity(
							claims, DefaultAuthenticationTypes.ApplicationCookie);
						context.Authentication.SignIn(id);
						context.Response.Write(
							"{\"Status\":200,\"Message\":\"Success\"}");
					}
				}
			}
			catch (Exception exc)
			{
				context.Response.Write(String.Format(
					"{{\"Status\":500,\"Message\":\"{0}\"}}",
					exc.Message));
			}
			await Task.FromResult(0);
		}
	}
}
