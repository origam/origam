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
using System.Web;
using System.Threading;

namespace Origam.hosting.utils
{
    public class ORIGAMLocaleResolver : IHttpModule
    {
        public static readonly String ORIGAM_CURRENT_LOCALE = "origamCurrentLocale";

        #region IHttpModule Members

        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += LocaleResolver;
        }

        #endregion

        private void LocaleResolver(object source, EventArgs e)
        {
            HttpApplication application = (HttpApplication)source;
            HttpContext context = application.Context;
            if ((context.Request.Cookies != null) 
            && (context.Request.Cookies[ORIGAM_CURRENT_LOCALE] != null))
            {
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(
                    context.Request.Cookies[ORIGAM_CURRENT_LOCALE].Value);
            }
        }
    }
}
