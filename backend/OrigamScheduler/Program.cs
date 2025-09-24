#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

namespace OrigamScheduler;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddWindowsService(options =>
        {
            options.ServiceName = "OrigamScheduler";
        });
        builder.Logging.ClearProviders();
        // Make Microsoft logging allow everything and discard logging
        // settings from appsettings.json.
        // This is done to prevent appsettings.json from
        // overriding log levels set in log4net.json.
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Services.Configure<LoggerFilterOptions>(o =>
        {
            o.MinLevel = LogLevel.Trace; 
            o.Rules.Clear();             
        });
        builder.Logging.AddLog4Net("log4net.config");
        builder.Services.AddHostedService<SchedulerWorker>();
        var host = builder.Build();
        host.Run();
    }
}
