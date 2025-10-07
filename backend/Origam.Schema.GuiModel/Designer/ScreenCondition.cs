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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.GuiModel;

[SchemaItemDescription("Screen Condition", "Screen Condition", "icon_parameter-mapping.png")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class ScreenCondition : AbstractSchemaItem
{
    public const string CategoryConst = "ScreenCondition";
    public override string ItemType => CategoryConst;
    public Guid ScreenId;

    [TypeConverter(typeof(FormControlSetConverter))]
    [XmlReference("screen", "ScreenId")]
    public FormControlSet Screen
    {
        get =>
            (FormControlSet)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(ScreenId)
                );
        set
        {
            ScreenId = value?.Id ?? Guid.Empty;
            if (ScreenId != null && ScreenId != Guid.Empty)
            {
                var formControl = (FormControlSet)
                    PersistenceProvider.RetrieveInstance(
                        typeof(ISchemaItem),
                        new ModelElementKey(ScreenId)
                    );
                Name = formControl.Name;
            }
        }
    }

    public ScreenCondition(Guid extensionId)
        : base(extensionId) { }

    public ScreenCondition(Key primaryKey)
        : base(primaryKey) { }

    public ScreenCondition() { }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(Screen);
    }
}
