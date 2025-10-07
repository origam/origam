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
using System.ComponentModel;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel;

[SchemaItemDescription("Filter", "Filters", "icon_filter.png")]
[HelpTopic("Filters")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class EntityFilter : AbstractSchemaItem
{
    public const string CategoryConst = "EntityFilter";

    public EntityFilter() { }

    public EntityFilter(Guid schemaExtensionId)
        : base(schemaExtensionId) { }

    public EntityFilter(Key primaryKey)
        : base(primaryKey) { }

    #region Overriden ISchemaItem Members
    public override bool CanMove(UI.IBrowserNode2 newNode)
    {
        return newNode is IDataEntity;
    }

    public override string ItemType => CategoryConst;
    public override bool UseFolders => false;
    #endregion
    #region ISchemaItemFactory Members
    [Browsable(false)]
    public override Type[] NewItemTypes => new[] { typeof(FunctionCall) };
    #endregion
}
