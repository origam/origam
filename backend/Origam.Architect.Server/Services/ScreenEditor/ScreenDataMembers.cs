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

using Origam.Architect.Server.ReturnModels;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Services.ScreenEditor;

public static class ScreenDataMembers
{
    public static IEnumerable<string> GetDataMembers(FormControlSet screen)
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

    public static void AddDropDown(List<EditorProperty> properties, FormControlSet screen)
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
}
