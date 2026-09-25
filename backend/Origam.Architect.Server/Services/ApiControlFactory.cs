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

using Origam.Architect.Server.ControlAdapter;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services.ScreenEditor;
using Origam.Architect.Server.Services.SectionEditor;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Services;

public class ApiControlFactory(ControlAdapterFactory adapterFactory)
{
    public ApiControl CreateWithChildren(
        ControlSetItem controlSetItem,
        DropDownValue[] dataSourceDropDownValues
    )
    {
        ApiControl apiControl = Create(controlSetItem, dataSourceDropDownValues);

        var childControls = controlSetItem.ChildItemsByType<ControlSetItem>("ControlSetItem");
        foreach (var childControl in childControls)
        {
            if (childControl.IsDeleted)
            {
                continue;
            }

            var child = CreateWithChildren(childControl, dataSourceDropDownValues);
            apiControl.Children.Add(child);
        }

        return apiControl;
    }

    public ApiControl Create(
        ControlSetItem controlSetItem,
        DropDownValue[] dataSourceDropDownValues
    )
    {
        ControlAdapter.ControlAdapter controlAdapter = adapterFactory.Create(controlSetItem);
        ApiControl apiControl = new ApiControl
        {
            Type = controlSetItem.ControlItem.ControlType,
            Id = controlSetItem.Id,
            BoundField = ScreenSectionBindings.BoundFieldName(controlSetItem),
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
                ScreenDataMembers.AddDropDown(apiControl.Properties, screen);
            }
        }

        return apiControl;
    }
}
