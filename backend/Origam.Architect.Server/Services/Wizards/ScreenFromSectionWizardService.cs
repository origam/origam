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

public class ScreenFromSectionWizardService(
    IPersistenceService persistenceService,
    ModelTransactionRunner transaction,
    SearchService searchService
) : WizardServiceBase(persistenceService, transaction, searchService)
{
    public ScreenFromSectionWizardData GetWizardData(Guid screenSectionId)
    {
        var panel = RetrievePanel(screenSectionId);

        return new ScreenFromSectionWizardData
        {
            SectionName = panel.Name,
            ExistingDataStructureNames = GetDataStructureNames(),
        };
    }

    public CreateWizardResult CreateScreenFromSection(CreateScreenFromSectionModel input)
    {
        var trimmedName = RequireName(input.Name, Strings.Wizard_ScreenNameRequired);
        var panel = RetrievePanel(input.ScreenSectionId);
        RequireUniqueDataStructureName(trimmedName);

        var groupName = panel.Group?.Name;

        var (dataStructure, form) = Transaction.Run(() =>
        {
            var newDataStructure = EntityHelper.CreateDataStructure(
                panel.DataEntity,
                trimmedName,
                persist: true
            );
            var newForm = GuiHelper.CreateForm(newDataStructure, groupName, panel);
            return (newDataStructure, newForm);
        });

        return BuildResult([dataStructure, form]);
    }

    private PanelControlSet RetrievePanel(Guid screenSectionId)
    {
        return Retrieve<PanelControlSet>(screenSectionId, Strings.Wizard_ScreenSectionNotFound);
    }
}
