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

using Origam.DA.Common.Interfaces;

namespace Origam.DA.Common.DatabasePlatform;

public class PostgresProfile : IDatabaseProfile
{
    private readonly int maxIdentifierLength = 63;

    public string CheckIdentifierLength(int length)
    {
        return length > maxIdentifierLength
            ? string.Format(
                format: Strings.IdentifierMaxLength,
                arg0: "Postgre SQL",
                arg1: maxIdentifierLength
            )
            : null;
    }

    public string CheckIndexNameLength(string indexName)
    {
        if (indexName.Length > maxIdentifierLength)
        {
            return string.Format(
                    format: Strings.IndexMaxLength,
                    arg0: $"\n{indexName}\n",
                    arg1: "Postgre SQL",
                    arg2: maxIdentifierLength
                )
                + "\n"
                + string.Format(format: Strings.PostgresIndexNameLength);
        }

        return null;
    }
}
