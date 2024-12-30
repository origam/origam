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
    public bool Update(AbstractControlSet screenSection, SectionEditorChangesModel input)
    {
        bool editorIsDirty = false;
        screenSection.Name = input.Name;
        screenSection.DataSourceId = input.SelectedDataSourceId;
        foreach (var changes in input.ModelChanges)
        {
            ControlSetItem itemToUpdate = screenSection.GetChildByIdRecursive(changes.SchemaItemId) as ControlSetItem;
            if (itemToUpdate == null)
            {
                throw new Exception($"Child with id: {changes.SchemaItemId} not found in {screenSection.Id}");
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

        return editorIsDirty;
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
            
            List<EditorField> fields = GetFields(screenSection);
            ApiControl apiControl =  LoadContent(screenSection.MainItem, fields);
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
    public ScreenEditorData GetScreenEditorData(ISchemaItem editedItem)
    {
        if (editedItem is FormControlSet screen)
        {
            var dataStructureProvider =
                schemaService.GetProvider<DataStructureSchemaItemProvider>();
            var dataSources = dataStructureProvider.ChildItems
                .Select(x => new DataSource
                    { Name = x.Name, SchemaItemId = x.Id })
                .OrderBy(x => x.Name)
                .ToList();
            
            var userControlProvider =
                schemaService.GetProvider<UserControlSchemaItemProvider>();
            
            var sections = userControlProvider.ChildItems
                .OfType<ControlItem>()
                .Where(item => item.ControlType != "Origam.Gui.Win.AsForm" && 
                               item.IsComplexType && 
                               item.ControlToolBoxVisibility != ControlToolBoxVisibility.Nowhere)
                .Select(item => new ToolBoxItem{Name = item.Name, Id = item.Id})
                .OrderBy(x => x.Name);
            
            var widgets = userControlProvider.ChildItems
                .OfType<ControlItem>()
                .Where(item => item.ControlType != "Origam.Gui.Win.AsForm" && 
                               !item.IsComplexType && 
                               item.ControlToolBoxVisibility is 
                                   ControlToolBoxVisibility.FormDesigner or 
                                   ControlToolBoxVisibility.PanelAndFormDesigner)
                .Select(item => new ToolBoxItem{Name = item.Name, Id = item.Id})
                .OrderBy(x => x.Name);
            
            ApiControl apiControl =  LoadContent(screen.MainItem, new List<EditorField>());
            return new ScreenEditorData
            {
                Name = editedItem.Name,
                SchemaExtensionId = editedItem.SchemaExtensionId,
                DataSources = dataSources,
                RootControl = apiControl,
                SelectedDataSourceId = screen.DataSourceId,
                Sections = sections,
                Widgets = widgets
            };
        }

        return null;
    }

    private static List<EditorField> GetFields(PanelControlSet screenSection)
    {
        IDataEntity dataEntity = screenSection.DataEntity;
        return dataEntity
            .ChildItemsByType<IDataEntityColumn>(AbstractDataEntityColumn
                .CategoryConst)
            .OrderBy(field => field.Name)
            .Select(field => new EditorField
            {
                Name = field.Name,
                Type = field.DataType
            })
            .ToList();
    }

    private ApiControl LoadContent(ControlSetItem controlSetItem,
        List<EditorField> fields)
    {
        ApiControl apiControl = LoadItem(controlSetItem, fields);

        var childControls = controlSetItem
            .ChildItemsByType<ControlSetItem>("ControlSetItem");
        foreach (var childControl in childControls)
        {
            if (childControl.IsDeleted)
            {
                continue;
            }
            var child = LoadContent(childControl, fields);
            apiControl.Children.Add(child);
        }

        return apiControl;
    }
    
    private ApiControl LoadItem(ControlSetItem controlSetItem,
        List<EditorField> fields)
    {
        ControlAdapter.ControlAdapter controlAdapter = adapterFactory.Create(controlSetItem);
        ApiControl apiControl = new ApiControl
        {
            Type = controlSetItem.ControlItem.ControlType,
            Id = controlSetItem.Id,
            Properties = controlAdapter.GetEditorProperties(fields)
        };

        if (controlSetItem.RootItem is PanelControlSet controlSet)
        {
            var bindingInfo = controlSetItem.ChildItems
                .OfType<PropertyBindingInfo>()
                .FirstOrDefault();
            var caption = controlSet.DataEntity
                .ChildItemsByType<IDataEntityColumn>(AbstractDataEntityColumn
                    .CategoryConst)
                .FirstOrDefault(x => x.Name == bindingInfo?.Value)
                ?.Caption ?? bindingInfo?.Value;
            apiControl.Name = caption ?? controlSetItem.Name;
        }
        else
        {
            apiControl.Name = controlSetItem.RootItem.Name;
        }

        return apiControl;
    }

    public ApiControl CreateNewItem(SectionEditorItemModel itemModelData, PanelControlSet screenSection)
    {
        ISchemaItem parent = screenSection.GetChildByIdRecursive(itemModelData.ParentControlSetItemId);
        if (parent == null)
        {
            throw new Exception(
                $"Parent object {itemModelData.ParentControlSetItemId} not found");
        }
        ControlItem controlItem = schemaService.GetProvider<UserControlSchemaItemProvider>()
            .ChildItems
            .OfType<ControlItem>()
            .First(item => item.ControlType == itemModelData.ComponentType);
        ControlSetItem newItem = parent.NewItem<ControlSetItem>(
            schemaService.ActiveSchemaExtensionId, null);
        newItem.ControlItem = controlItem;
        newItem.Name = itemModelData.FieldName ?? controlItem.Name;
        
        ControlAdapter.ControlAdapter controlAdapter = adapterFactory.Create(newItem);
        controlAdapter.InitializeProperties(
            top: itemModelData.Top, 
            left: itemModelData.Left);
        List<EditorField> fields = GetFields(screenSection);
        return LoadItem(newItem, fields);
    }

    public ApiControl CreateNewItem(ScreenEditorItemModel itemModelData, FormControlSet screen)
    {
        ISchemaItem parent = screen.GetChildByIdRecursive(itemModelData.ParentControlSetItemId);
        if (parent == null)
        {
            throw new Exception(
                $"Parent object {itemModelData.ParentControlSetItemId} not found");
        }
        ControlItem controlItem = schemaService.GetProvider<UserControlSchemaItemProvider>()
            .ChildItems
            .OfType<ControlItem>()
            .First(item => item.Id == itemModelData.ControlItemId); // This will have to be done some other way in case of a plugin. See ControlSetEditor.GetControlbyType(Type type)

        ControlSetItem newItem = parent.NewItem<ControlSetItem>(
            schemaService.ActiveSchemaExtensionId, null);
        newItem.ControlItem = controlItem;
        newItem.Name = controlItem.Name;

        ControlAdapter.ControlAdapter controlAdapter = adapterFactory.Create(newItem);
        controlAdapter.InitializeProperties(
            top: itemModelData.Top,
            left: itemModelData.Left);
        return LoadItem(newItem, new List<EditorField>());


        // if(bind == null)
        //     return;
		      //
        // Control cntrl = bind.Control;
        // if(cntrl ==null)
        //     return;
        //
        // ControlPropertyItem propItem= null;
        // if(!(cntrl.Tag is ControlSetItem))
        //     return;
        // ControlSetItem cntrSetItem=cntrl.Tag as ControlSetItem;
        // propItem=FindPropertyItem(cntrl,bind.PropertyName);
			     //
        // if(propItem==null)
        //     throw new NullReferenceException("Property " + bind.PropertyName + " definition (control: " + cntrl.Name + ") doesn't exists");
        // PropertyBindingInfo propertyBind=FindPropertyValueItem(cntrSetItem,propItem,true) as PropertyBindingInfo;
		      //
        // if(action == CollectionChangeAction.Remove)
        // {
        //     propertyBind.IsDeleted = true;
        //     return;
        // }
        //
        // if(propertyBind==null)
        //     throw new NullReferenceException("Property binding value (" + bind.PropertyName + ") definition (control: " + cntrl.Name + ") doesn't exists or can't creat new one");
        // propertyBind.ControlPropertyItem = propItem;
        // propertyBind.Name =bind.PropertyName;
        // propertyBind.Value=bind.BindingMemberInfo.BindingField;
        // propertyBind.DesignDataSetPath = bind.BindingMemberInfo.BindingMember;

    }

    public void DeleteItem(Guid schemaItemId, ISchemaItem rootItem)
    {
        ISchemaItem schemaItem = rootItem
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
