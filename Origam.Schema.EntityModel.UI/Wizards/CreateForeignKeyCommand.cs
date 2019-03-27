#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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

using Origam.UI;
using Origam.Workbench.Commands;

namespace Origam.Schema.EntityModel.Wizards
{
	/// <summary>
	/// Summary description for CreateNtoNEntityCommand.
	/// </summary>
	public class CreateForeignKeyCommand : AbstractMenuCommand
	{
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
			CreateForeignKeyWizard wiz = new CreateForeignKeyWizard();
			wiz.MasterEntity = entity;
			if(wiz.ShowDialog() == DialogResult.OK)
			{
				FieldMappingItem fk = EntityHelper.CreateForeignKey(
                    wiz.ForeignKeyName, wiz.Caption, wiz.AllowNulls, entity, 
                    wiz.ForeignEntity, wiz.ForeignField, wiz.Lookup, false);
				EditSchemaItem cmd = new EditSchemaItem();
				cmd.Owner = fk;
				cmd.Run();
			}
		}
	}
}
