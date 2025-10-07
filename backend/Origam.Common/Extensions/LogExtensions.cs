#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using log4net;
using Microsoft.Extensions.Logging;

namespace Origam.Extensions;

public static class LogExtensions
{
    public static readonly string IsLoggedKey = "logged";

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
            log.LogOrigamError(ex);
        }
    }

    public static void LogOrigamError(this ILog log, string message, Exception ex)
    {
        if (ex.Data.Contains(IsLoggedKey) && Equals(ex.Data[IsLoggedKey], true))
        {
            return;
        }
        log.Error(message, ex);
        ex.Data[IsLoggedKey] = true;
    }

    public static void LogOrigamError(this ILog log, Exception ex)
    {
        if (ex.Data.Contains(IsLoggedKey) && Equals(ex.Data[IsLoggedKey], true))
        {
            return;
        }
        log.Error(ex);
        ex.Data[IsLoggedKey] = true;
    }
}

public static class ILoggerExtensions
{
    public static void LogOrigamError(this ILogger log, Exception ex, string message)
    {
        if (
            ex.Data.Contains(LogExtensions.IsLoggedKey)
            && Equals(ex.Data[LogExtensions.IsLoggedKey], true)
        )
        {
            return;
        }
        log.LogError(ex, message);
        ex.Data[LogExtensions.IsLoggedKey] = true;
    }
}
