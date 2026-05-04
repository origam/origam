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
using Origam.Schema.EntityModel;
using Origam.UI;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Gui.Win.Commands;

public class ShowDataStructureFilterSetSql : AbstractMenuCommand
{
    private WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(SchemaService))
        as WorkbenchSchemaService;
    public override bool IsEnabled
    {
        get { return Owner is DataStructureFilterSet; }
        set { base.IsEnabled = value; }
    }

    public override void Run()
    {
        AbstractSqlDataService abstractSqlDataService =
            DataServiceFactory.GetDataService() as AbstractSqlDataService;
        AbstractSqlCommandGenerator generator = (AbstractSqlCommandGenerator)
            abstractSqlDataService.DbDataAdapterFactory.Clone();
        DataStructureFilterSet filterSet = Owner as DataStructureFilterSet;
        generator.PrettyFormat = true;
        generator.GenerateConsoleUseSyntax = true;
        StringBuilder builder = new StringBuilder();
        DataStructure ds = filterSet.RootItem as DataStructure;
        builder.AppendFormat(
            format: "-- SQL statements for data structure: {0}\r\n",
            arg0: ds.Name
        );
        // parameter declarations
        builder.AppendLine(
            value: generator.SelectParameterDeclarationsSql(
                filter: filterSet,
                paging: false,
                columnName: null
            )
        );
        List<string> tmpTables = new List<string>();
        foreach (DataStructureEntity entity in ds.Entities)
        {
            if (entity.Columns.Count > 0)
            {
                string tmpTable = "tmptable" + System.Guid.NewGuid();
                tmpTables.Add(item: tmpTable);
                builder.AppendLine(value: generator.CreateOutputTableSql(tmpTable: tmpTable));
                builder.AppendLine(
                    value: "-----------------------------------------------------------------"
                );
                builder.AppendLine(value: "-- " + entity.Name);
                builder.AppendLine(
                    value: "-----------------------------------------------------------------"
                );
                builder.AppendLine(
                    value: generator.SelectSql(
                        ds: ds,
                        entity: entity,
                        filter: filterSet,
                        sortSet: null,
                        columnsInfo: DA.ColumnsInfo.Empty,
                        parameters: new Hashtable(),
                        selectParameterReferences: null,
                        paging: false
                    ) + ";"
                );
            }
        }
        builder.AppendLine(value: generator.CreateDataStructureFooterSql(tmpTables: tmpTables));
        new ShowSqlConsole(owner: new SqlConsoleParameters(command: builder.ToString())).Run();
    }
}
