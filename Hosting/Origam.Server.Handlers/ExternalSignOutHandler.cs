using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Origam.Server.Handlers
{
    public class ExternalSignOutHandler : IHttpHandler
    {
        public void ProcessRequest (HttpContext context) {
            Origam.OrigamUserContext.Reset();
        }
     
        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}
