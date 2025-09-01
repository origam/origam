namespace Origam.Config;


public interface IConfig
{
    public long? GetValue(string[] appSettingsPath);
}

public static class ConfigFactory {
    public static IConfig GetConfig()
    {
#if NETSTANDARD
        return new Config();
#endif
        return new NullConfig();
    }
}