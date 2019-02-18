using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Origam.ServerCommon.Pages;

namespace Origam.ServerCore
{
    public class UserApiMiddleWare
    {
        public UserApiMiddleWare(RequestDelegate next)
        {
        }

        public async Task Invoke(HttpContext context)
        {
            UserApiProcessor userApiProcessor = new UserApiProcessor();
            var contextWrapper = new StandardHttpContextWrapper(context);
            userApiProcessor.Process(contextWrapper);
        }
    }
}