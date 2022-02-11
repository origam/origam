using System;
using log4net;

namespace Origam.Extensions
{
    public static class LogExtensions
    {
        // Intended for error handling of logging code.
        // Remember to wrap all calls to this method in if(log.IsXXXEnabled){} 
        // to minimize performance impact of logging. 
        public static void RunHandled(this ILog log, Action loggingAction)
        {
            try
            {
                loggingAction();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}