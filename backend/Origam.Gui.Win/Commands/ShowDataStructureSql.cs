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

using System.Collections;
using System.Collections.Generic;
using System.Text;
using Origam.DA.Service;
using Origam.Schema.EntityModel;
using Origam.UI;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Gui.Win.Commands;

public class ShowDataStructureSql : AbstractMenuCommand
{
    public override bool IsEnabled
    {
        get => Owner is DataStructure;
        set => base.IsEnabled = value;
    }

    public override void Run()
    {
        var dataService = DataServiceFactory.GetDataService() 
            as AbstractSqlDataService;
        var sqlGenerator = (AbstractSqlCommandGenerator)dataService
            .DbDataAdapterFactory.Clone();
        sqlGenerator.PrettyFormat = true;
        sqlGenerator.GenerateConsoleUseSyntax = true;
        var dataStructure = Owner as DataStructure;
        var output = new StringBuilder();
        output.AppendLine(
            $"-- SQL statements for data structure: {dataStructure.Name}");
        List<string> tmpTables = new List<string>();
        foreach (var dsEntity in dataStructure.Entities)
        {
            if (dsEntity.Columns.Count <= 0)
            {
                continue;
            }
            string tmpTable = $"tmptable{System.Guid.NewGuid()}";
            tmpTables.Add(tmpTable);
            output.AppendLine(sqlGenerator.CreateOutputTableSql(tmpTable));
            output.AppendLine(
                "-----------------------------------------------------------------");
            output.AppendLine($"-- {dsEntity.Name}");
            output.AppendLine(
                "-----------------------------------------------------------------");
            output.Append(
                sqlGenerator.SelectSql(
                    ds: dataStructure,
                    entity: dsEntity,
                    filter: null,
                    sortSet: null,
                    columnsInfo: DA.ColumnsInfo.Empty,
                    parameters: new Hashtable(),
                    selectParameterReferences: null,
                    paging: false
                )
            );
            output.AppendLine(";");
        }
        output.AppendLine(sqlGenerator.CreateDataStructureFooterSql(tmpTables));
        new ShowSqlConsole(new SqlConsoleParameters(output.ToString())).Run();
    }
}
