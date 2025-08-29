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
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel;

/// <summary>
/// Summary description for DataQuery.
/// </summary>
[XmlModelRoot(CategoryConst)]
public abstract class DataStructureMethod : AbstractSchemaItem
{
    public const string CategoryConst = "DataStructureFilterSet";

    public DataStructureMethod()
        : base() { }

    public DataStructureMethod(Guid schemaExtensionId)
        : base(schemaExtensionId) { }

    public DataStructureMethod(Key primaryKey)
        : base(primaryKey) { }

    #region Overriden AbstractDataEntityColumn Members

    public override string ItemType
    {
        get { return CategoryConst; }
    }
    public override bool UseFolders
    {
        get { return false; }
    }

    public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
    {
        return (newNode as ISchemaItem).PrimaryKey.Equals(this.ParentItem.PrimaryKey);
    }
    #endregion
}
