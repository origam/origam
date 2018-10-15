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
