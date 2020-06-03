#region license

/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using Microsoft.Extensions.Configuration;

namespace Origam.Extensions
{
    public static class IConfigurationSectionExtensions
    {
        public static string GetStringOrThrow(this IConfigurationSection section, string key)
        {
            string stringValue = section[key];
            if (stringValue == null)
            {
                throw new ArgumentException($"Key \"{key}\" was not found in configuration in section \"{section.Path}\". Check your appsettings.json");
            }
            return stringValue;
        }
        
        public static bool GetBoolOrThrow(this IConfigurationSection section, string key)
        {
            string stringValue = section.GetStringOrThrow(key);
            bool success = Boolean.TryParse(stringValue, out bool boolValue);
            if (!success)
            {
                throw new ArgumentException($"Cannot parse \"{key}\" to bool");
            }
            return boolValue;
        }
        public static int GetIntOrThrow(this IConfigurationSection section, string key)
        {
            string stringValue = section.GetStringOrThrow(key);
            bool success = int.TryParse(stringValue, out int intValue);
            if (!success)
            {
                throw new ArgumentException($"Cannot parse \"{key}\" to int");
            }
            return intValue;
        }
        
        public static IConfigurationSection GetSectionOrThrow(this IConfigurationSection section, string key)
        {
            var subSection = section.GetSection(key);
            if (!subSection.Exists())
            {
                throw new ArgumentException($"Sub section \"{key}\" was not found in \"{section.Path}\". Check your appsettings.json");
            }
            return subSection;
        }        
       
    }
}