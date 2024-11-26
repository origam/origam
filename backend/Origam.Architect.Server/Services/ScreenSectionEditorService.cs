
using System.Reflection;
using Origam.Architect.Server.Models;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class ScreenSectionEditorService(
    SchemaService schemaService,
    IPersistenceService persistenceService)
{
    private readonly IPersistenceProvider persistenceProvider = persistenceService.SchemaProvider;
    
    public SectionEditorModel GetSectionEditorData(ISchemaItem editedItem)
    {
        if (editedItem is PanelControlSet screenSection)
        {
            var entityProvider = schemaService.GetProvider(typeof(EntityModelSchemaItemProvider)) as EntityModelSchemaItemProvider;
            var dataSources = entityProvider.ChildItems
                .Select(x => new DataSource { Name = x.Name, SchemaItemId = x.Id })
                .OrderBy(x => x.Name)
                .ToList();
            
            IDataEntity dataEntity = screenSection.DataEntity;
            
            List<EditorField> fields = dataEntity
                .ChildItemsByType<IDataEntityColumn>(AbstractDataEntityColumn.CategoryConst)
                .OrderBy(field => field.Name)
                .Select(field => new EditorField
                {
                    Name = field.Name, 
                    Type = field.DataType
                })
                .ToList();
            ControlSetItem controlSetItem = screenSection.PanelControl.PanelControlSet.MainItem;
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

    private ApiControl LoadItem(ControlSetItem controlSetItem)
    {
        ApiControl control = new ApiControl
        {
            Type = controlSetItem.ControlItem.Path,
            Id = controlSetItem.Id,
        };
        // var childItemsByType = controlSetItem.ControlItem.ChildItemsByType<ControlPropertyItem>(
        //     ControlPropertyItem.CategoryConst);
        control.ValueItems = controlSetItem.ChildItems
            .OfType<PropertyValueItem>()
            .Select(valueItem => new ApiValueItem
            {
                Name = valueItem.ControlPropertyItem.NodeText,
                Value = valueItem.Value
            }).ToList();
        var bindingInfo = controlSetItem.ChildItems
            .OfType<PropertyBindingInfo>()
            .FirstOrDefault();
        control.Name = bindingInfo?.Value ?? "";
        return control;
    }
}

public class ApiControl
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public List<ApiValueItem> ValueItems { get; set; }
    public List<ApiControl> Children { get; set; } = new();

}

public class ApiValueItem
{
    public string Name { get; set; }
    public string Value { get; set; }
}