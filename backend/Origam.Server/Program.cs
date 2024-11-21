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
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Origam.Server;

public class Program
{
    private static Log4NetProvider log4NetProvider;

    public static void Main(string[] args)
    {
        Log4NetProviderOptions options = new Log4NetProviderOptions();
        options.Watch = true;
        log4NetProvider = new Log4NetProvider(options);
        ILogger startupLogger = log4NetProvider.CreateLogger();
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Kestrel to use HTTP only in development/CI environments
            if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
            {
                builder.WebHost.ConfigureKestrel(serverOptions =>
                {
                    serverOptions.ListenAnyIP(8080);
                });
            }

            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false,
                    reloadOnChange: true)
                .AddJsonFile(
                    $"appsettings.{builder.Environment.EnvironmentName}.json",
                    optional: true)
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables();

            builder.Logging.Services.AddSingleton(log4NetProvider);
            builder.Logging.SetMinimumLevel(LogLevel.Trace);
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            builder.Services.AddSingleton(builder.Configuration);

            var startup = new Startup(builder.Configuration);
            startup.ConfigureServices(builder.Services);

            var app = builder.Build();
            startup.Configure(app, app.Environment,
                app.Services.GetRequiredService<ILoggerFactory>());

            app.Run();
        }
        catch (Exception e)
        {
            startupLogger.LogCritical($"{e.Message}\n{e.StackTrace}");
            throw;
        }
    }
}