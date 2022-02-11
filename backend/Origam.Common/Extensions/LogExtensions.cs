using System;
using log4net;

namespace Origam.Extensions
{
    public static class LogExtensions
    {
        public static void HandledDebug(this ILog log, Action loggingAction)
        {
            if (!log.IsDebugEnabled)
            {
                return;
            }

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