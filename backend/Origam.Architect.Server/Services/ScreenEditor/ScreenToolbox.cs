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

using Origam.Architect.Server.Models;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services.ScreenEditor;

public class ScreenToolbox(SchemaService schemaService)
{
    public static readonly Guid TabControlId = new("2e39362b-80a6-4430-a9bd-b3013583a2fe");
    public static readonly Guid TabPageId = new("6d13ec20-3b17-456e-ae43-3021cb067a70");
    private readonly List<string> implementedScreenWidgets =
    [
        "AsTree",
        "Label",
        "Panel",
        "SplitPanel",
        "TabControl",
    ];

    public IEnumerable<ToolBoxItem> GetSections()
    {
        return GetControlItems()
            .Where(IsScreenSection)
            .Select(item => new ToolBoxItem { Name = item.Name, Id = item.Id })
            .OrderBy(x => x.Name);
    }

    public IEnumerable<ToolBoxItem> GetWidgets()
    {
        return GetControlItems()
            .Where(IsScreenWidget)
            .Select(item => new ToolBoxItem { Name = item.Name, Id = item.Id })
            .OrderBy(x => x.Name);
    }

    public ControlItem FindControlItem(ScreenEditorItemModel itemModelData)
    {
        if (itemModelData.ControlItemId != Guid.Empty)
        {
            return GetControlItems().First(item => item.Id == itemModelData.ControlItemId); // This will have to be done some other way in case of a plugin. See ControlSetEditor.GetControlbyType(Type type)
        }

        List<ControlItem> matches = GetControlItems()
            .Where(item => IsScreenSection(item) || IsScreenWidget(item) || item.Id == TabPageId)
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
}
