using System;
using Microsoft.Extensions.Configuration;

namespace Origam.Extensions
{
    public static class IConfigurationExtensions
    {
        public static IConfigurationSection GetSectionOrThrow(this IConfiguration configuration, string key)
        {
            var section = configuration.GetSection(key);
            if (!section.Exists())
            {
                throw new ArgumentException($"Section \"{key}\" was not found in configuration. Check your appsettings.json");
            }
            return section;
        }
    }
}