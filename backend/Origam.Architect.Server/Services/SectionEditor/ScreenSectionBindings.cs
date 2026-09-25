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
using Origam.Architect.Server.ReturnModels;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Services.SectionEditor;

public static class ScreenSectionBindings
{
    public static List<EditorField> GetFields(PanelControlSet screenSection)
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

    public static DropDownValue[] GetFieldDropDownValues(PanelControlSet screenSection)
    {
        return GetFields(screenSection)
            .Select(field => new DropDownValue(field.Name, field.Name))
            .Prepend(new DropDownValue(string.Empty, string.Empty))
            .ToArray();
    }

    public static IEnumerable<ControlSetItem> GetLiveControls(ISchemaItem parent)
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

    public static string BoundFieldName(ControlSetItem item)
    {
        return item.ChildItemsByType<PropertyBindingInfo>(PropertyBindingInfo.CategoryConst)
            .FirstOrDefault(binding => !binding.IsDeleted && !string.IsNullOrEmpty(binding.Value))
            ?.Value;
    }

    public static bool IsBoundToField(ControlSetItem item)
    {
        return BoundFieldName(item) != null;
    }

    public static IDataEntityColumn BoundField(PanelControlSet screenSection, string fieldName)
    {
        return screenSection
            .DataEntity?.ChildItemsByType<IDataEntityColumn>(AbstractDataEntityColumn.CategoryConst)
            .FirstOrDefault(column => column.Name == fieldName);
    }

    public static PropertyValueItem FindValueItem(ControlSetItem item, string propertyName)
    {
        return item.ChildItemsByType<PropertyValueItem>(PropertyValueItem.CategoryConst)
            .FirstOrDefault(value => value.ControlPropertyItem.Name == propertyName);
    }

    public static int IntValue(ControlSetItem item, string propertyName)
    {
        return FindValueItem(item, propertyName)?.IntValue ?? 0;
    }
}
