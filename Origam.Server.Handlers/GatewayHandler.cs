using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Origam.Server.Handlers
{
    public class GatewayHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
