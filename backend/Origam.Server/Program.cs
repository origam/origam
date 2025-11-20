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

using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Origam.Server.Configuration;

namespace Origam.Server;

public class Program
{
    private static Log4NetProvider log4NetProvider;

    public static void Main(string[] args)
    {
        var options = new Log4NetProviderOptions { Watch = true };
        log4NetProvider = new Log4NetProvider(options);
        ILogger startupLogger = log4NetProvider.CreateLogger();

        try
        {
            var host = CreateWebHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var startUpConfiguration = provider.GetRequiredService<StartUpConfiguration>();
                var identityServerConfig = provider.GetRequiredService<OpenIddictConfig>();
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

                OrigamUtils.ConnectOrigamRuntime(
                    loggerFactory,
                    startUpConfiguration.ReloadModelWhenFilesChangesDetected
                );
                OrigamUtils.CleanUpDatabase();
                OpenIddictConfigManager
                    .CreateOrUpdateAsync(provider, identityServerConfig)
                    .GetAwaiter()
                    .GetResult();
            }

            host.Run();
        }
        catch (Exception e)
        {
            startupLogger.LogCritical($"{e.Message}\n{e.StackTrace}");
            throw;
        }
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .ConfigureLogging(
                (hostingContext, logging) =>
                {
                    logging.Services.AddSingleton(log4NetProvider);
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddConsole();
                    logging.AddDebug();
                }
            );
}
