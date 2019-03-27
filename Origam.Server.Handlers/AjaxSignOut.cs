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

using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Origam.Server.Handlers
{
    public class AjaxSignOut : OwinMiddleware
    {
        public AjaxSignOut(OwinMiddleware next) : base(next)
        {
        }

        override public async Task Invoke(IOwinContext context)
        {
            context.Response.ContentType = "application/javascript";
            try
            {
                context.Authentication.SignOut();
                Origam.OrigamUserContext.Reset();
                context.Response.Write("{\"Status\":200,\"Message\":\"Success\"}");
            }
            catch (Exception e)
            {
                context.Response.Write(String.Format(
                    "{\"Status\":500,\"Message\":\"{0}\"",
                    HttpUtility.JavaScriptStringEncode(e.Message)));
            }
            await Task.FromResult(0);
        }
    }
}
