using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Origam.ServerCore.Middleware
{
    // Some classes were in different namespaces and/or assemblies in the old .NET 
    // this creates issues when these classes are deserialized from xml. 
    // The SoapNamespaceReplacerMiddleware fixes this issue. 
    public class SoapNamespaceReplacerMiddleware
    {
        private readonly RequestDelegate next;

        public SoapNamespaceReplacerMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;

            var stream = request.Body;          
            var content = await new StreamReader(stream).ReadToEndAsync();

            content = content.Replace(
                "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e");

            var requestData = Encoding.UTF8.GetBytes(content);
            request.Body = new MemoryStream(requestData);

            await next(context);
        }
    }
}