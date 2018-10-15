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
