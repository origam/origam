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
using System.Windows.Forms;
using Origam.Gui.Win;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Gui.Designer;

[ProvideProperty(propertyName: "MappingCondition", receiverType: typeof(Control))]
public class MultiColumnAdapterFieldExtenderProvider : IExtenderProvider
{
    [Category(category: "Multi Column Adapter Field")]
    [TypeConverter(type: typeof(DataConstantConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    public DataConstant GetMappingCondition(Control acontrol)
    {
        ControlSetItem csi = acontrol.Tag as ControlSetItem;
        if (csi != null)
        {
            IPersistenceService persistence =
                ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
                as IPersistenceService;
            return (DataConstant)
                persistence.SchemaProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: csi.MultiColumnAdapterFieldCondition)
                );
        }

        return null;
    }

    public void SetMappingCondition(Control acontrol, DataConstant value)
    {
        ControlSetItem csi = acontrol.Tag as ControlSetItem;
        if (csi != null)
        {
            csi.MultiColumnAdapterFieldCondition = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }

    public bool CanExtend(object extendee)
    {
        if (
            extendee is Control
            && ((extendee as Control).Parent is MultiColumnAdapterFieldWrapper)
            && (extendee as Control).Tag is ISchemaItem
        )
        {
            return true;
        }

        return false;
    }
}
