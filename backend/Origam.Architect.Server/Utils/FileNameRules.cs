#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

namespace Origam.Architect.Server.Utils;

public static class FileNameRules
{
    private static readonly HashSet<string> ReservedDeviceNames = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "CON",
        "PRN",
        "AUX",
        "NUL",
        "COM1",
        "COM2",
        "COM3",
        "COM4",
        "COM5",
        "COM6",
        "COM7",
        "COM8",
        "COM9",
        "LPT1",
        "LPT2",
        "LPT3",
        "LPT4",
        "LPT5",
        "LPT6",
        "LPT7",
        "LPT8",
        "LPT9",
    };

    public static bool HasInvalidChars(string name)
    {
        return name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || name.Contains('/')
            || name.Contains('\\');
    }

    public static bool IsReservedOrUnsafe(string name)
    {
        if (name == "." || name == "..")
        {
            return true;
        }
        if (name.EndsWith("."))
        {
            return true;
        }
        string baseName = name.Split('.')[0];
        return ReservedDeviceNames.Contains(baseName);
    }
}
