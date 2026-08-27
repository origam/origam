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

using System.Collections;
using Origam.Architect.Server.Models.Responses.Wizards;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services.Wizards;

public abstract class ScreenWizardServiceBase(
    IPersistenceService persistenceService,
    ModelTransactionRunner transaction,
    SearchService searchService
) : WizardServiceBase(persistenceService, transaction, searchService)
{
    private const string PanelTitlePropertyName = "PanelTitle";
    private const string TopPropertyName = "Top";
    private const int FieldTopStart = 36;
    private const int FieldTopStep = 30;

    protected static List<ScreenWizardColumn> GetScreenColumns(IDataEntity entity)
    {
        return entity
            .EntityColumns.Where(column => !string.IsNullOrEmpty(column.ToString()))
            .OrderBy(column => column.Name)
            .Select(column => new ScreenWizardColumn
            {
                Id = column.Id,
                Name = column.Name,
                IsPrimaryKey = column.IsPrimaryKey,
                CanGenerateControl = GuiHelper.CanBuildDefaultControl(column),
            })
            .ToList();
    }

    protected static Hashtable RetrieveControlFieldNames(
        IDataEntity entity,
        IEnumerable<Guid> fieldIds
    )
    {
        var fieldNames = new Hashtable();
        foreach (var column in fieldIds.Select(fieldId => RetrieveColumn(entity, fieldId)))
        {
            if (!GuiHelper.CanBuildDefaultControl(column))
            {
                throw new UserOrigamException(
                    string.Format(Strings.Wizard_FieldCannotGenerateControl, column.Name)
                );
            }
            fieldNames[column.Name] = true;
        }
        return fieldNames;
    }

    protected static void SetPanelTitle(PanelControlSet panel, string caption)
    {
        if (string.IsNullOrWhiteSpace(caption) || panel.ChildItems.Count == 0)
        {
            return;
        }
        var rootControl = panel.ChildItems[0];
        var titleProperty = rootControl
            .ChildItemsByType<PropertyValueItem>(PropertyValueItem.CategoryConst)
            .FirstOrDefault(property =>
                property.ControlPropertyItem?.Name == PanelTitlePropertyName
            );
        if (titleProperty == null)
        {
            return;
        }
        titleProperty.Value = caption.Trim();
        titleProperty.Persist();
    }

    protected static void RelayoutFields(PanelControlSet panel)
    {
        if (panel.ChildItems.Count == 0)
        {
            return;
        }
        var rootControl = panel.ChildItems[0];
        var topProperties = rootControl
            .ChildItemsByType<ControlSetItem>(ControlSetItem.CategoryConst)
            .Select(control =>
                control
                    .ChildItemsByType<PropertyValueItem>(PropertyValueItem.CategoryConst)
                    .FirstOrDefault(property =>
                        property.ControlPropertyItem?.Name == TopPropertyName
                    )
            )
            .Where(topProperty => topProperty != null)
            .OrderBy(topProperty =>
                int.TryParse(topProperty.Value, out var top) ? top : int.MaxValue
            )
            .ToList();

        var nextTop = FieldTopStart;
        foreach (var topProperty in topProperties)
        {
            topProperty.Value = nextTop.ToString();
            topProperty.Persist();
            nextTop += FieldTopStep;
        }
    }
}
