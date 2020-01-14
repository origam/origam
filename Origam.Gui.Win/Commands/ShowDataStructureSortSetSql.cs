#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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

namespace Origam.Gui.Win.Commands
{
    public class ShowDataStructureSortSetSql : AbstractMenuCommand
    {
        private WorkbenchSchemaService _schema =
            ServiceManager.Services.GetService(
                typeof(SchemaService)) as WorkbenchSchemaService;

        public override bool IsEnabled
        {
            get
            {
                return Owner is DataStructureSortSet;
            }
            set
            {
                base.IsEnabled = value;
            }
        }

        public override void Run()
        {
            StringBuilder builder = new StringBuilder();
            AbstractSqlDataService abstractSqlDataService = DataService.GetDataService() as AbstractSqlDataService;
            AbstractSqlCommandGenerator generator = (AbstractSqlCommandGenerator)abstractSqlDataService.DbDataAdapterFactory.Clone();
            generator.PrettyFormat = true;
            generator.generateConsoleUseSyntax = true;
            bool displayPagingParameters = true;
            DataStructure ds = (Owner as ISchemaItem).RootItem as DataStructure;
            builder.AppendLine("-- SQL statements for data structure: " + ds.Name);
            foreach (DataStructureEntity entity in ds.Entities)
            {
                if (entity.Columns.Count > 0)
                {
                    builder.AppendLine("-----------------------------------------------------------------");
                    builder.AppendLine("-- " + entity.Name);
                    builder.AppendLine("-----------------------------------------------------------------");
                    // parameter declarations
                    builder.AppendLine(
                        generator.SelectParameterDeclarationsSql(
                            ds, entity, Owner as DataStructureSortSet,
                            displayPagingParameters, null));
                    builder.AppendLine(generator.CreateOutputTableSql());
                    builder.AppendLine(
                        generator.SelectSql(ds,
                            entity,
                            null,
                            Owner as DataStructureSortSet,
                            DA.ColumnsInfo.Empty,
                            new Hashtable(),
                            new Hashtable(),
                            displayPagingParameters
                        )
                    );
                    builder.AppendLine(generator.CreateDataStructureFooterSql());
                }
            }
            new ShowSqlConsole(builder.ToString()).Run();
        }
    }
}