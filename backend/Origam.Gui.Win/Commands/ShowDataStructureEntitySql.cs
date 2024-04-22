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
        ServiceManager.Services.GetService(
            typeof(SchemaService)) as WorkbenchSchemaService;

    public override bool IsEnabled
    {
        get
        {
                return Owner is DataStructureEntity;
            }
        set
        {
                base.IsEnabled = value;
            }
    }

    public override void Run()
    {
            AbstractSqlDataService abstractSqlDataService = DataServiceFactory.GetDataService() as AbstractSqlDataService;
            AbstractSqlCommandGenerator generator = (AbstractSqlCommandGenerator) abstractSqlDataService.DbDataAdapterFactory.Clone();
            DataStructureEntity entity = Owner as DataStructureEntity;
            StringBuilder builder = new StringBuilder();
            if (entity.Columns.Count > 0)
            {
                DataStructure ds = (Owner as ISchemaItem).RootItem as DataStructure;
                builder.AppendLine("-- SQL statements for data structure: " + ds.Name);
                generator.PrettyFormat = true;
                generator.GenerateConsoleUseSyntax = true;
                // parameter declarations
                builder.AppendLine(
                    generator.SelectParameterDeclarationsSql(
                        ds,
                        Owner as DataStructureEntity,
                        (DataStructureFilterSet)null, false, null)
                );
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine("-- " + (Owner as DataStructureEntity).Name);
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine(
                    generator.SelectSql(
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
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine("-- Load Record After Update SQL");
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine(
                    generator.SelectRowSql(
                        Owner as DataStructureEntity,
                        null,
                        new Hashtable(),
                        DA.ColumnsInfo.Empty,
                        true
                    )
                );
                builder.AppendLine();
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine("-- INSERT SQL");
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine(
                    generator.InsertSql(
                        ds,
                        Owner as DataStructureEntity
                    )
                );
                builder.AppendLine();
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine("-- UPDATE SQL");
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine(
                    generator.UpdateSql(
                        ds,
                        Owner as DataStructureEntity
                    )
                );
                builder.AppendLine();
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine("-- DELETE SQL");
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine(
                    generator.DeleteSql(
                        ds,
                        Owner as DataStructureEntity
                    )
                );
            }
            else
            {
                builder.AppendLine("No SQL command generated for this entity. No columns selected.");
            }
            new ShowSqlConsole(new SqlConsoleParameters(builder.ToString())).Run();
        }
}