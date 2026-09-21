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

using System.Xml;
using Origam.Architect.Server.ControlAdapter;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services.ScreenEditor;
using Origam.Architect.Server.Services.SectionEditor;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;
using static Origam.Architect.Server.Services.SectionEditor.ScreenSectionBindings;

namespace Origam.Architect.Server.Services;

public class DesignerEditorService(
    SchemaService schemaService,
    IPersistenceService persistenceService,
    IDocumentationService documentationService,
    ControlAdapterFactory adapterFactory,
    PanelControlFactory panelControlFactory,
    ApiControlFactory apiControlFactory
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

            ApiControl apiControl = apiControlFactory.CreateWithChildren(
                screenSection.MainItem,
                ScreenSectionBindings.GetFieldDropDownValues(screenSection)
            );
            return new SectionEditorData
            {
                Name = editedItem.Name,
                SchemaExtensionId = editedItem.SchemaExtensionId,
                DataSources = dataSources,
                RootControl = apiControl,
                SelectedDataSourceId = screenSection.DataEntity?.Id ?? Guid.Empty,
                Fields = ScreenSectionBindings.GetFields(screenSection),
                Warnings = ScreenSectionWarningFinder.FindWarnings(screenSection),
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

            ApiControl apiControl = apiControlFactory.CreateWithChildren(screen.MainItem, []);
            return new ScreenEditorData
            {
                Name = editedItem.Name,
                SchemaExtensionId = editedItem.SchemaExtensionId,
                DataSources = dataSources,
                RootControl = apiControl,
                SelectedDataSourceId = screen.DataSourceId,
                Sections = sections,
                Widgets = widgets,
                DataMembers = ScreenDataMembers.GetDataMembers(screen).ToList(),
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

    private static bool IsPlugin(ControlItem controlItem)
    {
        return controlItem.ControlType
            is "Origam.Gui.Win.ScreenLevelPlugin"
                or "Origam.Gui.Win.SectionLevelPlugin";
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
            ScreenItem = apiControlFactory.CreateWithChildren(newItem, []),
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
            sectionControl = apiControlFactory.CreateWithChildren(
                controlItem.PanelControlSet.MainItem,
                []
            );
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

        List<string> dataMembers = ScreenDataMembers.GetDataMembers(screen).ToList();
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
                ApiControl sectionControl = apiControlFactory.CreateWithChildren(screenSection, []);
                sectionControl.Properties.Find(x => x.Name == "Top").Value = 0;
                sectionControl.Properties.Find(x => x.Name == "Left").Value = 0;
                return sectionControl;
            }
        );
    }

    public bool SaveScreenSection(PanelControlSet screenSection)
    {
        ScreenSectionValidator.ValidateForRuntime(screenSection);
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
