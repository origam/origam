#region license

/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using System.Linq;

namespace Origam.DA;
public class Ordering
{
    public string ColumnName { get; }
    public string Direction { get; }
    public int SortOrder { get; }
    public Guid LookupId { get;}
    public Ordering(string columnName, string direction, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(direction))
        {
            throw new ArgumentException(nameof(direction) + " cannot be empty");
        }           
        if (string.IsNullOrWhiteSpace(columnName))
        {
            throw new ArgumentException(nameof(columnName) + " cannot be empty");
        }
        ColumnName = columnName;
        Direction = direction;
        SortOrder = sortOrder;
    }
    public Ordering(string columnName, string direction,
        Guid lookupId, int sortOrder)
        : this(columnName, direction, sortOrder)
    {
        ColumnName = columnName;
        Direction = direction;
        LookupId = lookupId;
    }
}
