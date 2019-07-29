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
using Origam.Schema.WorkflowModel;
using Origam.UI;
using Origam.UI.WizardForm;
using Origam.Workbench;

namespace Origam.Gui.Win.Wizards
{
	/// <summary>
	/// Summary description for CreatePanelFromEntityCommand.
	/// </summary>
	public class CreateWorkQueueClassFromEntityCommand : AbstractMenuCommand
	{
        ScreenWizardForm wizardForm;
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
            WorkQueueClass workQueue = new WorkQueueClass();
            list.Add(new ListViewItem(workQueue.ItemType, workQueue.Icon));

            Stack stackPage = new Stack();
            stackPage.Push(PagesList.finish);
            stackPage.Push(PagesList.ScreenForm);
            stackPage.Push(PagesList.startPage);

            wizardForm = new ScreenWizardForm
            {
                ItemTypeList = list,
                Title = "Create Work Queue Class From Entity Wizard",
                PageTitle = "",
                Description = "This will create Field for looku and bla bla bla bla bla bla bla bla bla bla bla bla ",
                Pages = stackPage,
                Entity = Owner as IDataEntity,
                NameOfEntity = (Owner as IDataEntity).Name,
                IsRoleVisible = false,
                textColumnsOnly = false,
                checkOnClick = true,
                ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
                Command = this
            };
            Wizard wiz = new Wizard(wizardForm);
			if(wiz.ShowDialog() != DialogResult.OK)
			{
                GeneratedModelElements.Clear();
            }
		}

        public override void Execute()
        {
            WorkQueueClass result = WorkflowHelper.CreateWorkQueueClass(
                    wizardForm.Entity, wizardForm.SelectedFields, GeneratedModelElements);
        }
    }
}