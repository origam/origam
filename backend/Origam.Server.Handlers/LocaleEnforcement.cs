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

using log4net;
using Microsoft.Owin;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Origam.Server.Handlers
{
    public class LocaleEnforcement : OwinMiddleware
    {
        private static readonly ILog log 
            = LogManager.GetLogger(typeof(LocaleEnforcement));

        public LocaleEnforcement(OwinMiddleware next) : base(next)
        {
        }

        override public async Task Invoke(IOwinContext context)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("Processing: " + context.Request.Uri);
            }
            string cookieValue = context.Request.Cookies["origamLanguage"];
            if (!String.IsNullOrEmpty(cookieValue))
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug("Setting locale to " + cookieValue);
                }
                Thread.CurrentThread.CurrentUICulture 
                    = CultureInfo.GetCultureInfo(cookieValue);
                Thread.CurrentThread.CurrentCulture 
                    = CultureInfo.CreateSpecificCulture(cookieValue);
            }
            await Next.Invoke(context);
        }
    }
}
