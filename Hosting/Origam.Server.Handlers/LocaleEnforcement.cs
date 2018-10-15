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
