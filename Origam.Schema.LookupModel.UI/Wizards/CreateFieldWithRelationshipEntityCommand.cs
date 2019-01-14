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
using System.Windows.Forms;
using Origam.Schema.EntityModel;
using Origam.UI;

namespace Origam.Schema.LookupModel.Wizards
{
    /// <summary>
    /// Summary description for CreateFieldWithLookupRelationshipEntityCommand.
    /// </summary>
    public class CreateFieldWithRelationshipEntityCommand : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
                return Owner is IDataEntity
                    || Owner is IDataEntityColumn;
			}
			set
			{
				throw new ArgumentException("Cannot set this property", "IsEnabled");
			}
		}

		public override void Run()
		{
            FieldMappingItem baseField = Owner as FieldMappingItem;
            IDataEntity baseEntity = Owner as IDataEntity;
            if (baseField != null)
            {
                baseEntity = baseField.ParentItem as IDataEntity;
            }
            CreateFieldWithRelationshipEntityWizard wiz = new CreateFieldWithRelationshipEntityWizard
            {
                Entity = baseEntity
            };

            if (wiz.ShowDialog() == DialogResult.OK)
            {
                // 1. entity
                TableMappingItem table = (TableMappingItem)baseEntity;
                EntityRelationItem relation = EntityHelper.CreateRelation(table, (IDataEntity)wiz.RelatedEntity,wiz.ParentChildCheckbox, true);
                EntityHelper.CreateRelationKey(relation, 
                    (AbstractDataEntityColumn)wiz.BaseEntityFieldSelect,
                    (AbstractDataEntityColumn)wiz.RelatedEntityFieldSelect,true);
            }
        }
    }
}
