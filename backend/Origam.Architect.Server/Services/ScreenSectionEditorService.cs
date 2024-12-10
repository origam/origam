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
    public SectionEditorModel Update(PanelControlSet screenSection, SectionEditorChangesModel input)
    {
        bool editorIsDirty = false;
        screenSection.Name = input.Name;
        screenSection.DataSourceId = input.SelectedDataSourceId;
        foreach (var changes in input.ModelChanges)
        {
            ControlSetItem itemToUpdate = screenSection.GetChildByIdRecursive(changes.SchemaItemId) as ControlSetItem;
            if (itemToUpdate == null)
            {
                throw new Exception($"item id: {changes.SchemaItemId} is not in the PanelControlSet");
            }

            if (itemToUpdate.Id != screenSection.MainItem.Id &&
                itemToUpdate.ParentItemId != (changes.ParentSchemaItemId ?? Guid.Empty))
            {
                itemToUpdate.ParentItem.ChildItems.Remove(itemToUpdate);
                if (changes.ParentSchemaItemId != null)
                {
                    ISchemaItem newParent = screenSection.GetChildByIdRecursive(changes
                        .ParentSchemaItemId.Value);
                    newParent.ChildItems.Add(itemToUpdate);
                }
            }

            foreach (var propertyChange in changes.Changes)
            {
                PropertyValueItem valueItem = itemToUpdate.ChildItems
                    .OfType<PropertyValueItem>()
                    .FirstOrDefault(item =>
                        item.ControlPropertyId ==
                        propertyChange.ControlPropertyId);
                if (valueItem != null)
                {
                    if (valueItem.Value != propertyChange.Value)
                    {
                        editorIsDirty = true;
                    }
                    valueItem.Value = propertyChange.Value;
                }
            }
        }

        var editorData = GetSectionEditorData(screenSection);
        return new SectionEditorModel
        {
            Data = editorData,
            IsDirty = editorIsDirty
        };
    }

    public SectionEditorData GetSectionEditorData(ISchemaItem editedItem)
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
            ApiControl apiControl = LoadRootApiControl(screenSection);
            return new SectionEditorData
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
    
    public ApiControl LoadRootApiControl(PanelControlSet screenSection)
    {
        ControlSetItem controlSetItem = screenSection.MainItem;
        return LoadContent(controlSetItem);
    }

    private ApiControl LoadContent(ControlSetItem controlSetItem)
    {
        ApiControl apiControl = LoadItem(controlSetItem);

        var childControls = controlSetItem
            .ChildItemsByType<ControlSetItem>("ControlSetItem");
        foreach (var childControl in childControls)
        {
            if (childControl.IsDeleted)
            {
                continue;
            }
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

    public ApiControl CreateNewItem(ScreenEditorItemModel itemModelData, PanelControlSet screenSection)
    {
        ISchemaItem parent = screenSection.GetChildById(itemModelData.ParentControlSetItemId);
        ControlItem controlItem = schemaService.GetProvider<UserControlSchemaItemProvider>().ChildItems
            .OfType<ControlItem>()
            .FirstOrDefault(item => item.ControlType == itemModelData.ComponentType);
        ControlSetItem newItem = parent.NewItem<ControlSetItem>(
            schemaService.ActiveSchemaExtensionId, null);
        newItem.ControlItem = controlItem;
        newItem.Name = itemModelData.FieldName;
        
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
                    propertyValueItem.Value = XmlConvert.ToString(itemModelData.Top);
                }
                else if (property.Name == "Left")
                {
                    propertyValueItem.Value = XmlConvert.ToString(itemModelData.Left);
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

    public void DeleteItem(Guid schemaItemId, PanelControlSet screenSection)
    {
        ISchemaItem schemaItem = screenSection
            .GetChildByIdRecursive(schemaItemId);
        if (schemaItem is ControlSetItem itemToUpdate)
        {
            itemToUpdate.IsDeleted = true;
        }
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
