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

using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using static Origam.Architect.Server.Services.SectionEditor.ScreenSectionBindings;

namespace Origam.Architect.Server.Services.SectionEditor;

public static class ScreenSectionWarningFinder
{
    public static List<string> FindWarnings(
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
            if (dataStructure == null)
            {
                continue;
            }
            string screenNames = string.Join(
                separator: ", ",
                screens.Select(screen => screen.Name)
            );
            DataStructureEntity dataStructureEntity = dataStructure.Entities.FirstOrDefault(
                candidate => candidate.EntityDefinition?.Id == entity.Id
            );
            if (dataStructureEntity == null)
            {
                warnings.Add(
                    string.Format(
                        Strings.SectionEditor_EntityMissingInDataStructure,
                        entity.Name,
                        dataStructure.Name,
                        screenNames
                    )
                );
                continue;
            }
            foreach (IDataEntityColumn field in fields)
            {
                warnings.AddRange(
                    FindFieldWarnings(field, dataStructure, dataStructureEntity, screenNames)
                );
            }
        }
        return warnings;
    }

    private static IEnumerable<string> FindFieldWarnings(
        IDataEntityColumn field,
        DataStructure dataStructure,
        DataStructureEntity dataStructureEntity,
        string screenNames
    )
    {
        if (!dataStructureEntity.ExistsEntityFieldAsColumn(field))
        {
            yield return string.Format(
                Strings.SectionEditor_FieldMissingInDataStructure,
                field.Name,
                dataStructure.Name,
                screenNames,
                dataStructureEntity.Id,
                field.Id
            );
        }
        if (
            field is DetachedField { DataType: OrigamDataType.Array } arrayField
            && arrayField.ArrayRelation != null
            && !HasRelationEntity(dataStructureEntity, arrayField.ArrayRelationId)
        )
        {
            yield return string.Format(
                Strings.SectionEditor_ArrayRelationMissingInDataStructure,
                field.Name,
                arrayField.ArrayRelation.Name,
                dataStructure.Name,
                screenNames,
                dataStructureEntity.Id,
                arrayField.ArrayRelationId
            );
        }
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
}
