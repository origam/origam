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

namespace Origam.ServerCore.Configuration
{
    public class PasswordConfiguration
    {
        public bool RequireDigit { get; } = true;
        public int RequiredLength { get; } = 10;
        public bool RequireNonAlphanumeric { get;} = true;
        public bool RequireUppercase { get;} = true;
        public bool RequireLowercase { get; } = true;

        public PasswordConfiguration(IConfiguration configuration)
        {
            IConfigurationSection passwordSection = configuration.GetSection("PasswordConfig");

            RequireDigit = ParseToBool(passwordSection, "RequireDigit");
            RequiredLength = ParseToInt(passwordSection, "RequiredLength");
            RequireNonAlphanumeric = ParseToBool(passwordSection, "RequireNonAlphanumeric");
            RequireUppercase = ParseToBool(passwordSection, "RequireUppercase");
            RequireLowercase = ParseToBool(passwordSection, "RequireLowercase");
        }
        
        private bool ParseToBool(IConfigurationSection section, string optionName)
        {
            bool success = Boolean.TryParse(section[optionName], out bool boolValue);
            if (!success)
            {
                throw new ArgumentException($"Cannot parse {optionName} to bool");
            }
            return boolValue;
        }
        private int ParseToInt(IConfigurationSection section, string optionName)
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
