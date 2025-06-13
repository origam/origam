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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using Microsoft.Extensions.Logging;
using Origam.Schema.MenuModel;
using Origam.Server.Model.Search;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server;
public class SearchHandler
{
    public IEnumerable<SearchResult> Search(string searchTerm)
    {
        IOrigamAuthorizationProvider authorizationProvider 
            = SecurityManager.GetAuthorizationProvider();
        IPrincipal principal = SecurityManager.CurrentPrincipal;
        return GetSearchSchemaItemProvider().ChildItems
            .OfType<SearchDataSource>()
            .Where(dataSource => authorizationProvider.Authorize(
                principal, dataSource.Roles))
            .ToList()
            .SelectMany(dataSource => AttachResultsToResponse(dataSource, searchTerm));
    }
    
    private IEnumerable<SearchResult> AttachResultsToResponse(SearchDataSource dataSource, string searchTerm)
    {
        var results = DataService.Instance.LoadData(
            dataSource.DataStructureId, dataSource.DataStructureMethodId, 
            Guid.Empty, Guid.Empty, null, dataSource.FilterParameter, 
            searchTerm);
        DataTable resultTable = results.Tables[0];
        bool containsDescription 
            = resultTable.Columns.Contains("Description");
        foreach (DataRow result in resultTable.Rows)
        {
            yield return new SearchResult
            {
                Group = dataSource.GroupLabel,
                DataSourceId = dataSource.Id,
                Label = result["Name"].ToString(),
                Description = containsDescription 
                    ? (string)result["Description"] 
                    : "",
                DataSourceLookupId = dataSource.LookupId,
                ReferenceId = result["ReferenceId"].ToString()
            };
        }
    }
    
    private SearchSchemaItemProvider GetSearchSchemaItemProvider()
    {
        return ServiceManager.Services
            .GetService<SchemaService>()
            .GetProvider<SearchSchemaItemProvider>();
    }
}
