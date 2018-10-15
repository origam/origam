using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Origam.Server.Handlers
{
    public class SignOutHandler : IHttpHandler
    {
        public void ProcessRequest (HttpContext context) {
            context.Response.Redirect("~/SignOut.cshtml");
        }
     
        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}
