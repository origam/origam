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

public class MsSqlProfile : IDatabaseProfile
{
    private readonly int maxIdentifierLength = 128;

    public string CheckIdentifierLength(int length)
    {
        return length > maxIdentifierLength
            ? string.Format(Strings.IdentifierMaxLength, "Postgre SQL", maxIdentifierLength)
            : null;
    }

    public string CheckIndexNameLength(string indexName)
    {
        return indexName.Length > maxIdentifierLength
            ? string.Format(Strings.IndexMaxLength, indexName, "Postgre SQL", maxIdentifierLength)
            : null;
    }
}
