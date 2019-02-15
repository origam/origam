using System.Web;
using Origam.ServerCommon.Pages;

namespace Origam.Server.Pages
{
    internal class FxHttpContextWrapper : IHttpContextWrapper
    {
        private readonly HttpContext context;

        public FxHttpContextWrapper(HttpContext context)
        {
            this.context = context;
            Response = new FxHttpResponseWrapper(context.Response);
            Request=new FxHttpRequestWrapper(context.Request);
        }

        public IResponseWrapper Response { get; }
        public IRequestWrapper Request { get; }
    }
}