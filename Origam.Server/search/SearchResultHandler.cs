#region license
/*
Copyright 2005 - 2017 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

using System.Web;
using System.Web.SessionState;
using Origam.Schema.MenuModel;
using log4net;
using Origam.Workbench.Services;
using core = Origam.Workbench.Services.CoreServices;
using System;
using System.Data;
using System.Reflection;
using System.Linq;

namespace Origam.Server.Search
{
    class SearchResultHandler : IHttpHandler, IRequiresSessionState
    {
        private static readonly ILog perfLog 
            = LogManager.GetLogger("Performance");
        private static readonly ILog log 
            = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const char tab = (char)9;

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                if (perfLog.IsInfoEnabled)
                {
                    perfLog.Info("Search");
                }
                SearchRequest searchRequest = RetrieveSearchRequest(context);
                if (searchRequest == null)
                {
                    return;
                }
                GetSearchSchemaItemProvider().ChildItems.
                    OfType<SearchDataSource>().ToList().ForEach(
                    dataSource => AttachResultsToResponse(
                        context, dataSource, searchRequest));
                context.Response.Write((char)10);
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error("Search failed.", ex);
                }
                context.Response.Write(
                    Properties.Resources.SearchError
                    + tab + tab + Properties.Resources.ContactAdmin
                    + tab + tab + tab + tab + (char)10);
                context.Response.Write((char)10);
            }
            finally
            {
                context.Response.End();
            }
        }

        private void AttachResultsToResponse(
            HttpContext context, SearchDataSource dataSource, 
            SearchRequest searchRequest)
        {
            var results = core.DataService.LoadData(
                dataSource.DataStructureId, dataSource.DataStructureMethodId, 
                Guid.Empty, Guid.Empty, null, dataSource.FilterParameter, 
                searchRequest.SearchString);
            DataTable resultTable = results.Tables[0];
            bool containsDescription 
                = resultTable.Columns.Contains("Description");
            foreach (DataRow result in resultTable.Rows)
            {
                string name = (string)result["Name"];
                string description = "";
                if (containsDescription)
                {
                    description = (string)result["Description"];
                }
                context.Response.Write(
                    dataSource.GroupLabel + tab
                    + dataSource.Id + tab +
                    result["Name"] + tab +
                    description + tab
                    +  "" + tab // url
                    + dataSource.LookupId + tab
                    + result["ReferenceId"].ToString() + (char)10);
            }
            context.Response.Flush();
        }
        
        private SearchRequest RetrieveSearchRequest(HttpContext context)
        {
            string requestId = context.Request.Params.Get("id");
            SearchRequest searchRequest
                = (SearchRequest)context.Application[requestId];
            context.Application[requestId] = null;
            return searchRequest;
        }

        private SearchSchemaItemProvider GetSearchSchemaItemProvider()
        {
            SchemaService schemaService = ServiceManager.Services.GetService(
                    typeof(SchemaService)) as SchemaService;
            return schemaService.GetProvider(typeof(SearchSchemaItemProvider)) 
                    as SearchSchemaItemProvider;
        }
    }
}
