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
using System.Text;
using Origam.Architect.Server.Models.Responses.Wizards;
using Origam.DA;
using Origam.DA.Service;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Architect.Server.Services.Wizards;

public class DataStructureSqlWizardService(
    IPersistenceService persistenceService,
    ModelTransactionRunner transaction,
    SearchService searchService
) : WizardServiceBase(persistenceService, transaction, searchService)
{
    private const string TempTablePrefix = "tmptable";

    public GetDataStructureSqlResult GetDataStructureSql(Guid dataStructureId)
    {
        var dataStructure = Retrieve<DataStructure>(
            dataStructureId,
            Strings.Wizard_DataStructureNotFound
        );

        return new GetDataStructureSqlResult
        {
            DataStructureId = dataStructure.Id,
            DataStructureName = dataStructure.Name,
            Sql = GenerateSql(dataStructure, filterSet: null),
        };
    }

    public GetDataStructureFilterSetSqlResult GetDataStructureFilterSetSql(Guid filterSetId)
    {
        var filterSet = Retrieve<DataStructureFilterSet>(
            filterSetId,
            Strings.Wizard_DataStructureFilterSetNotFound
        );
        var dataStructure = (DataStructure)filterSet.RootItem;

        return new GetDataStructureFilterSetSqlResult
        {
            FilterSetId = filterSet.Id,
            FilterSetName = filterSet.Name,
            DataStructureName = dataStructure.Name,
            Sql = GenerateSql(dataStructure, filterSet),
        };
    }

    private static string GenerateSql(DataStructure dataStructure, DataStructureFilterSet filterSet)
    {
        if (DataServiceFactory.GetDataService() is not AbstractSqlDataService dataService)
        {
            throw new UserOrigamException(Strings.Wizard_DataServiceNotSql);
        }

        var sqlGenerator = (AbstractSqlCommandGenerator)dataService.DbDataAdapterFactory.Clone();
        sqlGenerator.PrettyFormat = true;
        sqlGenerator.GenerateConsoleUseSyntax = true;

        var output = new StringBuilder();
        output.AppendLine($"-- SQL statements for data structure: {dataStructure.Name}");
        if (filterSet != null)
        {
            output.AppendLine(
                sqlGenerator.SelectParameterDeclarationsSql(
                    filter: filterSet,
                    paging: false,
                    columnName: null
                )
            );
        }
        var tmpTables = new List<string>();
        foreach (var dsEntity in dataStructure.Entities.Where(entity => entity.Columns.Count > 0))
        {
            string tmpTable = $"{TempTablePrefix}{Guid.NewGuid()}";
            tmpTables.Add(tmpTable);
            output.AppendLine(sqlGenerator.CreateOutputTableSql(tmpTable));
            output.AppendLine("-----------------------------------------------------------------");
            output.AppendLine($"-- {dsEntity.Name}");
            output.AppendLine("-----------------------------------------------------------------");
            output.Append(
                sqlGenerator.SelectSql(
                    ds: dataStructure,
                    entity: dsEntity,
                    filter: filterSet,
                    sortSet: null,
                    columnsInfo: ColumnsInfo.Empty,
                    parameters: new Hashtable(),
                    selectParameterReferences: null,
                    paging: false
                )
            );
            output.AppendLine(";");
        }
        output.AppendLine(sqlGenerator.CreateDataStructureFooterSql(tmpTables));
        return output.ToString();
    }
}
