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
                    var sp = scope.ServiceProvider;
                    var cfg = sp.GetRequiredService<IdentityServerConfig>();
                    OpenIddictSeeder.SeedAsync(sp, cfg).GetAwaiter().GetResult();
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
