#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;
using static Origam.Architect.Server.Services.SectionEditor.ScreenSectionBindings;

namespace Origam.Architect.Server.Services.ScreenEditor;

public class ScreenEditorService(
    SchemaService schemaService,
    ControlAdapterFactory adapterFactory,
    ApiControlFactory apiControlFactory,
    ScreenToolbox toolbox,
    ScreenAutoLayout autoLayout
)
{
    private readonly List<string> screenContainers = ["AsForm", "Panel", "SplitPanel", "TabPage"];

    public ScreenEditorData GetScreenEditorData(ISchemaItem editedItem)
    {
        if (editedItem is FormControlSet screen)
        {
            var dataStructureProvider =
                schemaService.GetProvider<DataStructureSchemaItemProvider>();
            if (dataStructureProvider == null)
            {
                throw new UserOrigamException(Strings.ScreenEditor_NoActivePackage);
            }

            var dataSources = dataStructureProvider
                .ChildItems.Select(x => new DataSource { Name = x.Name, SchemaItemId = x.Id })
                .OrderBy(x => x.Name)
                .ToList();
            dataSources.Insert(index: 0, DataSource.Empty);

            ApiControl apiControl = apiControlFactory.CreateWithChildren(screen.MainItem, []);
            return new ScreenEditorData
            {
                Name = editedItem.Name,
                SchemaExtensionId = editedItem.SchemaExtensionId,
                DataSources = dataSources,
                RootControl = apiControl,
                SelectedDataSourceId = screen.DataSourceId,
                Sections = toolbox.GetSections(),
                Widgets = toolbox.GetWidgets(),
                DataMembers = ScreenDataMembers.GetDataMembers(screen).ToList(),
                Warnings = ScreenWarningFinder.FindWarnings(screen),
            };
        }

        return null;
    }

    public ScreenEditorItem CreateNewItem(
        ScreenEditorItemModel itemModelData,
        FormControlSet screen,
        bool fitToParent = false
    )
    {
        var (newItem, sectionControl) = LoadControl(itemModelData, screen);

        if (newItem.ControlItem.Id == ScreenToolbox.TabControlId)
        {
            for (int i = 0; i < 2; i++)
            {
                LoadControl(
                    new ScreenEditorItemModel
                    {
                        ControlItemId = ScreenToolbox.TabPageId,
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
            autoLayout.ArrangeChildren((ControlSetItem)newItem.ParentItem, splitEvenly: true);
        }

        return new ScreenEditorItem
        {
            ScreenItem = apiControlFactory.CreateWithChildren(newItem, []),
            Section = sectionControl,
        };
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

        ControlItem controlItem = toolbox.FindControlItem(itemModelData);
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

    private void ValidateParent(ControlSetItem parent, ControlItem controlItem)
    {
        string parentType = parent.ControlItem.Name;
        bool canHold =
            controlItem.Id == ScreenToolbox.TabPageId
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
}
