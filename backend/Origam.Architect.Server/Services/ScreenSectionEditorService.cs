using System.ComponentModel;
using System.Reflection;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class ScreenSectionEditorService(
    SchemaService schemaService,
    IPersistenceService persistenceService,
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

    private object GetFormObject(string oldFullClassName)
    {
        try
        {
            string className = oldFullClassName
                .Split(".")
                .LastOrDefault();
            string newFullClassName =
                "Origam.Architect.Server.Controls." + className;
            Type type = Type.GetType(newFullClassName);
            return Activator.CreateInstance(type);
        }
        catch (Exception ex)
        {
            throw new Exception("Cannot find a form class for " +
                                oldFullClassName, ex);
        }
    }

    private EditorProperty LoadEditorProperty(ControlSetItem controlSetItem, PropertyInfo property, bool loadDefaults)
    {
        object value;
        if (loadDefaults)
        {
            var defaultValueAttribute = property.GetCustomAttribute(typeof(DefaultValueAttribute)) as DefaultValueAttribute;
            value = defaultValueAttribute?.Value;
        }
        else
        {
            PropertyValueItem valueItem = controlSetItem.ChildItems
                .OfType<PropertyValueItem>()
                .FirstOrDefault(item => item.ControlPropertyItem.NodeText == property.Name);
            value = valueItem?.TypedValue;
        }
        return propertyFactory.Create(property, value);
    }


    private ApiControl LoadItem(ControlSetItem controlSetItem, bool loadDefaults=false)
    {
        if (controlSetItem.RootItem is not PanelControlSet controlSet)
        {
            throw new Exception("Parent object must be " +
                                nameof(PanelControlSet));
        }

        object controlObject = GetFormObject(controlSetItem.ControlItem.ControlType);
        ApiControl control = new ApiControl
        {
            Type = controlSetItem.ControlItem.ControlType,
            Id = controlSetItem.Id
        };
        control.Properties = controlObject.GetType().GetProperties()
            .Select(prop => LoadEditorProperty(controlSetItem, prop, loadDefaults))
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

    public ApiControl CreateNewItem(string controlType, string fieldName, Guid parentControlSetItemId)
    {
        ControlItem controlItem = schemaService.GetProvider<UserControlSchemaItemProvider>().ChildItems
            .OfType<ControlItem>()
            .FirstOrDefault(item => item.ControlType == controlType);
        
        ControlSetItem parent = persistenceService.SchemaProvider.RetrieveInstance<ControlSetItem>(
            parentControlSetItemId);
        ControlSetItem newItem = parent.NewItem<ControlSetItem>(
            schemaService.ActiveSchemaExtensionId, null);
        newItem.ControlItem = controlItem;
        newItem.Name = fieldName;

        return LoadItem(newItem, loadDefaults: true);
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
