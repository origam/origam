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
        public static bool GetBool(this IConfigurationSection section, string optionName)
        {
            bool success = Boolean.TryParse(section[optionName], out bool boolValue);
            if (!success)
            {
                throw new ArgumentException($"Cannot parse {optionName} to bool");
            }
            return boolValue;
        }
        public static int GetInt(this IConfigurationSection section, string optionName)
        {
            bool success = int.TryParse(section[optionName], out int intValue);
            if (!success)
            {
                throw new ArgumentException($"Cannot parse {optionName} to int");
            }
            return intValue;
        }
    }
}