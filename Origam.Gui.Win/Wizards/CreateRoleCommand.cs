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
using Origam.Schema.DeploymentModel;
using Origam.Schema.MenuModel;
using Origam.UI;
using Origam.UI.WizardForm;
using Origam.Workbench;

namespace Origam.Gui.Win.Wizards
{
    class CreateRoleCommand : AbstractMenuCommand
    {
		StructureForm structureForm;
		SchemaBrowser _schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
		public override bool IsEnabled
		{
			get
			{
                IAuthorizationContextContainer obj = Owner as IAuthorizationContextContainer;
                return obj != null && obj.AuthorizationContext != "" && obj.AuthorizationContext != null
                    && obj.AuthorizationContext != "*";
			}
			set
			{
				throw new ArgumentException("Cannot set this property", "IsEnabled");
			}
		}

		public override void Run()
		{
			ServiceCommandUpdateScriptActivity scriptActivity = new ServiceCommandUpdateScriptActivity();
			ArrayList list = new ArrayList();
			list.Add(new ListViewItem(scriptActivity.ModelDescription(), scriptActivity.Icon));
			Stack stackPage = new Stack();
			stackPage.Push(PagesList.Finish);
			stackPage.Push(PagesList.StartPage);

			structureForm = new StructureForm()
			{
				Title = ResourceUtils.GetString("CreateDataStructureFromEntityWizardTitle"),
				Description = ResourceUtils.GetString("CreateDataStructureFromEntityWizardDescription"),
				ItemTypeList = list,
				Pages = stackPage,
				NameOfEntity = ((AbstractMenuItem)Owner).Name,
				ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
				Command = this
			};
			Wizard wizardscreen = new Wizard(structureForm);
			if (wizardscreen.ShowDialog() != DialogResult.OK)
			{
				GeneratedModelElements.Clear();
			}
        }
		public override void Execute()
		{
			IAuthorizationContextContainer obj = Owner as IAuthorizationContextContainer;
			ServiceCommandUpdateScriptActivity activity =
				CreateRole(obj.AuthorizationContext);
			GeneratedModelElements.Add(activity);
		}

		public override int GetImageIndex(string icon)
		{
			return _schemaBrowser.ImageIndex(icon);
		}
	}
}
