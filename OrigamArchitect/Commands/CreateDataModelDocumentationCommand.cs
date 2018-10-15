#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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

using System;
using System.Collections;

using Origam.UI;
using Origam.Workbench;
using Origam.Workbench.Services;
using Origam.Schema.EntityModel;

namespace OrigamArchitect.Commands
{
	/// <summary>
	/// Summary description for CreateDataModelDocumentationCommand.
	/// </summary>
	public class CreateDataModelDocumentationCommand : AbstractMenuCommand
	{
		SchemaService _schemaService = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;

		public override bool IsEnabled
		{
			get
			{
				return _schemaService.IsSchemaLoaded;
			}
			set
			{
				throw new ArgumentException("Cannot set this property", "IsEnabled");
			}
		}

		public override void Run()
		{
			ArrayList list = new ArrayList();

			ArrayList headerRow = new ArrayList();
			headerRow.Add("Table Name");
			headerRow.Add("Table Caption");
			headerRow.Add("Field Name");
			headerRow.Add("Field Caption");

			list.Add(headerRow);

			EntityModelSchemaItemProvider entities = _schemaService.GetProvider(typeof(EntityModelSchemaItemProvider)) as EntityModelSchemaItemProvider;
			
			foreach(IDataEntity entity in entities.ChildItemsByType(TableMappingItem.ItemTypeConst))
			{
				TableMappingItem table = entity as TableMappingItem;

				if(table != null)
				{
					foreach(IDataEntityColumn column in table.EntityColumns)
					{
						FieldMappingItem field = column as FieldMappingItem;

						if(field != null)
						{
							ArrayList row = new ArrayList();

							row.Add(table.MappedObjectName);
							row.Add(table.Caption);
							row.Add(field.MappedColumnName);
							row.Add(field.Caption);

							list.Add(row);
						}
					}
				}
			}

			WorkbenchSingleton.Workbench.ExportToExcel("Data Model Documentation", list);
		}		
	}
}
