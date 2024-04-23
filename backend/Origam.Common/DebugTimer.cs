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
using System.Diagnostics;

namespace Origam;

public class DebugTimer : IDisposable
{
    private readonly LogType logType;
    private readonly Stopwatch watch;
    private readonly string message;
    private static readonly log4net.ILog log =
        log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        
    public DebugTimer(LogType logType = LogType.CONSOLE,
        string message = "Elapsed time:")
    {
            this.message = message;
            this.logType = logType;
            watch= new Stopwatch();
            watch.Start();
        }

    public void Dispose()
    {
            watch.Stop();

            switch (logType)
            {
                case LogType.CONSOLE:
                    Console.WriteLine($"{message} {watch.Elapsed} s");
                    break;
                case LogType.DEBUG:
                    log.Debug($"{message} {watch.Elapsed} s");
                    break;
                case LogType.ERROR:
                    log.Error($"{message} {watch.Elapsed} s");
                    break;
                case LogType.INFO:
                    log.Info($"{message} {watch.Elapsed} s");
                    break;
                case LogType.WARNING:
                    log.Warn($"{message} {watch.Elapsed} s");
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
}
    
public enum LogType{CONSOLE, INFO, DEBUG, WARNING, ERROR}