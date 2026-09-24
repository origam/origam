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
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services.Wizards;

public class DataStructureWizardService(
    IPersistenceService persistenceService,
    ModelTransactionRunner transaction,
    SearchService searchService
) : WizardServiceBase(persistenceService, transaction, searchService)
{
    public DataStructureWizardData GetWizardData(Guid entityId)
    {
        var entity = RetrieveEntity(entityId);

        return new DataStructureWizardData
        {
            EntityName = entity.Name,
            ExistingDataStructureNames = GetDataStructureNames(),
        };
    }

    public CreateWizardResult CreateDataStructure(CreateDataStructureModel input)
    {
        var trimmedName = RequireName(input.Name, Strings.Wizard_DataStructureNameRequired);
        var entity = RetrieveEntity(input.EntityId);
        RequireUniqueDataStructureName(trimmedName);

        var dataStructure = Transaction.Run(() =>
            EntityHelper.CreateDataStructure(entity, trimmedName, persist: true)
        );

        return BuildResult([dataStructure]);
    }
}
