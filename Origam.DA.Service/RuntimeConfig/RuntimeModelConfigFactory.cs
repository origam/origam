using System.IO;

namespace Origam.DA.Service
{
    public static class RuntimeModelConfigFactory
    {
        public static IRuntimeModelConfig Create(string pathToConfigFile)
        {
            return File.Exists(pathToConfigFile)
                ? new RuntimeModelConfig(pathToConfigFile)
                : new NullRuntimeModelConfig() as IRuntimeModelConfig;
        }
    }
}