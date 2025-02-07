namespace OrigamScheduler;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddLog4Net("log4net.config");
        builder.Services.AddHostedService<SchedulerWorker>();

        var host = builder.Build();
        host.Run();
    }
}