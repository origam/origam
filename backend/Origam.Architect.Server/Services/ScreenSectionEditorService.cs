using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class ScreenSectionEditorService(
    SchemaService schemaService,
    EditorPropertyFactory propertyFactory)
{
    public SectionEditorModel GetSectionEditorData(ISchemaItem editedItem)
    {
        if (editedItem is PanelControlSet screenSection)
        {
            var entityProvider =
                schemaService.GetProvider(typeof(EntityModelSchemaItemProvider))
                    as EntityModelSchemaItemProvider;
            var dataSources = entityProvider.ChildItems
                .Select(x => new DataSource
                    { Name = x.Name, SchemaItemId = x.Id })
                .OrderBy(x => x.Name)
                .ToList();

            IDataEntity dataEntity = screenSection.DataEntity;

            List<EditorField> fields = dataEntity
                .ChildItemsByType<IDataEntityColumn>(AbstractDataEntityColumn
                    .CategoryConst)
                .OrderBy(field => field.Name)
                .Select(field => new EditorField
                {
                    Name = field.Name,
                    Type = field.DataType
                })
                .ToList();
            ControlSetItem controlSetItem =
                screenSection.PanelControl.PanelControlSet.MainItem;
            ApiControl apiControl = LoadContent(controlSetItem);
            return new SectionEditorModel
            {
                Name = editedItem.Name,
                SchemaExtensionId = editedItem.SchemaExtensionId,
                DataSources = dataSources,
                RootControl = apiControl,
                SelectedDataSourceId = screenSection.DataEntity.Id,
                Fields = fields
            };
        }

        return null;
    }

    private ApiControl LoadContent(ControlSetItem controlSetItem)
    {
        ApiControl apiControl = LoadItem(controlSetItem);

        var childControls = controlSetItem
            .ChildItemsByType<ControlSetItem>("ControlSetItem");
        foreach (var childControl in childControls)
        {
            var child = LoadContent(childControl);
            apiControl.Children.Add(child);
        }

        return apiControl;
    }

    private Type GetControlType(string oldFullClassName)
    {
        try
        {
            string className = oldFullClassName
                .Split(".")
                .LastOrDefault();
            string newFullClassName =
                "Origam.Architect.Server.Controls." + className;
            return Type.GetType(newFullClassName);
        }
        catch (Exception ex)
        {
            throw new Exception("Cannot find a form class for " +
                                oldFullClassName, ex);
        }
    }
    
    private ApiControl LoadItem(ControlSetItem controlSetItem)
    {
        if (controlSetItem.RootItem is not PanelControlSet controlSet)
        {
            throw new Exception("Parent object must be " +
                                nameof(PanelControlSet));
        }

        Type controlType = GetControlType(controlSetItem.ControlItem.ControlType);
        ApiControl control = new ApiControl
        {
            Type = controlSetItem.ControlItem.ControlType,
            Id = controlSetItem.Id
        };
        control.Properties = controlType.GetProperties()
            .Select(property =>
            {
                PropertyValueItem valueItem = controlSetItem.ChildItems
                    .OfType<PropertyValueItem>()
                    .FirstOrDefault(item => item.ControlPropertyItem.Name == property.Name);
                return propertyFactory.Create(property, valueItem);
            })
            .ToList();
        var bindingInfo = controlSetItem.ChildItems
            .OfType<PropertyBindingInfo>()
            .FirstOrDefault();
        var caption = controlSet.DataEntity
            .ChildItemsByType<IDataEntityColumn>(AbstractDataEntityColumn
                .CategoryConst)
            .FirstOrDefault(x => x.Name == bindingInfo?.Value)
            ?.Caption ?? bindingInfo?.Value;
        control.Name = caption ?? controlSetItem.Name;
        return control;
    }

    public ApiControl CreateNewItem(ScreenEditorItem itemData, PanelControlSet screenSection)
    {
        ISchemaItem parent = screenSection.PanelControl.PanelControlSet.GetChildById(itemData.ParentControlSetItemId);
        ControlItem controlItem = schemaService.GetProvider<UserControlSchemaItemProvider>().ChildItems
            .OfType<ControlItem>()
            .FirstOrDefault(item => item.ControlType == itemData.ComponentType);
        ControlSetItem newItem = parent.NewItem<ControlSetItem>(
            schemaService.ActiveSchemaExtensionId, null);
        newItem.ControlItem = controlItem;
        newItem.Name = itemData.FieldName;
        
        Type controlType = GetControlType(newItem.ControlItem.ControlType);
        foreach (var property in controlType.GetProperties())
        {
            var defaultValueAttribute = property.GetCustomAttribute(typeof(DefaultValueAttribute)) as DefaultValueAttribute;
            var propertyValueItem = newItem.NewItem<PropertyValueItem>(schemaService.ActiveSchemaExtensionId, null);
            object value = defaultValueAttribute?.Value;
            var propertyItem = new ControlPropertyItem(schemaService.ActiveSchemaExtensionId);
            propertyItem.Name = property.Name;
            propertyValueItem.ControlPropertyItem = propertyItem;
            if (property.PropertyType == typeof(int))
            {
                propertyItem.PropertyType = ControlPropertyValueType.Integer;
                if (property.Name == "Top")
                {
                    propertyValueItem.Value = XmlConvert.ToString(itemData.Top);
                }
                else if (property.Name == "Left")
                {
                    propertyValueItem.Value = XmlConvert.ToString(itemData.Left);
                }
                else
                {
                    propertyValueItem.Value = value == null ? null : XmlConvert.ToString((int)value);
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
            else
            {
                propertyItem.PropertyType = ControlPropertyValueType.String;
                propertyValueItem.Value = value?.ToString();
            }
        }
            
        return LoadItem(newItem);
    }
}

public class ApiControl
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public List<EditorProperty> Properties { get; set; }
    public List<ApiControl> Children { get; set; } = new();
}
