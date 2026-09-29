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
using Origam.Schema.GuiModel;
using static Origam.Architect.Server.Services.SectionEditor.ScreenSectionBindings;

namespace Origam.Architect.Server.Services.ScreenEditor;

public class ScreenLayoutArranger(ControlAdapterFactory adapterFactory)
{
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

    public void ArrangeChildren(ControlSetItem container, bool splitEvenly)
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
}
