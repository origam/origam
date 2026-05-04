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
using System.Text;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.UI;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Gui.Win.Commands;

public class ShowDataStructureEntitySql : AbstractMenuCommand
{
    private WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(SchemaService))
        as WorkbenchSchemaService;
    public override bool IsEnabled
    {
        get { return Owner is DataStructureEntity; }
        set { base.IsEnabled = value; }
    }

    public override void Run()
    {
        AbstractSqlDataService abstractSqlDataService =
            DataServiceFactory.GetDataService() as AbstractSqlDataService;
        AbstractSqlCommandGenerator generator = (AbstractSqlCommandGenerator)
            abstractSqlDataService.DbDataAdapterFactory.Clone();
        DataStructureEntity entity = Owner as DataStructureEntity;
        StringBuilder builder = new StringBuilder();
        if (entity.Columns.Count > 0)
        {
            DataStructure ds = (Owner as ISchemaItem).RootItem as DataStructure;
            builder.AppendLine(value: "-- SQL statements for data structure: " + ds.Name);
            generator.PrettyFormat = true;
            generator.GenerateConsoleUseSyntax = true;
            // parameter declarations
            builder.AppendLine(
                value: generator.SelectParameterDeclarationsSql(
                    ds: ds,
                    entity: Owner as DataStructureEntity,
                    filter: (DataStructureFilterSet)null,
                    paging: false,
                    columnName: null
                )
            );
            builder.AppendLine(
                value: "-----------------------------------------------------------------"
            );
            builder.AppendLine(value: "-- " + (Owner as DataStructureEntity).Name);
            builder.AppendLine(
                value: "-----------------------------------------------------------------"
            );
            builder.AppendLine(
                value: generator.SelectSql(
                    ds: ds,
                    entity: Owner as DataStructureEntity,
                    filter: null,
                    sortSet: null,
                    columnsInfo: DA.ColumnsInfo.Empty,
                    parameters: new Hashtable(),
                    selectParameterReferences: null,
                    paging: false
                )
            );
            builder.AppendLine();
            builder.AppendLine(
                value: "-----------------------------------------------------------------"
            );
            builder.AppendLine(value: "-- Load Record After Update SQL");
            builder.AppendLine(
                value: "-----------------------------------------------------------------"
            );
            builder.AppendLine(
                value: generator.SelectRowSql(
                    entity: Owner as DataStructureEntity,
                    filterSet: null,
                    selectParameterReferences: new Hashtable(),
                    columnsInfo: DA.ColumnsInfo.Empty,
                    forceDatabaseCalculation: true
                )
            );
            builder.AppendLine();
            builder.AppendLine(
                value: "-----------------------------------------------------------------"
            );
            builder.AppendLine(value: "-- INSERT SQL");
            builder.AppendLine(
                value: "-----------------------------------------------------------------"
            );
            builder.AppendLine(
                value: generator.InsertSql(ds: ds, entity: Owner as DataStructureEntity)
            );
            builder.AppendLine();
            builder.AppendLine(
                value: "-----------------------------------------------------------------"
            );
            builder.AppendLine(value: "-- UPDATE SQL");
            builder.AppendLine(
                value: "-----------------------------------------------------------------"
            );
            builder.AppendLine(
                value: generator.UpdateSql(ds: ds, entity: Owner as DataStructureEntity)
            );
            builder.AppendLine();
            builder.AppendLine(
                value: "-----------------------------------------------------------------"
            );
            builder.AppendLine(value: "-- DELETE SQL");
            builder.AppendLine(
                value: "-----------------------------------------------------------------"
            );
            builder.AppendLine(
                value: generator.DeleteSql(ds: ds, entity: Owner as DataStructureEntity)
            );
        }
        else
        {
            builder.AppendLine(
                value: "No SQL command generated for this entity. No columns selected."
            );
        }
        new ShowSqlConsole(owner: new SqlConsoleParameters(command: builder.ToString())).Run();
    }
}
