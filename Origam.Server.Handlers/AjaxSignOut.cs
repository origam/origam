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
