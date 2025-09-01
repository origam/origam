namespace Origam.Config;

public class NullConfig : IConfig
{
    public long? GetValue(string[] appSettingsPath)
    {
        return null;
    }
}