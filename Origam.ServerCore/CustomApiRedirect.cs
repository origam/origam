using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Origam.ServerCore
{
    public class CustomApiRedirect
    {
        private readonly RequestDelegate next;

        public CustomApiRedirect(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            string path = context.Request.Path.Value;
            if (true)
            {
                await next(context);
            }
            else
            {
//                var response = context.Response;
//                response.ContentType = "application/json";
//                response.StatusCode = 404;
//                log.LogError("A 404 code was returned in response to: \"" + path + "\" because the path did not pass allowed route check.");
//                await response.WriteAsync(JsonConvert.SerializeObject("Cannot find the requested route: " + path));
            }
        }

     
    }
}