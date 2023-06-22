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
        
        public static string[] GetStringArrayOrThrow(this IConfiguration section)
        {
            string[] stringArray = section.Get<string[]>();
            if (stringArray == null || stringArray.Length == 0)
            {
                throw new ArgumentException($"String array in section \"{section}\" was not found in configuration or was empty. Check your appsettings.json");
            }
            return stringArray;
        }        
        
        public static string[] GetStringArrayOrEmpty(this IConfiguration section)
        {
            string[] stringArray = section.Get<string[]>();
            if (stringArray == null || stringArray.Length == 0)
            {
                return Array.Empty<string>();
            }
            return stringArray;
        }
    }
}