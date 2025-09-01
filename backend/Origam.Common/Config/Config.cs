#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
