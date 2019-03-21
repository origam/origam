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
﻿using System;
using System.Threading.Tasks;
using log4net;
using Microsoft.Owin;

namespace Origam.Server.Handlers
{
    public class FluorineAuthenticationFilter : OwinMiddleware
    {
        private static readonly ILog log 
            = LogManager.GetLogger(typeof(FluorineAuthenticationFilter));

        public FluorineAuthenticationFilter(OwinMiddleware next) : base(next)
        {
        }

        override public async Task Invoke(IOwinContext context)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("Processing: " + context.Request.Uri);
            }
            if (context.Request.Uri.Segments[
                context.Request.Uri.Segments.Length - 1] 
            != "Gateway.aspx") {
                if (log.IsDebugEnabled)
                {
                    log.Debug("Passing through...");
                }
            } else if ((context.Authentication.User == null) 
            || (context.Authentication.User.Identity == null) 
            || !context.Authentication.User.Identity.IsAuthenticated) {
                if (log.IsWarnEnabled)
                {
                    log.Warn("User not signed in...");
                }
                if (context.Request.ContentType == "application/x-amf")
                {
                    context.Response.StatusCode = 477;
                }
                else
                {
                    context.Response.StatusCode = 401;
                }
                return;
            }
            await Next.Invoke(context);
        }
    }
}
