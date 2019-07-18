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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Schema.EntityModel;
using Origam.Schema.EntityModel.Wizards;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Services;
using Origam.UI;
using Origam.UI.WizardForm;
using Origam.Workbench;
using Origam.Workbench.Services;

namespace Origam.Gui.Win.Wizards
{
	/// <summary>
	/// Summary description for CreateFormFromEntityCommand.
	/// </summary>
	public class CreateFormFromEntityCommand : AbstractMenuCommand
	{
        ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
        SchemaBrowser _schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
        ScreenWizardForm wizardForm;
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
            DataStructureSchemaItemProvider dsprovider = schema.GetProvider(typeof(DataStructureSchemaItemProvider)) as DataStructureSchemaItemProvider;
            List<string> listdsName = dsprovider.ChildItemsByType(DataStructure.ItemTypeConst)
                            .ToArray()
                            .Select(x => { return ((AbstractSchemaItem)x).Name; })
                            .ToList();
            ArrayList list = new ArrayList();
            DataStructure dd = new DataStructure();
            PanelControlSet pp = new PanelControlSet();
            FormControlSet ff = new FormControlSet();
            list.Add(new ListViewItem(dd.ItemType, dd.Icon));
            list.Add(new ListViewItem(pp.ItemType, pp.Icon));
            list.Add(new ListViewItem(ff.ItemType, ff.Icon));
            
            Stack stackPage = new Stack();
            stackPage.Push(PagesList.finish);
            stackPage.Push(PagesList.ScreenForm);
            if (listdsName.Any(name => name == (Owner as IDataEntity).Name))
            {
                stackPage.Push(PagesList.StructureNamePage);
            }
            stackPage.Push(PagesList.startPage);

            wizardForm = new ScreenWizardForm
            {
                ItemTypeList = list,
                Description = "Create Screen Wizard",
                Pages = stackPage,
                Entity = Owner as IDataEntity,
                IsRoleVisible = false,
                textColumnsOnly = false,
                StructureList = listdsName,
                NameOfEntity = (Owner as IDataEntity).Name,
                ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
                Command = this
            };

            Wizard wiz = new Wizard(wizardForm);
            if (wiz.ShowDialog() != DialogResult.OK)
            {
                GeneratedModelElements.Clear();
            }
        }

        public override void Execute()
        {
            string groupName = null;
            if (wizardForm.Entity.Group != null) groupName = wizardForm.Entity.Group.Name;

            DataStructure dataStructure = EntityHelper.CreateDataStructure(wizardForm.Entity, wizardForm.NameOfEntity, true);
            GeneratedModelElements.Add(dataStructure);
            PanelControlSet panel = GuiHelper.CreatePanel(groupName, wizardForm.Entity, wizardForm.SelectedFieldNames, wizardForm.NameOfEntity);
            GeneratedModelElements.Add(panel);
            FormControlSet form = GuiHelper.CreateForm(dataStructure, groupName, panel);
            GeneratedModelElements.Add(form);
        }
    }

	public class CreateCompleteUICommand : AbstractMenuCommand
	{
        ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
        SchemaBrowser _schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
        ScreenWizardForm wizardForm;
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
            IDataEntity entity = Owner as IDataEntity;
            DataStructureSchemaItemProvider dsprovider = schema.GetProvider(typeof(DataStructureSchemaItemProvider)) as DataStructureSchemaItemProvider;
            List<string> listdsName = dsprovider.ChildItemsByType(DataStructure.ItemTypeConst)
                            .ToArray()
                            .Select(x => { return ((AbstractSchemaItem)x).Name; })
                            .ToList();

            ArrayList list = new ArrayList();
            DataStructure ds = new DataStructure();
            PanelControlSet panel1 = new PanelControlSet();
            FormControlSet frmSet = new FormControlSet();
            FormReferenceMenuItem form1 = new FormReferenceMenuItem();
            ServiceCommandUpdateScriptActivity activity1 = new ServiceCommandUpdateScriptActivity();

            list.Add(new ListViewItem(ds.ItemType, ds.Icon));
            list.Add(new ListViewItem(panel1.ItemType, panel1.Icon));
            list.Add(new ListViewItem(frmSet.ItemType, frmSet.Icon));
            list.Add(new ListViewItem(form1.ItemType, form1.Icon));
            list.Add(new ListViewItem(activity1.ItemType, activity1.Icon));

            Stack stackPage = new Stack();
            stackPage.Push(PagesList.finish);
            stackPage.Push(PagesList.ScreenForm);
            if (listdsName.Any(name => name == (Owner as IDataEntity).Name))
            {
                stackPage.Push(PagesList.StructureNamePage);
            }
            stackPage.Push(PagesList.startPage);

            wizardForm = new ScreenWizardForm
            {
                ItemTypeList = list,
                Description = "Create Complete UI Wizard",
                Pages = stackPage,
                Entity = Owner as IDataEntity,
                IsRoleVisible = true,
                textColumnsOnly = false,
                StructureList = listdsName,
                NameOfEntity = (Owner as IDataEntity).Name,
                ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
                Command = this,
                Role = entity.Name
            };

            Wizard wiz = new Wizard(wizardForm);
            if (wiz.ShowDialog() != DialogResult.OK)
            {
                GeneratedModelElements.Clear();
            }
		}

        public override void Execute()
        {
            string groupName = null;
            if (wizardForm.Entity.Group != null) groupName = wizardForm.Entity.Group.Name;

            DataStructure dataStructure = EntityHelper.CreateDataStructure(wizardForm.Entity, wizardForm.NameOfEntity, true);
            PanelControlSet panel = GuiHelper.CreatePanel(groupName, wizardForm.Entity, wizardForm.SelectedFieldNames,wizardForm.NameOfEntity);
            FormControlSet form = GuiHelper.CreateForm(dataStructure, groupName, panel);
            FormReferenceMenuItem menu = MenuHelper.CreateMenuItem(wizardForm.Entity.Caption == null || wizardForm.Entity.Caption == ""
                ? wizardForm.NameOfEntity : wizardForm.Entity.Caption, wizardForm.Role, form);
            GeneratedModelElements.Add(dataStructure);
            GeneratedModelElements.Add(panel);
            GeneratedModelElements.Add(form);
            GeneratedModelElements.Add(menu);
            if (wizardForm.Role != "*" && wizardForm.Role != "")
            {
                ServiceCommandUpdateScriptActivity activity = CreateRole(wizardForm.Role);
                GeneratedModelElements.Add(activity);
            }
        }
    }

	public class CreateFormFromPanelCommand : AbstractMenuCommand
	{
        public override bool IsEnabled
		{
			get
			{
				return Owner is PanelControlSet;
			}
			set
			{
				throw new ArgumentException("Cannot set this property", "IsEnabled");
			}
		}

		public override void Run()
		{
            PanelControlSet panel = Owner as PanelControlSet;
			string groupName = null;
			if(panel.Group != null) groupName = panel.Group.Name;
            DataStructure dataStructure = EntityHelper.CreateDataStructure(panel.DataEntity, panel.DataEntity.Name, true);
            GeneratedModelElements.Add(dataStructure);
            FormControlSet form = GuiHelper.CreateForm(dataStructure, groupName, panel);
            GeneratedModelElements.Add(form);
            Origam.Workbench.Commands.EditSchemaItem edit = new Origam.Workbench.Commands.EditSchemaItem();
            edit.Owner = form;
            edit.Run();
		}
	}

	public class CreateMenuFromFormCommand : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return Owner is FormControlSet;
			}
			set
			{
				throw new ArgumentException("Cannot set this property", "IsEnabled");
			}
		}

		public override void Run()
		{
			FormControlSet form = Owner as FormControlSet;
			CreateMenuFromFormWizard wiz = new CreateMenuFromFormWizard();
			wiz.Role = form.Name;
			if(wiz.ShowDialog() == DialogResult.OK)
			{
				FormReferenceMenuItem menu = MenuHelper.CreateMenuItem(wiz.Caption, wiz.Role, form);
                GeneratedModelElements.Add(menu);
				bool createRole = wiz.Role != "*" && wiz.Role != "";
				if(createRole)
				{
					ServiceCommandUpdateScriptActivity activity = CreateRole(wiz.Role);
                    GeneratedModelElements.Add(activity);
                }
			}
		}
	}

	public class CreateMenuFromDataConstantCommand : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return Owner is DataConstant;
			}
			set
			{
				throw new ArgumentException("Cannot set this property", "IsEnabled");
			}
		}

		public override void Run()
		{
			DataConstant constant = Owner as DataConstant;
			CreateMenuFromFormWizard wiz = new CreateMenuFromFormWizard();
			wiz.Role = constant.Name;
			if(wiz.ShowDialog() == DialogResult.OK)
			{
				DataConstantReferenceMenuItem menu = MenuHelper.CreateMenuItem(wiz.Caption, wiz.Role, constant);
                GeneratedModelElements.Add(menu);
                bool createRole = wiz.Role != "*" && wiz.Role != "";
				if(createRole)
				{
                    ServiceCommandUpdateScriptActivity activity = CreateRole(wiz.Role);
                    GeneratedModelElements.Add(activity);
				}
			}
		}
    }

	public class CreateMenuFromSequentialWorkflowCommand : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return Owner is Schema.WorkflowModel.Workflow;
			}
			set
			{
				throw new ArgumentException("Cannot set this property", "IsEnabled");
			}
		}

		public override void Run()
		{
			Schema.WorkflowModel.Workflow wf = Owner as Schema.WorkflowModel.Workflow;
			CreateMenuFromFormWizard wiz = new CreateMenuFromFormWizard();
			wiz.Role = wf.Name;
			if(wiz.ShowDialog() == DialogResult.OK)
			{
				WorkflowReferenceMenuItem menu = MenuHelper.CreateMenuItem(wiz.Caption, wiz.Role, wf);
                GeneratedModelElements.Add(menu);
				bool createRole = wiz.Role != "*" && wiz.Role != "";
				if(createRole)
				{
					ServiceCommandUpdateScriptActivity activity = CreateRole(wiz.Role);
                    GeneratedModelElements.Add(activity);
				}
			}
		}
	}
}
