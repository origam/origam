using System.Security.Principal;

namespace Origam.Architect.Server;

public class CustomThreadPrincipalMiddleware
{
    private readonly RequestDelegate next;

    public CustomThreadPrincipalMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            Thread.CurrentPrincipal = context.User;
        }
        else
        {
            IIdentity identity = new GenericIdentity("origam_server");
            IPrincipal principal = new GenericPrincipal(identity, new[] { "User" });

            Thread.CurrentPrincipal = principal;
        }

        await next(context);
    }
}
