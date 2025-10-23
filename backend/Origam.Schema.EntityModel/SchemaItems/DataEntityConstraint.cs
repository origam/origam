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

using System.Collections.Generic;

namespace Origam.Schema.EntityModel;

public enum ConstraintType
{
    PrimaryKey,
    ForeignKey,
    Unique,
}

/// <summary>
/// Summary description for DataEntityConstraint.
/// </summary>
public class DataEntityConstraint
{
    public DataEntityConstraint(ConstraintType type)
    {
        this.Type = type;
    }

    private ConstraintType _type;
    public ConstraintType Type
    {
        get { return _type; }
        set { _type = value; }
    }
    private IDataEntity _foreignEntity;
    public IDataEntity ForeignEntity
    {
        get { return _foreignEntity; }
        set { _foreignEntity = value; }
    }

    public List<IDataEntityColumn> Fields { get; } = new();
}
