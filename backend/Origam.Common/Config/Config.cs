#if NETSTANDARD
using System;

namespace Origam.Config;

public class Config : IConfig
{
    public long? GetValue(string[] appSettingsPath)
    {
        if (appSettingsPath == null)
        {
            return null;
        }

        string keyColon = string.Join(":", appSettingsPath);
        string value = Environment.GetEnvironmentVariable(keyColon);
        if (string.IsNullOrWhiteSpace(value))
        {
            string keySingle = string.Join("_", appSettingsPath);
            value = Environment.GetEnvironmentVariable(keySingle);
        }
        if (string.IsNullOrWhiteSpace(value))
        {
            string keyDouble = string.Join("__", appSettingsPath);
            value = Environment.GetEnvironmentVariable(keyDouble);
        }
        if (!string.IsNullOrWhiteSpace(value) && long.TryParse(value, out long parsed) && parsed > 0)
        {
            return parsed;
        }
        return null;
    }
}
#endif
