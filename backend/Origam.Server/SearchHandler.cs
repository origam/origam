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
