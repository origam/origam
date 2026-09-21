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
using System.Xml;
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
    ControlAdapterFactory adapterFactory,
    PanelControlFactory panelControlFactory
)
{
    private readonly Guid tabControlControlItemId = new("2e39362b-80a6-4430-a9bd-b3013583a2fe");
    private readonly Guid tabPageControlItemId = new("6d13ec20-3b17-456e-ae43-3021cb067a70");
    private readonly List<string> implementedScreenWidgets =
    [
        "AsTree",
        "Label",
        "Panel",
        "SplitPanel",
        "TabControl",
    ];
    private readonly List<string> screenContainers = ["AsForm", "Panel", "SplitPanel", "TabPage"];
    private readonly List<string> treeColumnProperties =
    [
        "IDColumn",
        "ParentIDColumn",
        "NameColumn",
    ];
    private readonly List<string> layoutProperties =
    [
        "Height",
        "Left",
        "Orientation",
        "TabIndex",
        "Top",
        "Width",
    ];
    private const int PanelGrowMargin = 20;
    private const int SplitPanelGap = 10;
    private const int MinSplitChildSize = 20;
    private const int TabPageOffsetLeft = 5;
    private const int TabPageOffsetTop = 20;

    public bool Update(AbstractControlSet screenSection, SectionEditorChangesModel input)
    {
        bool editorIsDirty = false;
        if (input.Name != null && screenSection.Name != input.Name)
        {
            screenSection.Name = input.Name;
            editorIsDirty = true;
        }

        if (
            input.SelectedDataSourceId is { } selectedDataSourceId
            && screenSection.DataSourceId != selectedDataSourceId
        )
        {
            screenSection.DataSourceId = selectedDataSourceId;
            editorIsDirty = true;
        }

        foreach (var changes in input.ModelChanges)
        {
            ControlSetItem itemToUpdate =
                screenSection.GetChildByIdRecursive(changes.SchemaItemId) as ControlSetItem;
            if (itemToUpdate == null)
            {
                throw new Exception(
                    $"Child with id: {changes.SchemaItemId} not found in {screenSection.Id}"
                );
            }

            if (
                changes.ParentSchemaItemId is { } newParentId
                && itemToUpdate.Id != screenSection.MainItem.Id
                && itemToUpdate.ParentItemId != newParentId
            )
            {
                ISchemaItem newParent = screenSection.GetChildByIdRecursive(newParentId);
                itemToUpdate.ParentItem.ChildItems.Remove(itemToUpdate);
                newParent.ChildItems.Add(itemToUpdate);
            }

            ControlAdapter.ControlAdapter controlAdapter = adapterFactory.Create(itemToUpdate);
            editorIsDirty |= controlAdapter.UpdateProperties(changes);
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
            var dataSources = entityProvider
                .ChildItems.Select(x => new DataSource { Name = x.Name, SchemaItemId = x.Id })
                .OrderBy(x => x.Name)
                .ToList();
            dataSources.Insert(index: 0, DataSource.Empty);

            List<EditorField> fields = GetFields(screenSection);
            DropDownValue[] dataSourceDropDownValues = fields
                .Select(field => new DropDownValue(field.Name, field.Name))
                .Prepend(new DropDownValue(string.Empty, string.Empty))
                .ToArray();
            ApiControl apiControl = LoadContent(screenSection.MainItem, dataSourceDropDownValues);
            return new SectionEditorData
            {
                Name = editedItem.Name,
                SchemaExtensionId = editedItem.SchemaExtensionId,
                DataSources = dataSources,
                RootControl = apiControl,
                SelectedDataSourceId = screenSection.DataEntity?.Id ?? Guid.Empty,
                Fields = GetFields(screenSection),
                Warnings = FindDataStructureWarnings(screenSection),
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
                throw new UserOrigamException($"No package is active. Select a package first.");
            }

            var dataSources = dataStructureProvider
                .ChildItems.Select(x => new DataSource { Name = x.Name, SchemaItemId = x.Id })
                .OrderBy(x => x.Name)
                .ToList();
            dataSources.Insert(index: 0, DataSource.Empty);

            var sections = GetControlItems()
                .Where(IsScreenSection)
                .Select(item => new ToolBoxItem { Name = item.Name, Id = item.Id })
                .OrderBy(x => x.Name);

            var widgets = GetControlItems()
                .Where(IsScreenWidget)
                .Select(item => new ToolBoxItem { Name = item.Name, Id = item.Id })
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
                Widgets = widgets,
                DataMembers = GetDataMembers(screen).ToList(),
                Warnings = FindScreenWarnings(screen),
            };
        }

        return null;
    }

    private IEnumerable<ControlItem> GetControlItems()
    {
        return schemaService
            .GetProvider<UserControlSchemaItemProvider>()
            .ChildItems.OfType<ControlItem>()
            .Where(item => item.ControlType != "Origam.Gui.Win.AsForm");
    }

    private static bool IsScreenSection(ControlItem item)
    {
        return item.IsComplexType
            && item.ControlToolBoxVisibility != ControlToolBoxVisibility.Nowhere;
    }

    private bool IsScreenWidget(ControlItem item)
    {
        return !item.IsComplexType
            && item.ControlToolBoxVisibility
                is ControlToolBoxVisibility.FormDesigner
                    or ControlToolBoxVisibility.PanelAndFormDesigner
            && (implementedScreenWidgets.Contains(item.Name) || IsPlugin(item));
    }

    private static List<EditorField> GetFields(PanelControlSet screenSection)
    {
        IDataEntity dataEntity = screenSection.DataEntity;
        if (screenSection.DataEntity == null)
        {
            return [];
        }

        return dataEntity
            .ChildItemsByType<IDataEntityColumn>(AbstractDataEntityColumn.CategoryConst)
            .OrderBy(field => field.Name)
            .Select(field => new EditorField { Name = field.Name, Type = field.DataType })
            .ToList();
    }

    public ApiControl LoadContent(
        ControlSetItem controlSetItem,
        DropDownValue[] dataSourceDropDownValues
    )
    {
        ApiControl apiControl = LoadItem(controlSetItem, dataSourceDropDownValues);

        var childControls = controlSetItem.ChildItemsByType<ControlSetItem>("ControlSetItem");
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

    private ApiControl LoadItem(
        ControlSetItem controlSetItem,
        DropDownValue[] dataSourceDropDownValues
    )
    {
        ControlAdapter.ControlAdapter controlAdapter = adapterFactory.Create(controlSetItem);
        ApiControl apiControl = new ApiControl
        {
            Type = controlSetItem.ControlItem.ControlType,
            Id = controlSetItem.Id,
            BoundField = BoundFieldName(controlSetItem),
            Properties = controlAdapter.GetEditorProperties(dataSourceDropDownValues),
        };

        if (controlSetItem.RootItem is PanelControlSet controlSet)
        {
            var caption = apiControl
                .Properties.FirstOrDefault(x => x.Name == "Caption")
                ?.Value?.ToString();
            if (string.IsNullOrEmpty(caption))
            {
                var bindingInfo = controlSetItem
                    .ChildItems.OfType<PropertyBindingInfo>()
                    .FirstOrDefault();
                caption =
                    controlSet
                        .DataEntity?.ChildItemsByType<IDataEntityColumn>(
                            AbstractDataEntityColumn.CategoryConst
                        )
                        ?.FirstOrDefault(x => x.Name == bindingInfo?.Value)
                        ?.Caption
                    ?? bindingInfo?.Value;
            }
            apiControl.Name = caption;
        }
        else
        {
            apiControl.Name = controlSetItem.RootItem.Name;
            if (controlSetItem.RootItem is FormControlSet screen)
            {
                AddDataMemberDropDown(apiControl.Properties, screen);
            }
        }

        return apiControl;
    }

    private static bool IsPlugin(ControlItem controlItem)
    {
        return controlItem.ControlType
            is "Origam.Gui.Win.ScreenLevelPlugin"
                or "Origam.Gui.Win.SectionLevelPlugin";
    }

    private static void AddDataMemberDropDown(
        List<EditorProperty> properties,
        FormControlSet screen
    )
    {
        int index = properties.FindIndex(property => property.Name == "DataMember");
        if (index < 0)
        {
            return;
        }

        EditorProperty dataMember = properties[index];
        string currentValue = dataMember.Value as string ?? string.Empty;
        DropDownValue[] dropDownValues = GetDataMembers(screen)
            .Append(currentValue)
            .Prepend(string.Empty)
            .Distinct()
            .Select(path => new DropDownValue(path, path))
            .ToArray();
        properties[index] = new EditorProperty(
            name: dataMember.Name,
            controlPropertyId: dataMember.ControlPropertyId,
            type: "looukup",
            value: dataMember.Value,
            dropDownValues: dropDownValues,
            category: dataMember.Category,
            description: dataMember.Description,
            readOnly: dataMember.ReadOnly
        );
    }

    private static IEnumerable<string> GetDataMembers(FormControlSet screen)
    {
        if (screen.DataStructure == null)
        {
            return [];
        }

        return screen
            .DataStructure.ChildItemsByType<DataStructureEntity>(DataStructureEntity.CategoryConst)
            .SelectMany(entity => GetDataMembers(entity, parentPath: null));
    }

    private static IEnumerable<string> GetDataMembers(DataStructureEntity entity, string parentPath)
    {
        string path = parentPath == null ? entity.Name : parentPath + "." + entity.Name;
        return entity
            .ChildItemsByType<DataStructureEntity>(DataStructureEntity.CategoryConst)
            .SelectMany(child => GetDataMembers(child, path))
            .Prepend(path);
    }

    public ApiControl CreateNewItem(
        SectionEditorItemModel itemModelData,
        PanelControlSet screenSection
    )
    {
        ISchemaItem parent =
            itemModelData.ParentControlSetItemId == Guid.Empty
            || itemModelData.ParentControlSetItemId == screenSection.Id
                ? screenSection.MainItem
                : screenSection.GetChildByIdRecursive(itemModelData.ParentControlSetItemId);
        if (parent == null)
        {
            throw new UserOrigamException(
                string.Format(
                    Strings.DesignerEditor_ParentControlNotFound,
                    itemModelData.ParentControlSetItemId
                )
            );
        }

        ControlItem controlItem = FindSectionWidget(itemModelData.ComponentType);
        IDataEntityColumn boundColumn = string.IsNullOrEmpty(itemModelData.FieldName)
            ? null
            : FindBoundColumn(screenSection, itemModelData.FieldName);
        ControlSetItem newItem = parent.NewItem<ControlSetItem>(
            schemaService.ActiveSchemaExtensionId,
            group: null
        );
        newItem.ControlItem = controlItem;
        newItem.Name = boundColumn == null ? controlItem.Name : itemModelData.FieldName;

        ControlAdapter.ControlAdapter controlAdapter = adapterFactory.Create(newItem);

        string caption = null;
        if (boundColumn != null)
        {
            IDataEntity dataEntity = screenSection.DataEntity;
            DataSet dataSet = new DatasetGenerator(userDefinedParameters: false).CreateDataSet(
                dataEntity
            );
            caption = dataSet.Tables[0].Columns[itemModelData.FieldName]?.Caption;

            if (controlAdapter.Control is IAsControl asControl)
            {
                string boundPropertyName = asControl.DefaultBindableProperty;
                ControlPropertyItem propertyItem = FindPropertyItem(newItem, boundPropertyName);
                PropertyBindingInfo propertyBinding = FindOrMakeBindingInfo(newItem, propertyItem);
                propertyBinding.ControlPropertyItem = propertyItem;
                propertyBinding.Name = boundPropertyName;
                propertyBinding.Value = itemModelData.FieldName;
                propertyBinding.DesignDataSetPath =
                    dataSet.Tables[0].TableName + "." + itemModelData.FieldName;
                // The line dataSet.Tables[0].TableName + "." + itemModelData.FieldName was taken from
                // class Origam.Gui.Designer.DesignerHostImpl method TryCreateComponent. It does say there Tables[0]
                // Looks strange, we will have to see how well it works.
            }

            if (
                controlAdapter.Control is ILookupBoundControl lookupControl
                && boundColumn.DefaultLookup != null
            )
            {
                lookupControl.LookupId = (Guid)boundColumn.DefaultLookup.PrimaryKey["Id"];
            }
        }

        controlAdapter.InitializeProperties(top: itemModelData.Top, left: itemModelData.Left);
        PropertyValueItem textValueItem = newItem
            .ChildItemsByType<PropertyValueItem>(PropertyValueItem.CategoryConst)
            .FirstOrDefault(x => x.Name == "Text");
        if (textValueItem != null && caption != null)
        {
            textValueItem.Value = caption;
        }
        if (controlAdapter.Control is Label && string.IsNullOrEmpty(itemModelData.FieldName))
        {
            newItem.Name = GetUniqueControlName(screenSection, newItem);
            if (textValueItem != null)
            {
                textValueItem.Value = newItem.Name;
            }
        }

        GrowRootPanel(screenSection.MainItem, newItem);

        DropDownValue[] dataSourceDropDownValues = GetFields(screenSection)
            .Select(field => new DropDownValue(field.Name, field.Name))
            .Prepend(new DropDownValue(string.Empty, string.Empty))
            .ToArray();
        ApiControl createdControl = LoadItem(newItem, dataSourceDropDownValues);
        if (boundColumn != null)
        {
            createdControl.Warnings = FindDataStructureWarnings(screenSection, [boundColumn]);
        }
        return createdControl;
    }

    public List<string> FindDataStructureWarnings(
        PanelControlSet screenSection,
        IReadOnlyList<IDataEntityColumn> fields = null
    )
    {
        IDataEntity entity = screenSection.DataEntity;
        if (entity == null)
        {
            return [];
        }
        fields ??= GetLiveControls(screenSection)
            .Select(BoundFieldName)
            .Where(fieldName => fieldName != null)
            .Distinct()
            .Select(fieldName => BoundField(screenSection, fieldName))
            .Where(field => field != null)
            .ToList();
        var warnings = new List<string>();
        var screensByDataStructure = ScreensUsing(screenSection)
            .Where(screen => screen.DataSourceId != Guid.Empty)
            .GroupBy(screen => screen.DataSourceId);
        foreach (IGrouping<Guid, FormControlSet> screens in screensByDataStructure)
        {
            DataStructure dataStructure = screens.First().DataStructure;
            DataStructureEntity dataStructureEntity = dataStructure?.Entities.FirstOrDefault(
                candidate => candidate.EntityId == entity.Id
            );
            if (dataStructureEntity == null)
            {
                continue;
            }
            string screenNames = string.Join(
                separator: ", ",
                screens.Select(screen => screen.Name)
            );
            foreach (IDataEntityColumn field in fields)
            {
                if (!dataStructureEntity.ExistsEntityFieldAsColumn(field))
                {
                    warnings.Add(
                        string.Format(
                            Strings.SectionEditor_FieldMissingInDataStructure,
                            field.Name,
                            dataStructure.Name,
                            screenNames,
                            dataStructureEntity.Id,
                            field.Id
                        )
                    );
                }
                if (
                    field is DetachedField { DataType: OrigamDataType.Array } arrayField
                    && arrayField.ArrayRelation != null
                    && !HasRelationEntity(dataStructureEntity, arrayField.ArrayRelationId)
                )
                {
                    warnings.Add(
                        string.Format(
                            Strings.SectionEditor_ArrayRelationMissingInDataStructure,
                            field.Name,
                            arrayField.ArrayRelation.Name,
                            dataStructure.Name,
                            screenNames,
                            dataStructureEntity.Id,
                            arrayField.ArrayRelationId
                        )
                    );
                }
            }
        }
        return warnings;
    }

    private static bool HasRelationEntity(DataStructureEntity dataStructureEntity, Guid relationId)
    {
        return dataStructureEntity
            .ChildItemsByType<DataStructureEntity>(DataStructureEntity.CategoryConst)
            .Any(child => child.EntityId == relationId && child.Columns.Count > 0);
    }

    private static IEnumerable<FormControlSet> ScreensUsing(PanelControlSet screenSection)
    {
        if (!ReferenceIndexManager.Initialized)
        {
            return [];
        }
        return screenSection
            .GetUsage()
            .SelectMany(item => item is ControlItem controlItem ? controlItem.GetUsage() : [item])
            .Select(item => item.RootItem)
            .OfType<FormControlSet>()
            .DistinctBy(screen => screen.Id)
            .ToList();
    }

    private ControlItem FindSectionWidget(string controlType)
    {
        List<ControlItem> controlItems = schemaService
            .GetProvider<UserControlSchemaItemProvider>()
            .ChildItems.OfType<ControlItem>()
            .ToList();
        ControlItem controlItem = controlItems.FirstOrDefault(item =>
            item.ControlType == controlType
        );
        if (controlItem != null)
        {
            return controlItem;
        }
        IEnumerable<string> sectionWidgetTypes = controlItems
            .Where(item =>
                item.ControlToolBoxVisibility
                    is ControlToolBoxVisibility.PanelDesigner
                        or ControlToolBoxVisibility.PanelAndFormDesigner
            )
            .Select(item => item.ControlType)
            .Where(type => type is not ("Origam.Gui.Win.AsForm" or "Origam.Gui.Win.AsPanel"))
            .OrderBy(type => type);
        throw new UserOrigamException(
            string.Format(
                Strings.SectionEditor_UnknownWidgetType,
                controlType,
                string.Join(separator: ", ", sectionWidgetTypes)
            )
        );
    }

    private static IDataEntityColumn FindBoundColumn(
        PanelControlSet screenSection,
        string fieldName
    )
    {
        if (screenSection.DataEntity == null)
        {
            throw new UserOrigamException(Strings.SectionEditor_NoDataSource);
        }
        return BoundField(screenSection, fieldName)
            ?? throw new UserOrigamException(
                string.Format(
                    Strings.SectionEditor_FieldNotFound,
                    fieldName,
                    screenSection.DataEntity.Name,
                    string.Join(
                        separator: ", ",
                        GetFields(screenSection).Select(field => field.Name)
                    )
                )
            );
    }

    private void GrowRootPanel(ControlSetItem rootPanel, ControlSetItem newItem)
    {
        int right = IntValue(newItem, propertyName: "Width");
        int bottom = IntValue(newItem, propertyName: "Height");
        for (
            ISchemaItem item = newItem;
            item is ControlSetItem control && control.Id != rootPanel.Id;
            item = item.ParentItem
        )
        {
            right += IntValue(control, propertyName: "Left");
            bottom += IntValue(control, propertyName: "Top");
        }

        var changes = new List<PropertyChange>();
        if (right > IntValue(rootPanel, propertyName: "Width"))
        {
            changes.Add(
                new PropertyChange
                {
                    Name = "Width",
                    Value = XmlConvert.ToString(right + PanelGrowMargin),
                }
            );
        }
        if (bottom > IntValue(rootPanel, propertyName: "Height"))
        {
            changes.Add(
                new PropertyChange
                {
                    Name = "Height",
                    Value = XmlConvert.ToString(bottom + PanelGrowMargin),
                }
            );
        }
        if (changes.Count > 0)
        {
            adapterFactory
                .Create(rootPanel)
                .UpdateProperties(
                    new ChangesModel { SchemaItemId = rootPanel.Id, Changes = changes }
                );
        }
    }

    private static int IntValue(ControlSetItem item, string propertyName)
    {
        return FindValueItem(item, propertyName)?.IntValue ?? 0;
    }

    private static string GetUniqueControlName(
        PanelControlSet screenSection,
        ControlSetItem newItem
    )
    {
        HashSet<string> usedNames = screenSection
            .ChildItemsRecursive.OfType<ControlSetItem>()
            .Where(item => item != newItem)
            .Select(item => item.Name)
            .ToHashSet();
        int index = 1;
        while (usedNames.Contains(newItem.ControlItem.Name + index))
        {
            index++;
        }
        return newItem.ControlItem.Name + index;
    }

    private PropertyBindingInfo FindOrMakeBindingInfo(
        ControlSetItem controlSetItem,
        ControlPropertyItem propertyToFind
    )
    {
        PropertyBindingInfo result = controlSetItem
            .ChildItemsByType<PropertyBindingInfo>(PropertyBindingInfo.CategoryConst)
            .FirstOrDefault(item =>
                Equals(item.ControlPropertyItem?.PrimaryKey, propertyToFind.PrimaryKey)
            );

        if (result == null)
        {
            result = controlSetItem.NewItem<PropertyBindingInfo>(
                schemaService.ActiveSchemaExtensionId,
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
            .ControlItem.ChildItemsByType<ControlPropertyItem>(ControlPropertyItem.CategoryConst)
            .FirstOrDefault(propItem =>
                string.Equals(
                    propItem.Name,
                    propertyName,
                    StringComparison.CurrentCultureIgnoreCase
                )
            );

        if (propertyItem == null)
        {
            throw new Exception(
                $"Property {propertyName} was not found on ControlItem {controlSetItem.ControlItem.Id}"
            );
        }

        return propertyItem;
    }

    public ScreenEditorItem CreateNewItem(
        ScreenEditorItemModel itemModelData,
        FormControlSet screen,
        bool fitToParent = false
    )
    {
        var (newItem, sectionControl) = LoadControl(itemModelData, screen);

        if (newItem.ControlItem.Id == tabControlControlItemId)
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
                    screen
                );
            }
        }

        if (fitToParent)
        {
            ArrangeChildren((ControlSetItem)newItem.ParentItem, splitEvenly: true);
        }

        return new ScreenEditorItem
        {
            ScreenItem = LoadContent(newItem, []),
            Section = sectionControl,
        };
    }

    private Tuple<ControlSetItem, ApiControl> LoadControl(
        ScreenEditorItemModel itemModelData,
        FormControlSet screen
    )
    {
        ControlSetItem parent =
            itemModelData.ParentControlSetItemId == Guid.Empty
            || itemModelData.ParentControlSetItemId == screen.Id
                ? screen.MainItem
                : screen.GetChildByIdRecursive(itemModelData.ParentControlSetItemId)
                    as ControlSetItem;
        if (parent == null)
        {
            throw new UserOrigamException(
                string.Format(
                    Strings.DesignerEditor_ParentControlNotFound,
                    itemModelData.ParentControlSetItemId
                )
            );
        }

        ControlItem controlItem = FindControlItem(itemModelData);
        ValidateParent(parent, controlItem);

        ControlSetItem newItem = parent.NewItem<ControlSetItem>(
            schemaService.ActiveSchemaExtensionId,
            group: null
        );
        newItem.ControlItem = controlItem;
        newItem.Name = CreateUniqueName(screen, controlItem);

        ApiControl sectionControl = null;
        object height = null;
        object width = null;
        if (controlItem.PanelControlSet != null)
        {
            sectionControl = LoadContent(controlItem.PanelControlSet.MainItem, []);
            height = sectionControl.Properties.Find(prop => prop.Name == "Height").Value;
            width = sectionControl.Properties.Find(prop => prop.Name == "Width").Value;
        }

        ControlAdapter.ControlAdapter controlAdapter = adapterFactory.Create(newItem);
        controlAdapter.InitializeProperties(
            top: itemModelData.Top,
            left: itemModelData.Left,
            height: (int?)height,
            width: (int?)width
        );
        PropertyValueItem tabIndex = newItem.GetPropertyOrNull("TabIndex");
        if (tabIndex != null)
        {
            tabIndex.Value = XmlConvert.ToString(NextTabIndex(parent, newItem));
        }
        return new Tuple<ControlSetItem, ApiControl>(newItem, sectionControl);
    }

    private ControlItem FindControlItem(ScreenEditorItemModel itemModelData)
    {
        if (itemModelData.ControlItemId != Guid.Empty)
        {
            return GetControlItems().First(item => item.Id == itemModelData.ControlItemId); // This will have to be done some other way in case of a plugin. See ControlSetEditor.GetControlbyType(Type type)
        }

        List<ControlItem> matches = GetControlItems()
            .Where(item =>
                IsScreenSection(item) || IsScreenWidget(item) || item.Id == tabPageControlItemId
            )
            .Where(item =>
                string.Equals(
                    item.Name,
                    itemModelData.ControlName,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            .ToList();
        if (matches.Count != 1)
        {
            throw new UserOrigamException(
                string.Format(
                    Strings.ScreenEditor_WidgetNotFound,
                    itemModelData.ControlName,
                    matches.Count,
                    string.Join(
                        separator: ", ",
                        GetControlItems().Where(IsScreenWidget).Select(item => item.Name)
                    )
                )
            );
        }

        return matches[0];
    }

    private void ValidateParent(ControlSetItem parent, ControlItem controlItem)
    {
        string parentType = parent.ControlItem.Name;
        bool canHold =
            controlItem.Id == tabPageControlItemId
                ? parentType == "TabControl"
                : screenContainers.Contains(parentType);
        if (!canHold)
        {
            throw new UserOrigamException(
                string.Format(
                    Strings.ScreenEditor_ParentCannotHoldWidget,
                    controlItem.Name,
                    parent.Name,
                    parentType
                )
            );
        }

        if (parentType == "SplitPanel" && GetLiveChildren(parent).Count >= 2)
        {
            throw new UserOrigamException(
                string.Format(Strings.ScreenEditor_SplitPanelFull, parent.Name)
            );
        }
    }

    private static List<ControlSetItem> GetLiveChildren(ControlSetItem container)
    {
        return container
            .ChildItemsByType<ControlSetItem>(ControlSetItem.CategoryConst)
            .Where(item => !item.IsDeleted)
            .OrderBy(item => IntValue(item, propertyName: "TabIndex"))
            .ToList();
    }

    public void ArrangeChanged(FormControlSet screen, SectionEditorChangesModel input)
    {
        foreach (ChangesModel changes in input.ModelChanges)
        {
            bool layoutChanged =
                changes.ParentSchemaItemId != null
                || changes.Changes.Any(change => layoutProperties.Contains(change.Name));
            if (
                !layoutChanged
                || screen.GetChildByIdRecursive(changes.SchemaItemId)
                    is not ControlSetItem changedItem
            )
            {
                continue;
            }

            if (changes.Changes.Any(change => change.Name == "Orientation"))
            {
                DockSplitPanelChildren(
                    changedItem,
                    GetLiveChildren(changedItem),
                    splitEvenly: true
                );
            }

            ArrangeChildren(
                changedItem.ParentItem as ControlSetItem ?? changedItem,
                splitEvenly: false
            );
        }
    }

    private void ArrangeChildren(ControlSetItem container, bool splitEvenly)
    {
        List<ControlSetItem> children = GetLiveChildren(container);
        switch (container.ControlItem.Name)
        {
            case "SplitPanel":
            {
                DockSplitPanelChildren(container, children, splitEvenly);
                break;
            }
            case "AsForm":
            {
                FillParent(
                    children,
                    width: IntValue(container, propertyName: "Width"),
                    height: IntValue(container, propertyName: "Height")
                );
                break;
            }
            case "TabPage":
            {
                var tabControl = (ControlSetItem)container.ParentItem;
                FillParent(
                    children,
                    width: IntValue(tabControl, propertyName: "Width") - (TabPageOffsetLeft * 2),
                    height: IntValue(tabControl, propertyName: "Height")
                        - TabPageOffsetTop
                        - TabPageOffsetLeft
                );
                break;
            }
        }

        foreach (ControlSetItem child in children)
        {
            ArrangeChildren(child, splitEvenly: false);
        }
    }

    private void FillParent(List<ControlSetItem> children, int width, int height)
    {
        if (children.Count == 1 && children[0].ControlItem.Name != "Label")
        {
            SetBounds(children[0], top: 0, left: 0, width, height);
        }
    }

    private void DockSplitPanelChildren(
        ControlSetItem splitPanel,
        List<ControlSetItem> children,
        bool splitEvenly
    )
    {
        if (children.Count == 0)
        {
            return;
        }

        bool isHorizontal = FindValueItem(splitPanel, propertyName: "Orientation")?.Value != "1";
        int splitWidth = IntValue(splitPanel, propertyName: "Width");
        int splitHeight = IntValue(splitPanel, propertyName: "Height");
        int innerWidth = splitWidth - (SplitPanelGap * 2);
        int innerHeight = splitHeight - (SplitPanelGap * 2);
        bool halve = splitEvenly && children.Count > 1;
        int firstWidth = isHorizontal
            ? innerWidth
            : LimitFirstChildSize(
                halve
                    ? (innerWidth - SplitPanelGap) / 2
                    : IntValue(children[0], propertyName: "Width"),
                innerWidth
            );
        int firstHeight = isHorizontal
            ? LimitFirstChildSize(
                halve
                    ? (innerHeight - SplitPanelGap) / 2
                    : IntValue(children[0], propertyName: "Height"),
                innerHeight
            )
            : innerHeight;
        SetBounds(children[0], top: SplitPanelGap, left: SplitPanelGap, firstWidth, firstHeight);
        if (children.Count < 2)
        {
            return;
        }

        int secondTop = isHorizontal ? firstHeight + (SplitPanelGap * 2) : SplitPanelGap;
        int secondLeft = isHorizontal ? SplitPanelGap : firstWidth + (SplitPanelGap * 2);
        SetBounds(
            children[1],
            secondTop,
            secondLeft,
            width: isHorizontal
                ? innerWidth
                : Math.Max(MinSplitChildSize, splitWidth - SplitPanelGap - secondLeft),
            height: isHorizontal
                ? Math.Max(MinSplitChildSize, splitHeight - SplitPanelGap - secondTop)
                : innerHeight
        );
    }

    private static int LimitFirstChildSize(int size, int available)
    {
        return Math.Max(
            MinSplitChildSize,
            Math.Min(size, available - SplitPanelGap - MinSplitChildSize)
        );
    }

    private void SetBounds(ControlSetItem item, int top, int left, int width, int height)
    {
        adapterFactory
            .Create(item)
            .UpdateProperties(
                new ChangesModel
                {
                    SchemaItemId = item.Id,
                    Changes =
                    [
                        new PropertyChange { Name = "Top", Value = XmlConvert.ToString(top) },
                        new PropertyChange { Name = "Left", Value = XmlConvert.ToString(left) },
                        new PropertyChange { Name = "Width", Value = XmlConvert.ToString(width) },
                        new PropertyChange { Name = "Height", Value = XmlConvert.ToString(height) },
                    ],
                }
            );
    }

    public List<string> FindScreenWarnings(FormControlSet screen)
    {
        var warnings = new List<string>();
        if (screen.MainItem == null)
        {
            return warnings;
        }

        List<ControlSetItem> rootWidgets = GetLiveChildren(screen.MainItem);
        if (rootWidgets.Count > 1)
        {
            warnings.Add(
                string.Format(
                    Strings.ScreenEditor_MoreRootWidgets,
                    string.Join(separator: ", ", rootWidgets.Select(item => item.Name))
                )
            );
        }

        List<string> dataMembers = GetDataMembers(screen).ToList();
        foreach (ControlSetItem item in GetLiveControls(screen))
        {
            PropertyValueItem dataMember = FindValueItem(item, propertyName: "DataMember");
            bool needsDataMember =
                item.ControlItem.IsComplexType
                || item.ControlItem.Name == "AsTree"
                || item.ControlItem.ControlType == "Origam.Gui.Win.SectionLevelPlugin";
            if (needsDataMember && !dataMembers.Contains(dataMember?.Value))
            {
                warnings.Add(
                    string.Format(
                        Strings.ScreenEditor_DataMemberMissing,
                        item.Name,
                        dataMember?.Value,
                        string.Join(separator: ", ", dataMembers)
                    )
                );
            }

            if (item.ControlItem.Name == "AsTree")
            {
                warnings.AddRange(
                    treeColumnProperties
                        .Where(property =>
                            string.IsNullOrEmpty(FindValueItem(item, property)?.Value)
                        )
                        .Select(property =>
                            string.Format(
                                Strings.ScreenEditor_TreeColumnMissing,
                                item.Name,
                                property
                            )
                        )
                );
            }

            if (item.ControlItem.Name == "SplitPanel" && GetLiveChildren(item).Count != 2)
            {
                warnings.Add(string.Format(Strings.ScreenEditor_SplitPanelNeedsTwo, item.Name));
            }
        }

        return warnings;
    }

    private static string CreateUniqueName(FormControlSet screen, ControlItem controlItem)
    {
        string baseName = controlItem.IsComplexType ? "AsPanel" : controlItem.Name;
        HashSet<string> usedNames = screen
            .ChildItemsRecursive.OfType<ControlSetItem>()
            .Where(item => !item.IsDeleted)
            .Select(item => item.Name)
            .ToHashSet();
        if (!controlItem.IsComplexType && !usedNames.Contains(baseName))
        {
            return baseName;
        }

        int number = 1;
        while (usedNames.Contains(baseName + number))
        {
            number++;
        }

        return baseName + number;
    }

    private static int NextTabIndex(ISchemaItem parent, ControlSetItem newItem)
    {
        return parent
                .ChildItemsByType<ControlSetItem>(ControlSetItem.CategoryConst)
                .Where(item => item.Id != newItem.Id && !item.IsDeleted)
                .Select(item => item.GetPropertyOrNull("TabIndex")?.IntValue ?? -1)
                .DefaultIfEmpty(-1)
                .Max() + 1;
    }

    public void DeleteItem(List<Guid> schemaItemIds, ISchemaItem rootItem)
    {
        foreach (var schemaItemId in schemaItemIds)
        {
            ISchemaItem schemaItem = rootItem.GetChildByIdRecursive(schemaItemId);
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
            sectionId => sectionId,
            sectionId =>
            {
                var screenControlSet = (ControlSetItem)
                    formControlSet.GetChildByIdRecursive(sectionId);
                var screenSection = screenControlSet.ControlItem.PanelControlSet.MainItem;
                ApiControl sectionControl = LoadContent(screenSection, []);
                sectionControl.Properties.Find(x => x.Name == "Top").Value = 0;
                sectionControl.Properties.Find(x => x.Name == "Left").Value = 0;
                return sectionControl;
            }
        );
    }

    public bool SaveScreenSection(PanelControlSet screenSection)
    {
        ValidateForRuntime(screenSection);
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
                items.Add(screenSection);
                documentationService.CloneDocumentation(items);
            }

            screenSection.OldPrimaryKey = null;
            if (createWidget)
            {
                panelControlFactory.Create(screenSection, schemaService.ActiveSchemaExtensionId);
                return true;
            }
        }
        finally
        {
            persistenceService.SchemaListProvider.EndTransaction();
        }

        return false;
    }

    private static void ValidateForRuntime(PanelControlSet screenSection)
    {
        foreach (ControlSetItem item in GetLiveControls(screenSection))
        {
            string controlName = item.ControlItem.Name;
            if (
                controlName == "RadioButton"
                && IsBoundToField(item)
                && (FindValueItem(item, propertyName: "DataConstantId")?.GuidValue ?? Guid.Empty)
                    == Guid.Empty
            )
            {
                throw new UserOrigamException(
                    string.Format(Strings.SectionEditor_RadioButtonValueConstantMissing, item.Name)
                );
            }
            if (
                controlName == GuiHelper.CONTROL_NAME_MULTICOLUMNADAPTERFIELD
                && IsBoundToField(item)
                && !item.ChildItemsByType<ControlSetItem>(ControlSetItem.CategoryConst)
                    .Any(child => !child.IsDeleted && RendersAsProperty(child))
            )
            {
                throw new UserOrigamException(
                    string.Format(Strings.SectionEditor_WrapperHasNoBoundWidgets, item.Name)
                );
            }
            if (
                controlName is GuiHelper.CONTROL_NAME_COMBOBOX or "TagInput" or "Checklist"
                && IsBoundToField(item)
                && (FindValueItem(item, propertyName: "LookupId")?.GuidValue ?? Guid.Empty)
                    == Guid.Empty
            )
            {
                throw new UserOrigamException(
                    string.Format(Strings.SectionEditor_DropdownLookupMissing, item.Name)
                );
            }
            if (controlName is "TagInput" or "Checklist" && IsBoundToField(item))
            {
                string fieldName = BoundFieldName(item);
                IDataEntityColumn field = BoundField(screenSection, fieldName);
                if (field != null && field.DataType != OrigamDataType.Array)
                {
                    throw new UserOrigamException(
                        string.Format(
                            Strings.SectionEditor_TagInputFieldNotArray,
                            item.Name,
                            fieldName,
                            field.DataType
                        )
                    );
                }
            }
            if (controlName == "ColorPicker" && IsBoundToField(item))
            {
                string fieldName = BoundFieldName(item);
                IDataEntityColumn field = BoundField(screenSection, fieldName);
                if (field != null && field.DataType != OrigamDataType.Integer)
                {
                    throw new UserOrigamException(
                        string.Format(
                            Strings.SectionEditor_ColorPickerFieldNotInteger,
                            item.Name,
                            fieldName
                        )
                    );
                }
            }
        }
        ValidateFieldsBoundOnce(screenSection);
    }

    private static void ValidateFieldsBoundOnce(PanelControlSet screenSection)
    {
        var fieldBoundTwice = GetLiveControls(screenSection)
            .Where(item => IsBoundToField(item) && item.ControlItem.Name != "RadioButton")
            .GroupBy(BoundFieldName)
            .FirstOrDefault(group => group.Count() > 1);
        if (fieldBoundTwice == null)
        {
            return;
        }
        throw new UserOrigamException(
            string.Format(
                Strings.SectionEditor_FieldBoundTwice,
                fieldBoundTwice.Key,
                string.Join(separator: ", ", fieldBoundTwice.Select(item => item.Name))
            )
        );
    }

    private static bool RendersAsProperty(ControlSetItem item)
    {
        return IsBoundToField(item)
            && item.ControlItem.Name != "RadioButton"
            && !(FindValueItem(item, propertyName: "HideOnForm")?.BoolValue ?? false);
    }

    private static IEnumerable<ControlSetItem> GetLiveControls(ISchemaItem parent)
    {
        foreach (
            ControlSetItem item in parent.ChildItemsByType<ControlSetItem>(
                ControlSetItem.CategoryConst
            )
        )
        {
            if (item.IsDeleted)
            {
                continue;
            }
            yield return item;
            foreach (ControlSetItem descendant in GetLiveControls(item))
            {
                yield return descendant;
            }
        }
    }

    private static string BoundFieldName(ControlSetItem item)
    {
        return item.ChildItemsByType<PropertyBindingInfo>(PropertyBindingInfo.CategoryConst)
            .FirstOrDefault(binding => !binding.IsDeleted && !string.IsNullOrEmpty(binding.Value))
            ?.Value;
    }

    private static bool IsBoundToField(ControlSetItem item)
    {
        return BoundFieldName(item) != null;
    }

    private static IDataEntityColumn BoundField(PanelControlSet screenSection, string fieldName)
    {
        return screenSection
            .DataEntity?.ChildItemsByType<IDataEntityColumn>(AbstractDataEntityColumn.CategoryConst)
            .FirstOrDefault(column => column.Name == fieldName);
    }

    private static PropertyValueItem FindValueItem(ControlSetItem item, string propertyName)
    {
        return item.ChildItemsByType<PropertyValueItem>(PropertyValueItem.CategoryConst)
            .FirstOrDefault(value => value.ControlPropertyItem.Name == propertyName);
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
    public string BoundField { get; set; }
    public List<string> Warnings { get; set; }
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
