using Origam.Architect.Server.ControlAdapter;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class DesignerEditorService(
    SchemaService schemaService,
    ControlAdapterFactory adapterFactory)
{
    private readonly Guid tabControlControlItemId = new ("2e39362b-80a6-4430-a9bd-b3013583a2fe");
    private readonly Guid tabPageControlItemId = new ("6d13ec20-3b17-456e-ae43-3021cb067a70");
    private readonly List<string> implementedScreenWidgets = ["TabControl", "SplitPanel", "AsTree"];
    public bool Update(AbstractControlSet screenSection,
        SectionEditorChangesModel input)
    {
        bool editorIsDirty = false;
        if (screenSection.Name != input.Name)
        {
            screenSection.Name = input.Name;
            editorIsDirty = true;
        }
        if (screenSection.DataSourceId != input.SelectedDataSourceId)
        {
            screenSection.DataSourceId = input.SelectedDataSourceId;
            editorIsDirty = true;
        }
        foreach (var changes in input.ModelChanges)
        {
            ControlSetItem itemToUpdate =
                screenSection.GetChildByIdRecursive(changes.SchemaItemId) as
                    ControlSetItem;
            if (itemToUpdate == null)
            {
                throw new Exception(
                    $"Child with id: {changes.SchemaItemId} not found in {screenSection.Id}");
            }

            if (itemToUpdate.Id != screenSection.MainItem.Id &&
                itemToUpdate.ParentItemId !=
                (changes.ParentSchemaItemId ?? Guid.Empty))
            {
                itemToUpdate.ParentItem.ChildItems.Remove(itemToUpdate);
                if (changes.ParentSchemaItemId != null)
                {
                    ISchemaItem newParent = screenSection.GetChildByIdRecursive(
                        changes
                            .ParentSchemaItemId.Value);
                    newParent.ChildItems.Add(itemToUpdate);
                }
            }

            ControlAdapter.ControlAdapter controlAdapter =
                adapterFactory.Create(itemToUpdate);
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
            dataSources.Insert(0,  DataSource.Empty);

            List<EditorField> fields = GetFields(screenSection);
            DropDownValue[] dataSourceDropDownValues = fields
                .Select(field => new DropDownValue(field.Name, field.Name))
                .ToArray();
            ApiControl apiControl = LoadContent(screenSection.MainItem,
                dataSourceDropDownValues);
            return new SectionEditorData
            {
                Name = editedItem.Name,
                SchemaExtensionId = editedItem.SchemaExtensionId,
                DataSources = dataSources,
                RootControl = apiControl,
                SelectedDataSourceId = screenSection.DataEntity?.Id ?? Guid.Empty,
                Fields = GetFields(screenSection)
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
            if (dataStructureProvider == null)
            {
                throw new UserOrigamException(
                    $"No package is active. Select a package first.");
            }

            var dataSources = dataStructureProvider.ChildItems
                .Select(x => new DataSource
                    { Name = x.Name, SchemaItemId = x.Id })
                .OrderBy(x => x.Name)
                .ToList();
            dataSources.Insert(0,  DataSource.Empty);

            var userControlProvider =
                schemaService.GetProvider<UserControlSchemaItemProvider>();

            var sections = userControlProvider.ChildItems
                .OfType<ControlItem>()
                .Where(item => item.ControlType != "Origam.Gui.Win.AsForm" &&
                               item.IsComplexType &&
                               item.ControlToolBoxVisibility !=
                               ControlToolBoxVisibility.Nowhere)
                .Select(item => new ToolBoxItem
                    { Name = item.Name, Id = item.Id })
                .OrderBy(x => x.Name);

            var widgets = userControlProvider.ChildItems
                .OfType<ControlItem>()
                .Where(item => item.ControlType != "Origam.Gui.Win.AsForm" && 
                               !item.IsComplexType && 
                               item.ControlToolBoxVisibility is 
                                   ControlToolBoxVisibility.FormDesigner or 
                                   ControlToolBoxVisibility.PanelAndFormDesigner)
                .Where(item => implementedScreenWidgets.Contains(item.Name))
                .Select(item => new ToolBoxItem{Name = item.Name, Id = item.Id})
                .OrderBy(x => x.Name);

            ApiControl apiControl = LoadContent(screen.MainItem, []);
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
        if (screenSection.DataEntity == null)
        {
            return [];
        }

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

    public ApiControl LoadContent(ControlSetItem controlSetItem,
        DropDownValue[] dataSourceDropDownValues)
    {
        ApiControl apiControl =
            LoadItem(controlSetItem, dataSourceDropDownValues);

        var childControls = controlSetItem
            .ChildItemsByType<ControlSetItem>("ControlSetItem");
        foreach (var childControl in childControls)
        {
            if (childControl.IsDeleted)
            {
                continue;
            }

            var child = LoadContent(childControl, dataSourceDropDownValues);
            apiControl.Children.Add(child);
        }

        return apiControl;
    }

    private ApiControl LoadItem(ControlSetItem controlSetItem,
        DropDownValue[] dataSourceDropDownValues)
    {
        ControlAdapter.ControlAdapter controlAdapter =
            adapterFactory.Create(controlSetItem);
        ApiControl apiControl = new ApiControl
        {
            Type = controlSetItem.ControlItem.ControlType,
            Id = controlSetItem.Id,
            Properties =
                controlAdapter.GetEditorProperties(dataSourceDropDownValues)
        };

        if (controlSetItem.RootItem is PanelControlSet controlSet)
        {
            var bindingInfo = controlSetItem.ChildItems
                .OfType<PropertyBindingInfo>()
                .FirstOrDefault();
            var caption = controlSet.DataEntity
                ?.ChildItemsByType<IDataEntityColumn>(AbstractDataEntityColumn
                    .CategoryConst)
                ?.FirstOrDefault(x => x.Name == bindingInfo?.Value)
                ?.Caption ?? bindingInfo?.Value;
            apiControl.Name = caption ?? controlSetItem.Name;
        }
        else
        {
            apiControl.Name = controlSetItem.RootItem.Name;
        }

        return apiControl;
    }

    public ApiControl CreateNewItem(SectionEditorItemModel itemModelData,
        PanelControlSet screenSection)
    {
        ISchemaItem parent =
            screenSection.GetChildByIdRecursive(itemModelData
                .ParentControlSetItemId);
        if (parent == null)
        {
            throw new Exception(
                $"Parent object {itemModelData.ParentControlSetItemId} not found");
        }

        ControlItem controlItem = schemaService
            .GetProvider<UserControlSchemaItemProvider>()
            .ChildItems
            .OfType<ControlItem>()
            .First(item => item.ControlType == itemModelData.ComponentType);
        ControlSetItem newItem = parent.NewItem<ControlSetItem>(
            schemaService.ActiveSchemaExtensionId, null);
        newItem.ControlItem = controlItem;
        newItem.Name = itemModelData.FieldName ?? controlItem.Name;

        ControlAdapter.ControlAdapter controlAdapter =
            adapterFactory.Create(newItem);
        controlAdapter.InitializeProperties(
            top: itemModelData.Top,
            left: itemModelData.Left);
        DropDownValue[] dataSourceDropDownValues = GetFields(screenSection)
            .Select(field => new DropDownValue(field.Name, field.Name))
            .ToArray();
        return LoadItem(newItem, dataSourceDropDownValues);
    }

    public ScreenEditorItem CreateNewItem(ScreenEditorItemModel itemModelData,
        FormControlSet screen)
    {
        var (newItem, sectionControl) = LoadControl(itemModelData, screen);
        
        if (itemModelData.ControlItemId == tabControlControlItemId)
        {
            for (int i = 0; i < 2; i++)
            {
                // This will add initial TabPages to the TabControl. They are
                // added to the newItem in side of the LoadControl so the result
                // can be ignored here
                LoadControl(
                    new ScreenEditorItemModel
                    {
                        ControlItemId = tabPageControlItemId,
                        Top = itemModelData.Top,
                        Left = itemModelData.Left,
                        ParentControlSetItemId = newItem.Id,
                    },
                    screen);
            }
        }

        return new ScreenEditorItem
        {
            ScreenItem = LoadContent(newItem, []),
            Section = sectionControl,
        };
    }

    private Tuple<ControlSetItem, ApiControl> LoadControl(
        ScreenEditorItemModel itemModelData, FormControlSet screen)
    {
        ISchemaItem parent =
            screen.GetChildByIdRecursive(itemModelData.ParentControlSetItemId);
        if (parent == null)
        {
            throw new Exception(
                $"Parent object {itemModelData.ParentControlSetItemId} not found");
        }

        ControlItem controlItem = schemaService
            .GetProvider<UserControlSchemaItemProvider>()
            .ChildItems
            .OfType<ControlItem>()
            .First(item =>
                item.Id ==
                itemModelData
                    .ControlItemId); // This will have to be done some other way in case of a plugin. See ControlSetEditor.GetControlbyType(Type type)

        ControlSetItem newItem = parent.NewItem<ControlSetItem>(
            schemaService.ActiveSchemaExtensionId, null);
        newItem.ControlItem = controlItem;
        newItem.Name = controlItem.Name;

        ApiControl sectionControl = null;
        object height = null;
        object width = null;
        if (controlItem.PanelControlSet != null)
        {
            sectionControl =
                LoadContent(controlItem.PanelControlSet.MainItem, []);
            height = sectionControl.Properties
                .Find(prop => prop.Name == "Height").Value;
            width = sectionControl.Properties.Find(prop => prop.Name == "Width")
                .Value;
        }

        ControlAdapter.ControlAdapter controlAdapter =
            adapterFactory.Create(newItem);
        controlAdapter.InitializeProperties(
            top: itemModelData.Top,
            left: itemModelData.Left,
            width: (int?)width,
            height: (int?)height);
        return new Tuple<ControlSetItem, ApiControl>(newItem, sectionControl);
    }

    public void DeleteItem(List<Guid> schemaItemIds, ISchemaItem rootItem)
    {
        foreach (var schemaItemId in schemaItemIds)
        {
            ISchemaItem schemaItem = rootItem
                .GetChildByIdRecursive(schemaItemId);
            if (schemaItem is ControlSetItem itemToUpdate)
            {
                itemToUpdate.IsDeleted = true;
            }
        }
    }

    public Dictionary<Guid, ApiControl> LoadSections(
        FormControlSet formControlSet, Guid[] sectionIds)
    {
        return sectionIds
            .ToDictionary(
                sectionId => sectionId,
                sectionId =>
                {
                    var screenControlSet =
                        (ControlSetItem)formControlSet.GetChildByIdRecursive(
                            sectionId);
                    var screenSection = screenControlSet.ControlItem
                        .PanelControlSet.MainItem;
                    ApiControl sectionControl = LoadContent(screenSection, []);
                    sectionControl.Properties.Find(x => x.Name == "Top").Value =
                        0;
                    sectionControl.Properties.Find(x => x.Name == "Left")
                        .Value = 0;
                    return sectionControl;
                });
    }
}

public class ScreenEditorItem
{
    public ApiControl ScreenItem { get; set; }
    public ApiControl Section { get; set; }
}

public class ApiControl
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public List<EditorProperty> Properties { get; set; }
    public List<ApiControl> Children { get; set; } = new();
}

public class ScreenApiControl
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public ApiControl Section { get; set; }
    public List<EditorProperty> Properties { get; set; }
    public List<ScreenApiControl> Children { get; set; } = new();
}