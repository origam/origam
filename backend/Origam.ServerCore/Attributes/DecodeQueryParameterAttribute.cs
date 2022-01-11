using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Origam.ServerCore.Attributes
{
    public class DecodeQueryParameterAttribute : ActionFilterAttribute
    {
        private readonly string parameterName;

        public DecodeQueryParameterAttribute(string parameterName)
        {
            this.parameterName = parameterName;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ActionArguments.ContainsKey(parameterName))
            {
                return;
            }

            string param = context.ActionArguments[parameterName] as string;
            context.ActionArguments[parameterName] = WebUtility.UrlDecode(param);
            base.OnActionExecuting(context);
        }
    }
}

