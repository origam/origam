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

using System;
using System.Collections;
using System.Windows.Forms;

using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Schema.EntityModel.Wizards
{
	/// <summary>
	/// Summary description for CreateNtoNEntityCommand.
	/// </summary>
	public class CreateChildEntityCommand : AbstractMenuCommand
	{
		WorkbenchSchemaService _schema = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;

		public override bool IsEnabled
		{
			get
			{
				return Owner is IDataEntity;
			}
			set
			{
				throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
			}
		}

		public override void Run()
		{
			IDataEntity entity = Owner as IDataEntity;

			CreateChildEntityWizard wiz = new CreateChildEntityWizard();
			wiz.Entity1 = entity;
			
			if(wiz.ShowDialog() == DialogResult.OK)
			{
				IDataEntity entity1 = wiz.Entity1;
				// 1. Create N:N Entity with reference to both entities
				TableMappingItem newEntity = EntityHelper.CreateTable(wiz.EntityName, wiz.Entity1.Group, false);
				newEntity.Persist();
                GeneratedModelElements.Add(newEntity);
				// Create index by parent entity
				DataEntityIndex index = newEntity.NewItem(typeof(DataEntityIndex), _schema.ActiveSchemaExtensionId, null) as DataEntityIndex;
				index.Name = "ix_" + entity1.Name;
				index.Persist();				
				// Create relation from the parent entity
				EntityRelationItem parentRelation = EntityHelper.CreateRelation(entity1, newEntity, true, true);
                GeneratedModelElements.Add(parentRelation);
                ArrayList entity1keys = new ArrayList();
				// Create reference columns
				foreach(IDataEntityColumn pk in entity1.EntityPrimaryKey)
				{
					if(!pk.ExcludeFromAllFields)
					{
						FieldMappingItem refEntity1 = EntityHelper.CreateColumn(newEntity, "ref" + entity1.Name + pk.Name, false, pk.DataType, pk.DataLength, entity1.Caption, entity1, pk, true);
						EntityRelationColumnPairItem key = EntityHelper.CreateRelationKey(parentRelation, pk, refEntity1, true);
						entity1keys.Add(refEntity1);
					}
				}
				if(wiz.Entity2 != null)
				{
					foreach(IDataEntityColumn pk in wiz.Entity2.EntityPrimaryKey)
					{
						if(!pk.ExcludeFromAllFields)
						{
							EntityHelper.CreateColumn(newEntity, "ref" + wiz.Entity2.Name + pk.Name, false, pk.DataType, pk.DataLength, wiz.Entity2.Caption, wiz.Entity2, pk, true);
						}
					}
				}
				int i = 0;
				foreach(IDataEntityColumn col in entity1keys)
				{
					DataEntityIndexField field = index.NewItem(typeof(DataEntityIndexField), _schema.ActiveSchemaExtensionId, null) as DataEntityIndexField;
					field.Field = col;
					field.OrdinalPosition = i;
					field.Persist();					
					i++;
				}
				newEntity.Persist();
				(entity1 as AbstractSchemaItem).Persist();
			}
		}
	}
}
