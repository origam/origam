using System;
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
