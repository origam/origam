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
using Origam.Schema.EntityModel;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Schema.GuiModel;

public class GuiHelper
{
    public const string CONTROL_NAME_PANEL = "AsPanel";
    public const string CONTROL_NAME_TEXTBOX = "AsTextBox";
    public const string CONTROL_NAME_COMBOBOX = "AsCombo";
    public const string CONTROL_NAME_CHECKBOX = "AsCheckBox";
    public const string CONTROL_NAME_DATEBOX = "AsDateBox";
    public const string CONTROL_NAME_FORM = "AsForm";
    public const string CONTROL_NAME_MULTICOLUMNADAPTERFIELD = "MultiColumnAdapterFieldWrapper";

    public static FormControlSet CreateForm(
        DataStructure dataSource,
        string groupName,
        PanelControlSet defaultPanel
    )
    {
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var formSchemaItemProvider = schemaService.GetProvider<FormSchemaItemProvider>();
        var schemaItemGroup = formSchemaItemProvider.GetGroup(name: groupName);
        var form = formSchemaItemProvider.NewItem<FormControlSet>(
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        form.Name = dataSource.Name;
        form.Group = schemaItemGroup;
        form.DataStructure = dataSource;
        var rootControl = CreateControl(parentControl: form, controlType: GetFormControl());
        var formProperties = new Hashtable { [key: "Width"] = 700, [key: "Height"] = 400 };
        PopulateControlProperties(control: rootControl, properties: formProperties);
        if (defaultPanel != null)
        {
            var panelControl = CreateControl(
                parentControl: rootControl,
                controlType: defaultPanel.PanelControl
            );
            // clone the panel's properties
            foreach (
                var originalProperty in defaultPanel
                    .ChildItems[index: 0]
                    .ChildItemsByType<PropertyValueItem>(itemType: PropertyValueItem.CategoryConst)
            )
            {
                var property = panelControl.NewItem<PropertyValueItem>(
                    schemaExtensionId: schemaService.ActiveSchemaExtensionId,
                    group: null
                );
                property.ControlPropertyItem = originalProperty.ControlPropertyItem;
                if (property.ControlPropertyItem.Name == "DataMember")
                {
                    property.Value = defaultPanel.DataEntity.Name;
                }
                else
                {
                    property.IntValue = originalProperty.IntValue;
                    property.BoolValue = originalProperty.BoolValue;
                    property.Value = originalProperty.Value;
                    property.GuidValue = originalProperty.GuidValue;
                }
            }
        }
        form.Persist();
        return form;
    }

    public static PanelControlSet CreatePanel(
        string groupName,
        IDataEntity entity,
        Hashtable fieldsToPopulate
    )
    {
        return CreatePanel(
            groupName: groupName,
            entity: entity,
            fieldsToPopulate: fieldsToPopulate,
            name: entity.Name
        );
    }

    public static PanelControlSet CreatePanel(
        string groupName,
        IDataEntity entity,
        Hashtable fieldsToPopulate,
        string name
    )
    {
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var panelSchemaItemProvider = schemaService.GetProvider<PanelSchemaItemProvider>();
        var schemaItemGroup = panelSchemaItemProvider.GetGroup(name: groupName);
        var panel = panelSchemaItemProvider.NewItem<PanelControlSet>(
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        panel.Name = name;
        panel.Group = schemaItemGroup;
        panel.DataEntity = entity;
        var rootControl = CreateControl(parentControl: panel, controlType: GetPanelControl());
        var panelProperties = new Hashtable
        {
            [key: "PanelTitle"] = entity.Caption,
            [key: "Width"] = 600,
            [key: "Height"] = 300,
            [key: "GridVisible"] = true,
            [key: "ShowNewButton"] = true,
            [key: "ShowDeleteButton"] = true,
        };
        PopulateControlProperties(control: rootControl, properties: panelProperties);
        var x = 108;
        var y = 36;
        var i = 0;
        foreach (IDataEntityColumn column in entity.EntityColumns)
        {
            if (!fieldsToPopulate.Contains(key: column.Name))
            {
                continue;
            }
            BuildDefaultControl(
                parentControl: rootControl,
                entity: entity,
                column: column,
                x: x,
                y: y,
                tabIndex: i
            );
            y += 18;
            i++;
        }
        panel.Persist();
        CreatePanelControl(panel: panel);
        return panel;
    }

    private static void BuildDefaultControl(
        ControlSetItem parentControl,
        IDataEntity entity,
        IDataEntityColumn column,
        int x,
        int y,
        int tabIndex
    )
    {
        var properties = new Hashtable
        {
            [key: "Left"] = x,
            [key: "Top"] = y,
            [key: "Height"] = 19,
            [key: "Width"] = 400,
            [key: "Caption"] = "",
            [key: "CaptionLength"] = 100,
            [key: "TabIndex"] = tabIndex,
        };
        switch (column.DataType)
        {
            case OrigamDataType.Integer:
            case OrigamDataType.Long:
            case OrigamDataType.Float:
            case OrigamDataType.Currency:
            case OrigamDataType.Memo:
            case OrigamDataType.Geography:
            case OrigamDataType.String:
            {
                var textBox = CreateControl(
                    parentControl: parentControl,
                    controlType: GetTextBoxControl()
                );
                textBox.Name = textBox.ControlItem.Name + tabIndex;
                PopulateControlProperties(control: textBox, properties: properties);
                PopulateControlBindings(
                    control: textBox,
                    entity: entity.Name,
                    field: column.Name,
                    property: "Value"
                );
                break;
            }
            case OrigamDataType.Boolean:
            {
                properties[key: "Text"] = column.Caption;
                properties[key: "Left"] = x - (int)properties[key: "CaptionLength"]; //checkbox starts on the left
                var checkBox = CreateControl(
                    parentControl: parentControl,
                    controlType: GetCheckBoxControl()
                );
                checkBox.Name = checkBox.ControlItem.Name + tabIndex;
                PopulateControlProperties(control: checkBox, properties: properties);
                PopulateControlBindings(
                    control: checkBox,
                    entity: entity.Name,
                    field: column.Name,
                    property: "Value"
                );
                break;
            }
            case OrigamDataType.Date:
            {
                var dateBox = CreateControl(
                    parentControl: parentControl,
                    controlType: GetDateBoxControl()
                );
                dateBox.Name = dateBox.ControlItem.Name + tabIndex;
                PopulateControlProperties(control: dateBox, properties: properties);
                PopulateControlBindings(
                    control: dateBox,
                    entity: entity.Name,
                    field: column.Name,
                    property: "DateValue"
                );
                break;
            }
            case OrigamDataType.UniqueIdentifier:
            {
                if (column.DefaultLookup == null)
                {
                    throw new Exception(
                        message: ResourceUtils.GetString(
                            key: "ErrorLookupNotSet",
                            args: entity.Name + "/" + column.Name
                        )
                    );
                }
                properties[key: "LookupId"] = column.DefaultLookup.PrimaryKey[key: "Id"];
                var comboBox = CreateControl(
                    parentControl: parentControl,
                    controlType: GetComboBoxControl()
                );
                comboBox.Name = comboBox.ControlItem.Name + tabIndex;
                PopulateControlProperties(control: comboBox, properties: properties);
                PopulateControlBindings(
                    control: comboBox,
                    entity: entity.Name,
                    field: column.Name,
                    property: "LookupValue"
                );
                break;
            }
            case OrigamDataType.Object:
            {
                var multiColumnAdapterFieldWrapper = CreateControl(
                    parentControl: parentControl,
                    controlType: GetMultiColumnAdapterFieldWrapperControl()
                );
                multiColumnAdapterFieldWrapper.Name =
                    multiColumnAdapterFieldWrapper.ControlItem.Name + tabIndex;
                PopulateControlProperties(
                    control: multiColumnAdapterFieldWrapper,
                    properties: properties
                );
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "DataType",
                    actualValue: column.DataType,
                    message: "Default control of this data type is not supported by the control builder."
                );
            }
        }
    }

    public static ControlSetItem CreateControl(ISchemaItem parentControl, ControlItem controlType)
    {
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var control = parentControl.NewItem<ControlSetItem>(
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        control.ControlItem = controlType;
        return control;
    }

    public static ControlItem CreatePanelControl(PanelControlSet panel)
    {
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var userControlSchemaItemProvider =
            schemaService.GetProvider<UserControlSchemaItemProvider>();
        var newControl = userControlSchemaItemProvider.NewItem<ControlItem>(
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        newControl.Name = panel.Name;
        newControl.IsComplexType = true;
        var panelControlSetType = typeof(PanelControlSet);
        newControl.ControlType = panelControlSetType.ToString();
        newControl.ControlNamespace = panelControlSetType.Namespace;
        newControl.PanelControlSet = panel;
        newControl.ControlToolBoxVisibility = ControlToolBoxVisibility.FormDesigner;
        var ancestor = new SchemaItemAncestor();
        ancestor.SchemaItem = newControl;
        ancestor.Ancestor = GetPanelControl();
        ancestor.PersistenceProvider = newControl.PersistenceProvider;
        newControl.ThrowEventOnPersist = false;
        newControl.Persist();
        ancestor.Persist();
        newControl.ThrowEventOnPersist = true;
        return newControl;
    }

    public static ControlItem GetPanelControl()
    {
        return GetControlByName(name: CONTROL_NAME_PANEL);
    }

    public static ControlItem GetFormControl()
    {
        return GetControlByName(name: CONTROL_NAME_FORM);
    }

    public static ControlItem GetTextBoxControl()
    {
        return GetControlByName(name: CONTROL_NAME_TEXTBOX);
    }

    public static ControlItem GetCheckBoxControl()
    {
        return GetControlByName(name: CONTROL_NAME_CHECKBOX);
    }

    public static ControlItem GetDateBoxControl()
    {
        return GetControlByName(name: CONTROL_NAME_DATEBOX);
    }

    public static ControlItem GetMultiColumnAdapterFieldWrapperControl()
    {
        return GetControlByName(name: CONTROL_NAME_MULTICOLUMNADAPTERFIELD);
    }

    public static ControlItem GetComboBoxControl()
    {
        return GetControlByName(name: CONTROL_NAME_COMBOBOX);
    }

    public static ControlItem GetControlByName(string name)
    {
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var userControlSchemaItemProvider =
            schemaService.GetProvider<UserControlSchemaItemProvider>();
        return userControlSchemaItemProvider.GetChildByName(
                name: name,
                itemType: ControlItem.CategoryConst
            ) as ControlItem;
    }

    private static void PopulateControlBindings(
        ControlSetItem control,
        string entity,
        string field,
        string property
    )
    {
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var propertyBindingInfo = control.NewItem<PropertyBindingInfo>(
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        propertyBindingInfo.ControlPropertyItem =
            control.ControlItem.GetChildByName(name: property) as ControlPropertyItem;
        propertyBindingInfo.Value = field;
        propertyBindingInfo.DesignDataSetPath = entity + "." + field;
    }

    private static void PopulateControlProperties(ControlSetItem control, Hashtable properties)
    {
        var schema = ServiceManager.Services.GetService<ISchemaService>();
        foreach (
            var propertyDef in control.ControlItem.ChildItemsByType<ControlPropertyItem>(
                itemType: ControlPropertyItem.CategoryConst
            )
        )
        {
            var propertyValueItem = control.NewItem<PropertyValueItem>(
                schemaExtensionId: schema.ActiveSchemaExtensionId,
                group: null
            );
            propertyValueItem.ControlPropertyItem = propertyDef;
            if (properties.Contains(key: propertyDef.Name))
            {
                propertyValueItem.SetValue(value: properties[key: propertyDef.Name]);
            }
        }
    }
}
