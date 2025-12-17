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
using Origam.DA.Service;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Architect.Server.Services;

public class SchemaDbCompareResultsService(IPersistenceService persistenceService)
    : ISchemaDbCompareResultsService
{
    public List<SchemaDbCompareResult> GetByPlatform(Platform platform)
    {
        var daPlatform = (AbstractSqlDataService)DataServiceFactory.GetDataService(platform);
        daPlatform.PersistenceProvider = persistenceService.SchemaProvider;

        var dbCompareResults = daPlatform.CompareSchema(persistenceService.SchemaProvider);
        foreach (SchemaDbCompareResult result in dbCompareResults)
        {
            result.Platform = platform;
        }
        return dbCompareResults;
    }

    public List<SchemaDbCompareResult> GetByIds(List<Guid> schemaItemIds, Platform platform)
    {
        var dbCompareResults = GetByPlatform(platform);

        var selectedResults = dbCompareResults
            .Where(r => r.SchemaItem != null && schemaItemIds.Contains(r.SchemaItem.Id))
            .ToList();

        return selectedResults;
    }

    public List<SchemaDbCompareResult> GetByNames(List<string> schemaItemNames, Platform platform)
    {
        var dbCompareResults = GetByPlatform(platform);

        var selectedResults = dbCompareResults
            .Where(r => r.SchemaItem != null)
            .Where(r => schemaItemNames.Contains(r.SchemaItem.Name))
            .ToList();

        return selectedResults;
    }
}
