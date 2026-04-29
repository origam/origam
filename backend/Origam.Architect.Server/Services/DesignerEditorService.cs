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

using System.Data;
using Origam.Architect.Server.ControlAdapter;
using Origam.Architect.Server.Controls;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class DesignerEditorService(
    SchemaService schemaService,
    IPersistenceService persistenceService,
    IDocumentationService documentationService,
    EditorService editorService,
    ControlAdapterFactory adapterFactory
)
{
    private readonly Guid tabControlControlItemId = new(g: "2e39362b-80a6-4430-a9bd-b3013583a2fe");
    private readonly Guid tabPageControlItemId = new(g: "6d13ec20-3b17-456e-ae43-3021cb067a70");
    private readonly List<string> implementedScreenWidgets = ["TabControl", "SplitPanel", "AsTree"];

    public bool Update(AbstractControlSet screenSection, SectionEditorChangesModel input)
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
                screenSection.GetChildByIdRecursive(id: changes.SchemaItemId) as ControlSetItem;
            if (itemToUpdate == null)
            {
                throw new Exception(
                    message: $"Child with id: {changes.SchemaItemId} not found in {screenSection.Id}"
                );
            }

            if (
                itemToUpdate.Id != screenSection.MainItem.Id
                && itemToUpdate.ParentItemId != (changes.ParentSchemaItemId ?? Guid.Empty)
            )
            {
                itemToUpdate.ParentItem.ChildItems.Remove(item: itemToUpdate);
                if (changes.ParentSchemaItemId != null)
                {
                    ISchemaItem newParent = screenSection.GetChildByIdRecursive(
                        id: changes.ParentSchemaItemId.Value
                    );
                    newParent.ChildItems.Add(item: itemToUpdate);
                }
            }

            ControlAdapter.ControlAdapter controlAdapter = adapterFactory.Create(
                controlSetItem: itemToUpdate
            );
            editorIsDirty = controlAdapter.UpdateProperties(changes: changes);
        }

        return editorIsDirty;
    }

    public SectionEditorData GetSectionEditorData(ISchemaItem editedItem)
    {
        if (editedItem is PanelControlSet screenSection)
        {
            var entityProvider =
                schemaService.GetProvider(type: typeof(EntityModelSchemaItemProvider))
                as EntityModelSchemaItemProvider;
            var dataSources = entityProvider
                .ChildItems.Select(selector: x => new DataSource
                {
                    Name = x.Name,
                    SchemaItemId = x.Id,
                })
                .OrderBy(keySelector: x => x.Name)
                .ToList();
            dataSources.Insert(index: 0, item: DataSource.Empty);

            List<EditorField> fields = GetFields(screenSection: screenSection);
            DropDownValue[] dataSourceDropDownValues = fields
                .Select(selector: field => new DropDownValue(Name: field.Name, Value: field.Name))
                .ToArray();
            ApiControl apiControl = LoadContent(
                controlSetItem: screenSection.MainItem,
                dataSourceDropDownValues: dataSourceDropDownValues
            );
            return new SectionEditorData
            {
                Name = editedItem.Name,
                SchemaExtensionId = editedItem.SchemaExtensionId,
                DataSources = dataSources,
                RootControl = apiControl,
                SelectedDataSourceId = screenSection.DataEntity?.Id ?? Guid.Empty,
                Fields = GetFields(screenSection: screenSection),
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
                    message: $"No package is active. Select a package first."
                );
            }

            var dataSources = dataStructureProvider
                .ChildItems.Select(selector: x => new DataSource
                {
                    Name = x.Name,
                    SchemaItemId = x.Id,
                })
                .OrderBy(keySelector: x => x.Name)
                .ToList();
            dataSources.Insert(index: 0, item: DataSource.Empty);

            var userControlProvider = schemaService.GetProvider<UserControlSchemaItemProvider>();

            var sections = userControlProvider
                .ChildItems.OfType<ControlItem>()
                .Where(predicate: item =>
                    item.ControlType != "Origam.Gui.Win.AsForm"
                    && item.IsComplexType
                    && item.ControlToolBoxVisibility != ControlToolBoxVisibility.Nowhere
                )
                .Select(selector: item => new ToolBoxItem { Name = item.Name, Id = item.Id })
                .OrderBy(keySelector: x => x.Name);

            var widgets = userControlProvider
                .ChildItems.OfType<ControlItem>()
                .Where(predicate: item =>
                    item.ControlType != "Origam.Gui.Win.AsForm"
                    && !item.IsComplexType
                    && item.ControlToolBoxVisibility
                        is ControlToolBoxVisibility.FormDesigner
                            or ControlToolBoxVisibility.PanelAndFormDesigner
                )
                .Where(predicate: item => implementedScreenWidgets.Contains(item: item.Name))
                .Select(selector: item => new ToolBoxItem { Name = item.Name, Id = item.Id })
                .OrderBy(keySelector: x => x.Name);

            ApiControl apiControl = LoadContent(
                controlSetItem: screen.MainItem,
                dataSourceDropDownValues: []
            );
            return new ScreenEditorData
            {
                Name = editedItem.Name,
                SchemaExtensionId = editedItem.SchemaExtensionId,
                DataSources = dataSources,
                RootControl = apiControl,
                SelectedDataSourceId = screen.DataSourceId,
                Sections = sections,
                Widgets = widgets,
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
            .ChildItemsByType<IDataEntityColumn>(itemType: AbstractDataEntityColumn.CategoryConst)
            .OrderBy(keySelector: field => field.Name)
            .Select(selector: field => new EditorField { Name = field.Name, Type = field.DataType })
            .ToList();
    }

    public ApiControl LoadContent(
        ControlSetItem controlSetItem,
        DropDownValue[] dataSourceDropDownValues
    )
    {
        ApiControl apiControl = LoadItem(
            controlSetItem: controlSetItem,
            dataSourceDropDownValues: dataSourceDropDownValues
        );

        var childControls = controlSetItem.ChildItemsByType<ControlSetItem>(
            itemType: "ControlSetItem"
        );
        foreach (var childControl in childControls)
        {
            if (childControl.IsDeleted)
            {
                continue;
            }

            var child = LoadContent(
                controlSetItem: childControl,
                dataSourceDropDownValues: dataSourceDropDownValues
            );
            apiControl.Children.Add(item: child);
        }

        return apiControl;
    }

    private ApiControl LoadItem(
        ControlSetItem controlSetItem,
        DropDownValue[] dataSourceDropDownValues
    )
    {
        ControlAdapter.ControlAdapter controlAdapter = adapterFactory.Create(
            controlSetItem: controlSetItem
        );
        ApiControl apiControl = new ApiControl
        {
            Type = controlSetItem.ControlItem.ControlType,
            Id = controlSetItem.Id,
            Properties = controlAdapter.GetEditorProperties(
                dataSourceDropDownValues: dataSourceDropDownValues
            ),
        };

        if (controlSetItem.RootItem is PanelControlSet controlSet)
        {
            var caption = apiControl
                .Properties.FirstOrDefault(predicate: x => x.Name == "Caption")
                ?.Value?.ToString();
            if (string.IsNullOrEmpty(value: caption))
            {
                var bindingInfo = controlSetItem
                    .ChildItems.OfType<PropertyBindingInfo>()
                    .FirstOrDefault();
                caption =
                    controlSet
                        .DataEntity?.ChildItemsByType<IDataEntityColumn>(
                            itemType: AbstractDataEntityColumn.CategoryConst
                        )
                        ?.FirstOrDefault(predicate: x => x.Name == bindingInfo?.Value)
                        ?.Caption ?? bindingInfo?.Value;
            }
            apiControl.Name = caption ?? controlSetItem.Name;
        }
        else
        {
            apiControl.Name = controlSetItem.RootItem.Name;
        }

        return apiControl;
    }

    public ApiControl CreateNewItem(
        SectionEditorItemModel itemModelData,
        PanelControlSet screenSection
    )
    {
        ISchemaItem parent = screenSection.GetChildByIdRecursive(
            id: itemModelData.ParentControlSetItemId
        );
        if (parent == null)
        {
            throw new Exception(
                message: $"Parent object {itemModelData.ParentControlSetItemId} not found"
            );
        }

        ControlItem controlItem = schemaService
            .GetProvider<UserControlSchemaItemProvider>()
            .ChildItems.OfType<ControlItem>()
            .First(predicate: item => item.ControlType == itemModelData.ComponentType);
        ControlSetItem newItem = parent.NewItem<ControlSetItem>(
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        newItem.ControlItem = controlItem;
        newItem.Name = itemModelData.FieldName ?? controlItem.Name;

        ControlAdapter.ControlAdapter controlAdapter = adapterFactory.Create(
            controlSetItem: newItem
        );

        IDataEntity dataEntity = persistenceService.SchemaProvider.RetrieveInstance<IDataEntity>(
            instanceId: screenSection.DataSourceId
        );
        DataSet dataSet = new DatasetGenerator(userDefinedParameters: false).CreateDataSet(
            entity: dataEntity
        );
        string caption = dataSet.Tables[index: 0].Columns[name: itemModelData.FieldName]?.Caption;
        if (controlAdapter.Control is IAsControl asControl)
        {
            string boundPropertyName = asControl.DefaultBindableProperty;
            ControlPropertyItem propertyItem = FindPropertyItem(
                controlSetItem: newItem,
                propertyName: boundPropertyName
            );
            PropertyBindingInfo propertyBinding = FindOrMakeBindingInfo(
                controlSetItem: newItem,
                propertyToFind: propertyItem
            );
            propertyBinding.ControlPropertyItem = propertyItem;
            propertyBinding.Name = boundPropertyName;
            propertyBinding.Value = itemModelData.FieldName;
            propertyBinding.DesignDataSetPath =
                dataSet.Tables[index: 0].TableName + "." + itemModelData.FieldName;
            // The line dataSet.Tables[0].TableName + "." + itemModelData.FieldName was taken from
            // class Origam.Gui.Designer.DesignerHostImpl method TryCreateComponent. It does say there Tables[0]
            // Looks strange, we will have to see how well it works.
        }

        controlAdapter.InitializeProperties(top: itemModelData.Top, left: itemModelData.Left);
        PropertyValueItem textValueItem = newItem
            .ChildItemsByType<PropertyValueItem>(itemType: PropertyValueItem.CategoryConst)
            .FirstOrDefault(predicate: x => x.Name == "Text");
        if (textValueItem != null && caption != null)
        {
            textValueItem.Value = caption;
        }

        DropDownValue[] dataSourceDropDownValues = GetFields(screenSection: screenSection)
            .Select(selector: field => new DropDownValue(Name: field.Name, Value: field.Name))
            .ToArray();
        return LoadItem(
            controlSetItem: newItem,
            dataSourceDropDownValues: dataSourceDropDownValues
        );
    }

    private PropertyBindingInfo FindOrMakeBindingInfo(
        ControlSetItem controlSetItem,
        ControlPropertyItem propertyToFind
    )
    {
        PropertyBindingInfo result = controlSetItem
            .ChildItemsByType<PropertyBindingInfo>(itemType: PropertyBindingInfo.CategoryConst)
            .FirstOrDefault(predicate: item =>
                Equals(objA: item.ControlPropertyItem?.PrimaryKey, objB: propertyToFind.PrimaryKey)
            );

        if (result == null)
        {
            result = controlSetItem.NewItem<PropertyBindingInfo>(
                schemaExtensionId: schemaService.ActiveSchemaExtensionId,
                group: null
            );
            result.ControlPropertyItem = propertyToFind;
            result.Name = propertyToFind.Name;
        }

        return result;
    }

    private ControlPropertyItem FindPropertyItem(ControlSetItem controlSetItem, string propertyName)
    {
        ControlPropertyItem propertyItem = controlSetItem
            .ControlItem.ChildItemsByType<ControlPropertyItem>(
                itemType: ControlPropertyItem.CategoryConst
            )
            .FirstOrDefault(predicate: propItem =>
                string.Equals(
                    a: propItem.Name,
                    b: propertyName,
                    comparisonType: StringComparison.CurrentCultureIgnoreCase
                )
            );

        if (propertyItem == null)
        {
            throw new Exception(
                message: $"Property {propertyName} was not found on ControlItem {controlSetItem.ControlItem.Id}"
            );
        }

        return propertyItem;
    }

    public ScreenEditorItem CreateNewItem(
        ScreenEditorItemModel itemModelData,
        FormControlSet screen
    )
    {
        var (newItem, sectionControl) = LoadControl(itemModelData: itemModelData, screen: screen);

        if (itemModelData.ControlItemId == tabControlControlItemId)
        {
            for (int i = 0; i < 2; i++)
            {
                // This will add initial TabPages to the TabControl. They are
                // added to the newItem in side of the LoadControl so the result
                // can be ignored here
                LoadControl(
                    itemModelData: new ScreenEditorItemModel
                    {
                        ControlItemId = tabPageControlItemId,
                        Top = itemModelData.Top,
                        Left = itemModelData.Left,
                        ParentControlSetItemId = newItem.Id,
                    },
                    screen: screen
                );
            }
        }

        return new ScreenEditorItem
        {
            ScreenItem = LoadContent(controlSetItem: newItem, dataSourceDropDownValues: []),
            Section = sectionControl,
        };
    }

    private Tuple<ControlSetItem, ApiControl> LoadControl(
        ScreenEditorItemModel itemModelData,
        FormControlSet screen
    )
    {
        ISchemaItem parent = screen.GetChildByIdRecursive(id: itemModelData.ParentControlSetItemId);
        if (parent == null)
        {
            throw new Exception(
                message: $"Parent object {itemModelData.ParentControlSetItemId} not found"
            );
        }

        ControlItem controlItem = schemaService
            .GetProvider<UserControlSchemaItemProvider>()
            .ChildItems.OfType<ControlItem>()
            .First(predicate: item => item.Id == itemModelData.ControlItemId); // This will have to be done some other way in case of a plugin. See ControlSetEditor.GetControlbyType(Type type)

        ControlSetItem newItem = parent.NewItem<ControlSetItem>(
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        newItem.ControlItem = controlItem;
        newItem.Name = controlItem.Name;

        ApiControl sectionControl = null;
        object height = null;
        object width = null;
        if (controlItem.PanelControlSet != null)
        {
            sectionControl = LoadContent(
                controlSetItem: controlItem.PanelControlSet.MainItem,
                dataSourceDropDownValues: []
            );
            height = sectionControl.Properties.Find(match: prop => prop.Name == "Height").Value;
            width = sectionControl.Properties.Find(match: prop => prop.Name == "Width").Value;
        }

        ControlAdapter.ControlAdapter controlAdapter = adapterFactory.Create(
            controlSetItem: newItem
        );
        controlAdapter.InitializeProperties(
            top: itemModelData.Top,
            left: itemModelData.Left,
            height: (int?)height,
            width: (int?)width
        );
        return new Tuple<ControlSetItem, ApiControl>(item1: newItem, item2: sectionControl);
    }

    public void DeleteItem(List<Guid> schemaItemIds, ISchemaItem rootItem)
    {
        foreach (var schemaItemId in schemaItemIds)
        {
            ISchemaItem schemaItem = rootItem.GetChildByIdRecursive(id: schemaItemId);
            if (schemaItem is ControlSetItem itemToUpdate)
            {
                itemToUpdate.IsDeleted = true;
            }
        }
    }

    public Dictionary<Guid, ApiControl> LoadSections(
        FormControlSet formControlSet,
        Guid[] sectionIds
    )
    {
        return sectionIds.ToDictionary(
            keySelector: sectionId => sectionId,
            elementSelector: sectionId =>
            {
                var screenControlSet = (ControlSetItem)
                    formControlSet.GetChildByIdRecursive(id: sectionId);
                var screenSection = screenControlSet.ControlItem.PanelControlSet.MainItem;
                ApiControl sectionControl = LoadContent(
                    controlSetItem: screenSection,
                    dataSourceDropDownValues: []
                );
                sectionControl.Properties.Find(match: x => x.Name == "Top").Value = 0;
                sectionControl.Properties.Find(match: x => x.Name == "Left").Value = 0;
                return sectionControl;
            }
        );
    }

    public bool SaveScreenSection(PanelControlSet screenSection)
    {
        var controlSchemaItemProvider = schemaService.GetProvider<UserControlSchemaItemProvider>();
        try
        {
            bool createWidget = !screenSection.IsPersisted;
            persistenceService.SchemaListProvider.BeginTransaction();
            screenSection.ClearCacheOnPersist = false;
            screenSection.Persist();
            // If the controlset was cloned, we clone its documentation, too.
            if (screenSection.OldPrimaryKey != null)
            {
                List<ISchemaItem> items = screenSection.ChildItemsRecursive;
                items.Add(item: screenSection);
                documentationService.CloneDocumentation(clonedSchemaItems: items);
            }

            screenSection.OldPrimaryKey = null;
            if (createWidget)
            {
                ControlItem newControl = controlSchemaItemProvider.NewItem<ControlItem>(
                    schemaExtensionId: schemaService.ActiveSchemaExtensionId,
                    group: null
                );
                newControl.Name = screenSection.Name;
                newControl.IsComplexType = true;
                Type t = typeof(PanelControlSet);
                newControl.ControlType = t.ToString();
                newControl.ControlNamespace = t.Namespace;
                newControl.PanelControlSet = screenSection;
                newControl.ControlToolBoxVisibility = ControlToolBoxVisibility.FormDesigner;
                SchemaItemAncestor ancestor = new SchemaItemAncestor();
                ancestor.SchemaItem = newControl;
                ancestor.Ancestor = editorService.GetControlByType(
                    fullTypeName: "Origam.Gui.Win.AsPanel"
                );
                ancestor.PersistenceProvider = newControl.PersistenceProvider;
                newControl.ThrowEventOnPersist = false;
                newControl.Persist();
                ancestor.Persist();
                newControl.ThrowEventOnPersist = true;
                return true;
            }
        }
        finally
        {
            persistenceService.SchemaListProvider.EndTransaction();
        }

        return false;
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
