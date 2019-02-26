using Microsoft.Extensions.Logging;
using Origam.ServerCore.Controllers;

namespace Origam.ServerCore.Extensions
{
    public static class LogExtonsions
    {
        public static void InfoFormat(this ILogger<AbstractController> log, string message, string arg)
        {
            log.LogInformation(message, new []{arg} );
        }
    }
}