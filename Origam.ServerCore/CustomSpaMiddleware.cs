using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace Origam.ServerCore
{
    public static class SpaApplicationBuilderExtensions
    {
        public static void UseCustomSpa(this IApplicationBuilder app, string pathToClientApp)
        {
            CustomSpaMiddleware.Attach(app, pathToClientApp);
        }
    }
    
    public static class CustomSpaMiddleware
    {
        public static void Attach(IApplicationBuilder app, string pathToClientApp)
        {
            app.Use((context, next) =>
            {
                if (context.GetEndpoint() != null)
                {
                    return next();
                }

                context.Request.Path = "/index.html";
                return next();
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(pathToClientApp)
            });
        }
    }
}