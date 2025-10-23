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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.GuiModel;

[SchemaItemDescription(
    "Screen Section Condition",
    "Screen Section Condition",
    "icon_parameter-mapping.png"
)]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class ScreenSectionCondition : AbstractSchemaItem
{
    public const string CategoryConst = "ScreenSectionCondition";
    public override string ItemType => CategoryConst;
    public Guid ScreenSectionId;

    [TypeConverter(typeof(PanelControlSetConverter))]
    [XmlReference("screenSection", "ScreenSectionId")]
    public PanelControlSet ScreenSection
    {
        get =>
            (PanelControlSet)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(ScreenSectionId)
                );
        set
        {
            ScreenSectionId = value?.Id ?? Guid.Empty;
            if (ScreenSectionId != null && ScreenSectionId != Guid.Empty)
            {
                var panelControl = (PanelControlSet)
                    PersistenceProvider.RetrieveInstance(
                        typeof(ISchemaItem),
                        new ModelElementKey(ScreenSectionId)
                    );
                Name = panelControl.Name;
            }
        }
    }

    public ScreenSectionCondition(Guid extensionId)
        : base(extensionId) { }

    public ScreenSectionCondition(Key primaryKey)
        : base(primaryKey) { }

    public ScreenSectionCondition() { }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(ScreenSection);
    }
}
