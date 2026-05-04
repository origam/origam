#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.UI;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Gui.Win.Commands;

public class ShowDataStructureSortSetSql : AbstractMenuCommand
{
    private WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(SchemaService))
        as WorkbenchSchemaService;
    public override bool IsEnabled
    {
        get { return Owner is DataStructureSortSet; }
        set { base.IsEnabled = value; }
    }

    public override void Run()
    {
        StringBuilder builder = new StringBuilder();
        AbstractSqlDataService abstractSqlDataService =
            DataServiceFactory.GetDataService() as AbstractSqlDataService;
        AbstractSqlCommandGenerator generator = (AbstractSqlCommandGenerator)
            abstractSqlDataService.DbDataAdapterFactory.Clone();
        generator.PrettyFormat = true;
        generator.GenerateConsoleUseSyntax = true;
        bool displayPagingParameters = true;
        DataStructure ds = (Owner as ISchemaItem).RootItem as DataStructure;
        builder.AppendLine(value: "-- SQL statements for data structure: " + ds.Name);
        List<string> tmpTables = new List<string>();
        // parameter declarations
        builder.AppendLine(
            value: generator.SelectParameterDeclarationsSql(
                ds: ds,
                sort: Owner as DataStructureSortSet,
                paging: displayPagingParameters,
                columnName: null
            )
        );
        foreach (DataStructureEntity entity in ds.Entities)
        {
            if (entity.Columns.Count > 0)
            {
                builder.AppendLine(
                    value: "-----------------------------------------------------------------"
                );
                builder.AppendLine(value: "-- " + entity.Name);
                builder.AppendLine(
                    value: "-----------------------------------------------------------------"
                );
                string tmpTable = "tmptable" + System.Guid.NewGuid();
                tmpTables.Add(item: tmpTable);
                builder.AppendLine(value: generator.CreateOutputTableSql(tmpTable: tmpTable));
                builder.AppendLine(
                    value: generator.SelectSql(
                        ds: ds,
                        entity: entity,
                        filter: null,
                        sortSet: Owner as DataStructureSortSet,
                        columnsInfo: DA.ColumnsInfo.Empty,
                        parameters: new Hashtable(),
                        selectParameterReferences: new Hashtable(),
                        paging: displayPagingParameters
                    ) + ";"
                );
            }
        }
        builder.AppendLine(value: generator.CreateDataStructureFooterSql(tmpTables: tmpTables));
        new ShowSqlConsole(owner: new SqlConsoleParameters(command: builder.ToString())).Run();
    }
}
