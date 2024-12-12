using Origam.Architect.Server.ControlAdapter;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class ScreenSectionEditorService(
    SchemaService schemaService,
    ControlAdapterFactory adapterFactory)
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
            
            ControlAdapter.ControlAdapter controlAdapter = adapterFactory.Create(itemToUpdate);
            editorIsDirty = controlAdapter.UpdateProperties(changes);
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
    
    private ApiControl LoadItem(ControlSetItem controlSetItem)
    {
        if (controlSetItem.RootItem is not PanelControlSet controlSet)
        {
            throw new Exception("Parent object must be " +
                                nameof(PanelControlSet));
        }

        ControlAdapter.ControlAdapter controlAdapter = adapterFactory.Create(controlSetItem);
        ApiControl apiControl = new ApiControl
        {
            Type = controlSetItem.ControlItem.ControlType,
            Id = controlSetItem.Id
        };

        apiControl.Properties = controlAdapter.GetEditorProperties();
        
        var bindingInfo = controlSetItem.ChildItems
            .OfType<PropertyBindingInfo>()
            .FirstOrDefault();
        var caption = controlSet.DataEntity
            .ChildItemsByType<IDataEntityColumn>(AbstractDataEntityColumn
                .CategoryConst)
            .FirstOrDefault(x => x.Name == bindingInfo?.Value)
            ?.Caption ?? bindingInfo?.Value;
        apiControl.Name = caption ?? controlSetItem.Name;
        return apiControl;
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
        
        ControlAdapter.ControlAdapter controlAdapter = adapterFactory.Create(newItem);
        controlAdapter.InitializeProperties(
            top: itemModelData.Top, 
            left: itemModelData.Left);
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
