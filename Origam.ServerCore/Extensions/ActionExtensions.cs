using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Origam.ServerCore.Extensions
{
    public static class ActionExtensions
    {
        public static string GetMessage(this IActionResult actionResult)
        {
            string message = (actionResult as ObjectResult)?.Value as string;
            return message ?? "";
        }
    }
}
