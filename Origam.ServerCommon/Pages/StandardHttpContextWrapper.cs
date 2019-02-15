using Microsoft.AspNetCore.Http;

namespace Origam.ServerCommon.Pages
{
    public class StandardHttpContextWrapper : IHttpContextWrapper
    {
        private readonly HttpContext context;

        public StandardHttpContextWrapper(HttpContext context)
        {
            this.context = context;
            Response = new StandardHttpResponseWrapper(context);
            Request=new StandardHttpRequestWrapper(context);
        }

        public IResponseWrapper Response { get; }
        public IRequestWrapper Request { get; }
    }
}