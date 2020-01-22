#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Origam;
using Microsoft.Owin;

namespace Origam.Server.Handlers
{
    public class IsInRole : OwinMiddleware
    {
        public IsInRole(OwinMiddleware next) : base(next)
        {
        }

        override public async Task Invoke(IOwinContext context)
        {
            bool authorized = false;
            context.Response.ContentType = "application/json";
            string role = context.Request.Query.Get("name");
            if (role != null)
            {
                authorized = SecurityManager.GetAuthorizationProvider()
                    .Authorize(SecurityManager.CurrentPrincipal, role);
            }
            if (authorized)
            {
                context.Response.Write("{\"Result\":true}");
            }
            else
            {
                context.Response.Write("{\"Result\":false}");
            }
            await Task.FromResult(0);
        }
    }
}
