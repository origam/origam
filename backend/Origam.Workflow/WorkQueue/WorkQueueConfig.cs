using Origam.Config;

namespace Origam.Workflow.WorkQueue;

static class WorkQueueConfig
{
    internal static long GetMaxUncompressedBytes()
    {
        const int defaultMb = 50; // default for Architect and fallback
        IConfig config = ConfigFactory.GetConfig();
        long maxZipSizeMb = config.GetValue(new [] { "WorkQueue", "MaxUncompressedMbInZip" }) ?? defaultMb;
        return maxZipSizeMb * 1024L * 1024L;
    }

    internal static double GetMaxCompressionRatio()
    {
        const double defaultRatio = 100.0;
        IConfig config = ConfigFactory.GetConfig();
        double? configured = config.GetValue(new [] { "WorkQueue", "MaxCompressionRatio" });
        return configured ?? defaultRatio;
    }
}