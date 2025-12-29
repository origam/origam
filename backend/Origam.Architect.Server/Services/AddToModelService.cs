#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using Origam.Architect.Server.Interfaces.Services;
using Origam.DA;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class AddToModelService(
    SchemaService schemaService,
    IPlatformResolveService platformResolveService,
    ISchemaDbCompareResultsService schemaDbCompareResultsService
) : IAddToModelService
{
    public void Process(string platform, List<string> schemaItemNames)
    {
        Platform platformParsed = platformResolveService.Resolve(platform);

        var results = schemaDbCompareResultsService.GetByNames(schemaItemNames, platformParsed);
        var missingInSchemaResults = results.Where(r =>
            r.ResultType == DbCompareResultType.MissingInSchema
        );

        var activeExtensionName = schemaService.ActiveExtension.Name;
        var entityModelProvider = schemaService.GetProvider<EntityModelSchemaItemProvider>();
        SchemaItemGroup targetGroup = entityModelProvider.GetGroup(activeExtensionName);

        foreach (SchemaDbCompareResult result in missingInSchemaResults)
        {
            ISchemaItem schemaItem = result.SchemaItem;
            schemaItem.Group = targetGroup;
            schemaItem.RootProvider.ChildItems.Add(schemaItem);
            schemaItem.Persist();
        }
    }
}
