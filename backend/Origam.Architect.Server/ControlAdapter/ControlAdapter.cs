using System.ComponentModel;
using System.Reflection;
using System.Xml;
using Origam.Architect.Server.Controllers;
using Origam.Architect.Server.ReturnModels;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.ControlAdapter;

public class ControlAdapter(
    ControlSetItem controlSetItem,
    Type controlType,
    EditorPropertyFactory propertyFactory,
    SchemaService schemaService,
    IPersistenceService persistenceService)
{
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
    [Description("If set to true, client will attempt to send save request after each change, if there are no errors.")]
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
            persistenceService.SchemaProvider
                .RetrieveInstance<DataConstant>(controlSetItem.MultiColumnAdapterFieldCondition);
        set => controlSetItem.MultiColumnAdapterFieldCondition = 
            value?.Id ?? Guid.Empty;
    }
    
    private IEnumerable<PropertyInfo> GetSchemaItemProperties()
    {
        return GetType().GetProperties()
            .Where(propertyInfo =>
            {
                var attr = propertyInfo
                    .GetCustomAttribute<SchemaItemPropertyAttribute>();
                if (attr == null) return false;
                if (propertyInfo.Name == nameof(RequestSaveAfterChange))
                {
                    return controlSetItem.ControlItem
                        .RequestSaveAfterChangeAllowed;
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
            PropertyValueItem valueItem = controlSetItem.ChildItems
                .OfType<PropertyValueItem>()
                .FirstOrDefault(item =>
                    item.ControlPropertyId ==
                    propertyChange.ControlPropertyId);
            if (valueItem != null)
            {
                if (valueItem.Value != propertyChange.Value)
                {
                    changesMade = true;
                }

                valueItem.Value = propertyChange.Value;
            }

            PropertyInfo schemaItemProperty = GetSchemaItemProperties()
                .FirstOrDefault(x => x.Name == propertyChange.Name);
            if (schemaItemProperty != null)
            {
                schemaItemProperty.SetValue(this, propertyChange.Value);
                changesMade = true;
            }
        }

        return changesMade;
    }

    public List<EditorProperty> GetEditorProperties()
    {
        List<EditorProperty> properties = controlType.GetProperties()
            .Select(property =>
            {
                PropertyValueItem valueItem = controlSetItem.ChildItems
                    .OfType<PropertyValueItem>()
                    .FirstOrDefault(item =>
                        item.ControlPropertyItem.Name == property.Name);
                return propertyFactory.Create(property, valueItem);
            })
            .ToList();

        var schemaItemProperties = GetSchemaItemProperties()
            .Select(property => propertyFactory.Create(property, this));
        properties.AddRange(schemaItemProperties);
        return properties;
    }

    public void InitializeProperties(int top, int left)
    {
        foreach (var property in controlType.GetProperties())
        {
            var defaultValueAttribute =
                property.GetCustomAttribute(typeof(DefaultValueAttribute)) as
                    DefaultValueAttribute;
            var propertyValueItem =
                controlSetItem.NewItem<PropertyValueItem>(
                    schemaService.ActiveSchemaExtensionId, null);
            object value = defaultValueAttribute?.Value;
            var propertyItem =
                new ControlPropertyItem(schemaService.ActiveSchemaExtensionId);
            propertyItem.Name = property.Name;
            propertyValueItem.ControlPropertyItem = propertyItem;
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
                else
                {
                    propertyValueItem.Value = value == null
                        ? null
                        : XmlConvert.ToString((int)value);
                }
            }
            else if (property.PropertyType == typeof(bool))
            {
                propertyItem.PropertyType = ControlPropertyValueType.Boolean;
                propertyValueItem.Value = value == null
                    ? null
                    : XmlConvert.ToString((bool)value);
            }
            else if (property.PropertyType == typeof(Guid))
            {
                propertyItem.PropertyType =
                    ControlPropertyValueType.UniqueIdentifier;
                propertyValueItem.Value = value == null
                    ? null
                    : XmlConvert.ToString((Guid)value);
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
public class SchemaItemPropertyAttribute : Attribute
{
}