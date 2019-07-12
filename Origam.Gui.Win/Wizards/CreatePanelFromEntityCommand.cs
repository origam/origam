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
using System.Collections;
using System.Windows.Forms;
using Origam.Schema.EntityModel;
using Origam.Schema.EntityModel.Wizards;
using Origam.Schema.GuiModel;
using Origam.UI;
using Origam.UI.WizardForm;
using Origam.Workbench;

namespace Origam.Gui.Win.Wizards
{
	/// <summary>
	/// Summary description for CreatePanelFromEntityCommand.
	/// </summary>
	public class CreatePanelFromEntityCommand : AbstractMenuCommand
	{
        SchemaBrowser _schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
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
            ArrayList list = new ArrayList();
            PanelControlSet pp = new PanelControlSet();
            list.Add(new ListViewItem(pp.ItemType, _schemaBrowser.ImageIndex(pp.Icon)));
            
            Stack stackPage = new Stack();
            stackPage.Push(PagesList.ScreenForm);
            stackPage.Push(PagesList.startPage);

            ScreenWizardForm wizardForm = new ScreenWizardForm
            {
                listItemType = list,
                Description = "Create Screen Section Wizard",
                Pages = stackPage,
                Entity = Owner as IDataEntity,
                IsRoleVisible = false,
                textColumnsOnly = false,
                imgList = _schemaBrowser.EbrSchemaBrowser.imgList,
            };

            Wizard wiz = new Wizard(wizardForm);

            //CreateFormFromEntityWizard wiz = new CreateFormFromEntityWizard();

			//wiz.Entity = Owner as IDataEntity;
			if(wiz.ShowDialog() == DialogResult.OK)
			{
				string groupName = null;
				if(wizardForm.Entity.Group != null) groupName = wizardForm.Entity.Group.Name;

				PanelControlSet panel = GuiHelper.CreatePanel(groupName, wizardForm.Entity, wizardForm.SelectedFieldNames);

				Origam.Workbench.Commands.EditSchemaItem edit = new Origam.Workbench.Commands.EditSchemaItem();
				edit.Owner = panel;
				edit.Run();
			}
		}
	}
}
