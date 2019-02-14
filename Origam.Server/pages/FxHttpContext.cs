using System.Web;
using Origam.ServerCommon.Pages;

namespace Origam.Server.Pages
{
    internal class FxHttpContext : IHttpContext
    {
        private readonly HttpContext context;

        public FxHttpContext(HttpContext context)
        {
            this.context = context;
            Response = new FxHttpResponse(context.Response);
            Request=new FxHttpRequest(context.Request);
        }

        public IResponse Response { get; }
        public IRequest Request { get; }
    }
}