#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
                        // get password attempts
                        int? attemptedCount = userManager.GetFailedPasswordAttemptCount(username);
                        context.Authentication.SignOut();
                        context.Response.Write(
                            "{\"Status\":204,\"Message\":"
                            + JsonConvert.ToString(userManager.ExposeLoginAttemptsInfo && attemptedCount != null ?
                                    String.Format(Origam.Security.Common.Resources.InvalidUsernameOrPasswordWithAttemptInfo,
                                        userManager.MaxFailedAccessAttemptsBeforeLockout - attemptedCount)
                                    : Origam.Security.Common.Resources.InvalidUsernameOrPassword)
                            + ((userManager.ExposeLoginAttemptsInfo && attemptedCount != null) ?
                                (",\"FailedAttemptCount\" : " + attemptedCount.ToString()
                                 + ",\"MaxFailedAttemptCount\" : " + userManager.MaxFailedAccessAttemptsBeforeLockout.ToString())
                                : "")
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
