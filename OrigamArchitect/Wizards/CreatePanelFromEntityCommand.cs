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

using Origam.Schema.GuiModel;
using Origam.Schema.EntityModel;
using Origam.UI;

namespace OrigamArchitect
{
	/// <summary>
	/// Summary description for CreatePanelFromEntityCommand.
	/// </summary>
	public class CreatePanelFromEntityCommand : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return Owner is IDataEntity;
			}
			set
			{
				throw new ArgumentException("Cannot set this property", "IsEnabled");
			}
		}

		public override void Run()
		{
			CreateFormFromEntityWizard wiz = new CreateFormFromEntityWizard();

			wiz.Entity = Owner as IDataEntity;
			if(wiz.ShowDialog() == DialogResult.OK)
			{
				string groupName = null;
				if(wiz.Entity.Group != null) groupName = wiz.Entity.Group.Name;

				PanelControlSet panel = GuiHelper.CreatePanel(groupName, wiz.Entity, wiz.SelectedFieldNames);

				Origam.Workbench.Commands.EditSchemaItem edit = new Origam.Workbench.Commands.EditSchemaItem();
				edit.Owner = panel;
				edit.Run();
			}
		}
	}
}
