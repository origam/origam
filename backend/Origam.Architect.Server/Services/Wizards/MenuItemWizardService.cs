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
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services.Wizards;

public class MenuItemWizardService(
    IPersistenceService persistenceService,
    ModelTransactionRunner transaction,
    SearchService searchService
) : WizardServiceBase(persistenceService, transaction, searchService)
{
    public CreateWizardResult CreateMenuItem(CreateMenuItemModel input)
    {
        var caption = RequireName(input.Caption, Strings.Wizard_MenuCaptionRequired);
        var form = Retrieve<FormControlSet>(input.FormId, Strings.Wizard_FormControlSetNotFound);

        return CreateMenuItemWithSystemRole(
            input.Role,
            role => MenuHelper.CreateMenuItem(caption, role, form)
        );
    }

    public CreateWizardResult CreateWorkflowMenuItem(CreateWorkflowMenuItemModel input)
    {
        var caption = RequireName(input.Caption, Strings.Wizard_MenuCaptionRequired);
        var workflow = Retrieve<IWorkflow>(input.WorkflowId, Strings.Wizard_WorkflowNotFound);

        return CreateMenuItemWithSystemRole(
            input.Role,
            role => MenuHelper.CreateMenuItem(caption, role, workflow)
        );
    }

    private CreateWizardResult CreateMenuItemWithSystemRole(
        string inputRole,
        Func<string, ISchemaItem> createMenuItem
    )
    {
        var role = string.IsNullOrWhiteSpace(inputRole) ? AllRoles : inputRole.Trim();

        var generated = new List<ISchemaItem>();
        _ = Transaction.Run(() =>
        {
            var item = createMenuItem(role);
            generated.Add(item);

            if (role != AllRoles && HasCurrentDeploymentVersion())
            {
                CreateSystemRoleActivities(role, generated);
            }

            return item;
        });

        return BuildResult(generated);
    }
}
