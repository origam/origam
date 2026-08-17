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

using Origam.Architect.Server.Models.Requests.Wizards;
using Origam.Architect.Server.Models.Responses.Wizards;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services.Wizards;

public class ScreenWizardService(
    IPersistenceService persistenceService,
    ModelTransactionRunner transaction,
    SearchService searchService
) : ScreenWizardServiceBase(persistenceService, transaction, searchService)
{
    public ScreenWizardData GetWizardData(Guid entityId)
    {
        var entity = RetrieveEntity(entityId);

        return new ScreenWizardData
        {
            EntityName = entity.Name,
            Columns = GetScreenColumns(entity),
            ExistingDataStructureNames = GetDataStructureNames(),
        };
    }

    public CreateWizardResult CreateScreen(CreateScreenModel input)
    {
        var trimmedName = RequireName(input.Name, Strings.Wizard_ScreenNameRequired);
        var entity = RetrieveEntity(input.EntityId);
        RequireSelectedFields(input.SelectedFieldIds);
        RequireUniqueDataStructureName(trimmedName);

        var selectedNames = RetrieveControlFieldNames(entity, input.SelectedFieldIds);
        var groupName = entity.Group?.Name;

        var (dataStructure, panel, form) = Transaction.Run(() =>
        {
            var newDataStructure = EntityHelper.CreateDataStructure(
                entity,
                input.Name,
                persist: true
            );
            var newPanel = GuiHelper.CreatePanel(groupName, entity, selectedNames, input.Name);
            RelayoutFields(newPanel);
            var newForm = GuiHelper.CreateForm(newDataStructure, groupName, newPanel);
            return (newDataStructure, newPanel, newForm);
        });

        if (!string.IsNullOrWhiteSpace(input.Caption))
        {
            Transaction.Run(() => SetPanelTitle(panel, input.Caption));
        }

        return BuildResult([dataStructure, panel, form]);
    }
}
