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

using Origam.Architect.Server.Services.SectionEditor;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Services.ScreenEditor;

public static class DataStructureWarningFinder
{
    public static IEnumerable<string> FindWarnings(FormControlSet screen)
    {
        DataStructure dataStructure = screen.DataStructure;
        if (dataStructure == null)
        {
            return [];
        }

        List<DataStructureEntity> tables = dataStructure
            .Entities.Where(entity =>
                entity.Entity is not IAssociation { IsSelfJoin: true } && entity.Columns.Count > 0
            )
            .ToList();
        IEnumerable<string> duplicateNameWarnings = tables
            .GroupBy(entity => entity.Name)
            .Where(sameNameEntities => sameNameEntities.Count() > 1)
            .Select(sameNameEntities =>
                string.Format(
                    Strings.ScreenEditor_DuplicateEntityName,
                    sameNameEntities.Key,
                    dataStructure.Name,
                    string.Join(separator: ", ", sameNameEntities.Select(entity => entity.Id))
                )
            );
        IEnumerable<string> arrayRelationWarnings = tables.SelectMany(entity =>
            entity
                .Columns.Select(column => column.Field)
                .Where(field => field.DataType == OrigamDataType.Array)
                .SelectMany(field =>
                    ScreenSectionWarningFinder.FindFieldWarnings(
                        field,
                        dataStructure,
                        entity,
                        screen.Name
                    )
                )
        );
        return duplicateNameWarnings.Concat(arrayRelationWarnings);
    }
}
