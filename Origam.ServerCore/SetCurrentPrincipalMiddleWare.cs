using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Origam.ServerCore
{
    public class SetCurrentPrincipalMiddleWare
    {
        private readonly RequestDelegate next;

        public SetCurrentPrincipalMiddleWare(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            Thread.CurrentPrincipal = context.User;
            await next(context);
        }
    }
}