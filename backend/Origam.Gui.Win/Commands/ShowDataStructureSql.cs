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


namespace Origam.Gui.Win.Commands;

public class ShowDataStructureSql : AbstractMenuCommand
{
    private WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(typeof(SchemaService)) as WorkbenchSchemaService;
    public override bool IsEnabled
    {
        get { return Owner is DataStructure; }
        set { base.IsEnabled = value; }
    }

    public override void Run()
    {
        AbstractSqlDataService abstractSqlDataService =
            DataServiceFactory.GetDataService() as AbstractSqlDataService;
        AbstractSqlCommandGenerator generator = (AbstractSqlCommandGenerator)
            abstractSqlDataService.DbDataAdapterFactory.Clone();
        DataStructure ds = Owner as DataStructure;
        generator.PrettyFormat = true;
        generator.GenerateConsoleUseSyntax = true;
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("-- SQL statements for data structure: {0}\r\n", ds.Name);
        List<string> tmpTables = new List<string>();
        foreach (DataStructureEntity entity in ds.Entities)
        {
            if (entity.Columns.Count > 0)
            {
                string tmpTable = "tmptable" + System.Guid.NewGuid();
                tmpTables.Add(tmpTable);
                builder.AppendLine(generator.CreateOutputTableSql(tmpTable));
                builder.AppendLine(
                    "-----------------------------------------------------------------"
                );
                builder.AppendLine("-- " + entity.Name);
                builder.AppendLine(
                    "-----------------------------------------------------------------"
                );
                builder.AppendLine(
                    generator.SelectSql(
                        ds,
                        entity,
                        null,
                        null,
                        DA.ColumnsInfo.Empty,
                        new Hashtable(),
                        null,
                        false
                    ) + ";"
                );
            }
        }
        builder.AppendLine(generator.CreateDataStructureFooterSql(tmpTables));
        new ShowSqlConsole(new SqlConsoleParameters(builder.ToString())).Run();
    }
}
