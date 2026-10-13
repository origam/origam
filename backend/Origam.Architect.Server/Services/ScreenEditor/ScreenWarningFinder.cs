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
using static Origam.Architect.Server.Services.SectionEditor.ScreenSectionBindings;

namespace Origam.Architect.Server.Services.ScreenEditor;

public static class ScreenWarningFinder
{
    private static readonly List<string> TreeColumnProperties =
    [
        "IDColumn",
        "ParentIDColumn",
        "NameColumn",
    ];

    public static List<string> FindWarnings(FormControlSet screen)
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

        List<KeyValuePair<string, DataStructureEntity>> dataMemberEntities = ScreenDataMembers
            .GetDataMemberEntities(screen)
            .ToList();
        List<string> dataMembers = dataMemberEntities.Select(member => member.Key).ToList();
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
            else if (
                item.ControlItem.IsComplexType
                && item.ControlItem.PanelControlSet is { DataEntity: { } sectionEntity } section
            )
            {
                DataStructureEntity shownEntity = dataMemberEntities
                    .First(member => member.Key == dataMember.Value)
                    .Value;
                if (shownEntity.EntityDefinition?.Id == sectionEntity.Id)
                {
                    warnings.AddRange(
                        ScreenSectionWarningFinder.FindFieldWarnings(section, shownEntity, screen)
                    );
                }
                else
                {
                    warnings.Add(
                        FindEntityMismatch(
                            screen,
                            item.Name,
                            section,
                            dataMember.Value,
                            shownEntity.EntityDefinition,
                            dataMemberEntities
                        )
                    );
                }
            }

            if (item.ControlItem.Name == "AsTree")
            {
                warnings.AddRange(
                    TreeColumnProperties
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

        warnings.AddRange(FindDataStructureWarnings(screen));
        return warnings.Distinct().ToList();
    }

    private static string FindEntityMismatch(
        FormControlSet screen,
        string widgetName,
        PanelControlSet section,
        string dataMember,
        IDataEntity shownEntity,
        List<KeyValuePair<string, DataStructureEntity>> dataMemberEntities
    )
    {
        IDataEntity sectionEntity = section.DataEntity;
        List<string> fittingDataMembers = dataMemberEntities
            .Where(member => member.Value.EntityDefinition?.Id == sectionEntity.Id)
            .Select(member => member.Key)
            .ToList();
        if (fittingDataMembers.Count == 0)
        {
            string warning = string.Format(
                Strings.ScreenEditor_SectionEntityNotInDataStructure,
                widgetName,
                sectionEntity.Name,
                screen.DataStructure.Name,
                dataMember,
                shownEntity?.Name
            );
            List<string> dataSources = ScreenSectionWarningFinder
                .ScreensUsing(section)
                .Select(other => other.DataStructure)
                .Where(dataStructure =>
                    dataStructure?.Entities.Any(entity =>
                        entity.EntityDefinition?.Id == sectionEntity.Id
                    ) == true
                )
                .Select(dataStructure => dataStructure.Name)
                .Distinct()
                .ToList();
            return dataSources.Count == 0
                ? warning
                : warning
                    + " "
                    + string.Format(
                        Strings.ScreenEditor_SectionDataSourceHint,
                        section.Name,
                        string.Join(separator: ", ", dataSources)
                    );
        }

        return string.Format(
            Strings.ScreenEditor_DataMemberOfOtherEntity,
            widgetName,
            sectionEntity.Name,
            dataMember,
            shownEntity?.Name,
            string.Join(separator: ", ", fittingDataMembers)
        );
    }

    private static IEnumerable<string> FindDataStructureWarnings(FormControlSet screen)
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
