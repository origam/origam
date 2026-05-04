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

namespace Origam.Schema;

/// <summary>
/// Standard collection of primary key columns for any version related schema item.
/// </summary>
public class ModelElementKey : Key
{
    private const string PK1 = "Id";

    public ModelElementKey()
    {
        this.Add(key: PK1, value: System.Guid.NewGuid());
    }

    public ModelElementKey(Guid id)
        : this()
    {
        this.Id = id;
    }

    public Guid Id
    {
        get { return (Guid)this[key: PK1]; }
        set { this[key: PK1] = value; }
    }
}
