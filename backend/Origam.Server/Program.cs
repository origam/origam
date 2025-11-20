using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Origam.Server.Configuration;

namespace Origam.Server
{
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
                    var identityServerConfig = provider.GetRequiredService<IdentityServerConfig>();
                    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

                    OrigamUtils.ConnectOrigamRuntime(
                        loggerFactory,
                        startUpConfiguration.ReloadModelWhenFilesChangesDetected
                    );
                    OrigamUtils.CleanUpDatabase();
                    OpenIddictSeeder
                        .SeedAsync(provider, identityServerConfig)
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
}
