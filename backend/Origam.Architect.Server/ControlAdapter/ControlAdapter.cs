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

using System.ComponentModel;
using System.Reflection;
using System.Xml;
using Origam.Architect.Server.ArchitectLogic;
using Origam.Architect.Server.Attributes;
using Origam.Architect.Server.Controllers;
using Origam.Architect.Server.Controls;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services;
using Origam.Extensions;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.ControlAdapter;

public class ControlAdapter(
    ControlSetItem controlSetItem,
    IControl control,
    EditorPropertyFactory propertyFactory,
    SchemaService schemaService,
    IPersistenceService persistenceService,
    PropertyParser propertyParser
)
{
    public IControl Control { get; } = control;

    [Category("(ORIGAM)")]
    [SchemaItemProperty]
    public string SchemaItemName
    {
        get => controlSetItem.Name;
        set => controlSetItem.Name = value;
    }

    [Category("(ORIGAM)")]
    [SchemaItemProperty]
    public string Roles
    {
        get => controlSetItem.Roles;
        set => controlSetItem.Roles = value;
    }

    [Category("(ORIGAM)")]
    [SchemaItemProperty]
    public string Features
    {
        get => controlSetItem.Features;
        set => controlSetItem.Features = value;
    }

    [ReadOnly(true)]
    [Category("(ORIGAM)")]
    [SchemaItemProperty]
    public string SchemaItemId => controlSetItem.Id.ToString();

    [Category("Behavior")]
    [Description(
        "If set to true, client will attempt to send save request after each change, if there are no errors."
    )]
    [SchemaItemProperty]
    public bool RequestSaveAfterChange
    {
        get => controlSetItem.RequestSaveAfterChange;
        set => controlSetItem.RequestSaveAfterChange = value;
    }

    [Category("Multi Column Adapter Field")]
    [TypeConverter(typeof(DataConstantConverter))]
    [SchemaItemProperty]
    public DataConstant MappingCondition
    {
        get =>
            persistenceService.SchemaProvider.RetrieveInstance<DataConstant>(
                controlSetItem.MultiColumnAdapterFieldCondition
            );
        set => controlSetItem.MultiColumnAdapterFieldCondition = value?.Id ?? Guid.Empty;
    }

    private IEnumerable<PropertyInfo> GetSchemaItemProperties()
    {
        return GetType()
            .GetProperties()
            .Where(propertyInfo =>
            {
                var attr = propertyInfo.GetCustomAttribute<SchemaItemPropertyAttribute>();
                if (attr == null)
                    return false;
                if (propertyInfo.Name == nameof(RequestSaveAfterChange))
                {
                    return controlSetItem.ControlItem.RequestSaveAfterChangeAllowed;
                }
                if (propertyInfo.Name == nameof(MappingCondition))
                {
                    // Don't know how to implement it yet.
                    return false;
                }

                return true;
            });
    }

    public bool UpdateProperties(ChangesModel changes)
    {
        bool changesMade = false;
        foreach (var propertyChange in changes.Changes)
        {
            PropertyBindingInfo bindingInfo = controlSetItem
                .ChildItems.OfType<PropertyBindingInfo>()
                .FirstOrDefault(item => item.ControlPropertyId == propertyChange.ControlPropertyId);
            if (bindingInfo != null)
            {
                if (bindingInfo.Value != propertyChange.Value)
                {
                    changesMade = true;
                }

                bindingInfo.Value = propertyChange.Value;
                continue;
            }
            PropertyValueItem valueItem = controlSetItem
                .ChildItems.OfType<PropertyValueItem>()
                .FirstOrDefault(item => item.ControlPropertyId == propertyChange.ControlPropertyId);
            if (valueItem != null)
            {
                if (valueItem.Value != propertyChange.Value)
                {
                    changesMade = true;
                }

                valueItem.Value = propertyChange.Value;
                continue;
            }

            PropertyInfo schemaItemProperty = GetSchemaItemProperties()
                .FirstOrDefault(x => x.Name == propertyChange.Name);
            if (schemaItemProperty != null)
            {
                object parsedValue = propertyParser.Parse(schemaItemProperty, propertyChange.Value);
                schemaItemProperty.SetValue(this, parsedValue);
                changesMade = true;
            }
        }

        return changesMade;
    }

    public List<EditorProperty> GetEditorProperties(DropDownValue[] dataSourceDropDownValues)
    {
        IEnumerable<EditorProperty> properties = Control
            .GetType()
            .GetProperties()
            .Where(property => property.GetAttribute<NotAModelPropertyAttribute>() == null)
            .Select(property =>
            {
                PropertyBindingInfo bindingInfo = controlSetItem
                    .ChildItems.OfType<PropertyBindingInfo>()
                    .FirstOrDefault(item => item.ControlPropertyItem.Name == property.Name);
                if (bindingInfo != null)
                {
                    return propertyFactory.Create(property, bindingInfo, dataSourceDropDownValues);
                }

                PropertyValueItem valueItem = controlSetItem
                    .ChildItems.OfType<PropertyValueItem>()
                    .FirstOrDefault(item => item.ControlPropertyItem.Name == property.Name);
                return propertyFactory.Create(property, valueItem);
            });
        var schemaItemProperties = GetSchemaItemProperties()
            .Select(property => propertyFactory.Create(property, this));
        return properties.Concat(schemaItemProperties).ToList();
    }

    private ControlPropertyItem FindPropertyItem(string propertyName)
    {
        var propertyItem = controlSetItem
            .ControlItem.ChildItemsByType<ControlPropertyItem>(ControlPropertyItem.CategoryConst)
            .FirstOrDefault(x => x.Name == propertyName);
        if (propertyItem == null)
        {
            throw new Exception("ControlPropertyItem " + propertyName + " not found");
        }

        return propertyItem;
    }

    public void InitializeProperties(int top, int left, int? height = null, int? width = null)
    {
        Control.Initialize(controlSetItem);
        Type type = Control.GetType();
        foreach (var property in type.GetProperties())
        {
            if (property.GetAttribute<NotAModelPropertyAttribute>() != null)
            {
                continue;
            }
            var propertyValueItem = controlSetItem.NewItem<PropertyValueItem>(
                schemaService.ActiveSchemaExtensionId,
                null
            );
            object value = property.GetValue(Control);
            ControlPropertyItem propertyItem = FindPropertyItem(property.Name);
            propertyItem.Name = property.Name;
            propertyValueItem.ControlPropertyItem = propertyItem;
            propertyValueItem.Name = property.Name;
            if (property.PropertyType == typeof(int))
            {
                propertyItem.PropertyType = ControlPropertyValueType.Integer;
                if (property.Name == "Top")
                {
                    propertyValueItem.Value = XmlConvert.ToString(top);
                }
                else if (property.Name == "Left")
                {
                    propertyValueItem.Value = XmlConvert.ToString(left);
                }
                else if (height.HasValue && property.Name == "Height")
                {
                    propertyValueItem.Value = XmlConvert.ToString(height.Value);
                }
                else if (width.HasValue && property.Name == "Width")
                {
                    propertyValueItem.Value = XmlConvert.ToString(width.Value);
                }
                else
                {
                    propertyValueItem.Value =
                        value == null ? null : XmlConvert.ToString((int)value);
                }
            }
            else if (property.PropertyType == typeof(bool))
            {
                propertyItem.PropertyType = ControlPropertyValueType.Boolean;
                propertyValueItem.Value = value == null ? null : XmlConvert.ToString((bool)value);
            }
            else if (property.PropertyType == typeof(Guid))
            {
                propertyItem.PropertyType = ControlPropertyValueType.UniqueIdentifier;
                propertyValueItem.Value = value == null ? null : XmlConvert.ToString((Guid)value);
            }
            else if (property.PropertyType.IsEnum)
            {
                propertyItem.PropertyType = ControlPropertyValueType.String;
                propertyValueItem.Value = Convert.ToInt32(value).ToString();
            }
            else
            {
                propertyItem.PropertyType = ControlPropertyValueType.String;
                propertyValueItem.Value = value?.ToString();
            }
        }
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class SchemaItemPropertyAttribute : Attribute { }
