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
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;

namespace Origam.Schema.WorkflowModel;

[SchemaItemDescription(
    name: "Custom Screen",
    folderName: "Custom Screens",
    iconName: "icon_screen.png"
)]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class WorkQueueCustomScreen : AbstractSchemaItem
{
    public const string CategoryConst = "WorkQueueCustomScreen";

    public override string ItemType => CategoryConst;

    public WorkQueueCustomScreen() { }

    public WorkQueueCustomScreen(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public WorkQueueCustomScreen(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    public Guid ScreenId;

    [Category(category: "Custom Work Queue Screen")]
    [TypeConverter(type: typeof(FormControlSetConverter))]
    [NotNullModelElementRule]
    [XmlReference(attributeName: "screen", idField: "ScreenId")]
    public FormControlSet Screen
    {
        get =>
            (FormControlSet)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: ScreenId)
                );
        set
        {
            if ((value == null) || !ScreenId.Equals(g: (Guid)value.PrimaryKey[key: "Id"]))
            {
                Method = null;
            }
            ScreenId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
        }
    }
    public Guid MethodId;

    [Category(category: "Custom Work Queue Screen")]
    [TypeConverter(type: typeof(WorkQueueCustomScreenMethodConverter))]
    [XmlReference(attributeName: "method", idField: "MethodId")]
    public DataStructureMethod Method
    {
        get =>
            (DataStructureMethod)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: MethodId)
                );
        set => MethodId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: Screen);
        if (Method != null)
        {
            dependencies.Add(item: Method);
        }
        base.GetExtraDependencies(dependencies: dependencies);
    }
}

class WorkQueueCustomScreenMethodConverter : TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
        var currentItem = context.Instance as WorkQueueCustomScreen;
        List<DataStructureMethod> methods = currentItem?.Screen?.DataStructure?.Methods;
        if (methods == null)
        {
            return new StandardValuesCollection(values: new List<DataStructureMethod>());
        }
        var output = new List<DataStructureMethod>(capacity: methods.Count);
        foreach (DataStructureMethod method in methods)
        {
            output.Add(item: method);
        }
        output.Add(item: null);
        output.Sort();
        return new StandardValuesCollection(values: output);
    }

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return (sourceType == typeof(string))
            || base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value?.GetType() != typeof(string))
        {
            return base.ConvertFrom(context: context, culture: culture, value: value);
        }
        var currentItem = context.Instance as WorkQueueCustomScreen;
        List<DataStructureMethod> methods = currentItem?.Screen.DataStructure.Methods;
        if (methods == null)
        {
            return null;
        }
        foreach (DataStructureMethod item in methods)
        {
            if (item.Name == value.ToString())
            {
                return item;
            }
        }
        return null;
    }
}
