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
using Origam.Schema.DeploymentModel;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.UI;

namespace Origam.Gui.Win.Wizards
{
	/// <summary>
	/// Summary description for CreateFormFromEntityCommand.
	/// </summary>
	public class CreateFormFromEntityCommand : AbstractMenuCommand
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
			wiz.IsRoleVisible = false;

			if(wiz.ShowDialog() == DialogResult.OK)
			{
				string groupName = null;
				if(wiz.Entity.Group != null) groupName = wiz.Entity.Group.Name;

				DataStructure dataStructure = EntityHelper.CreateDataStructure(wiz.Entity, wiz.Entity.Name, true);
                GeneratedModelElements.Add(dataStructure);
                PanelControlSet panel = GuiHelper.CreatePanel(groupName, wiz.Entity, wiz.SelectedFieldNames);
                GeneratedModelElements.Add(panel);
                FormControlSet form = GuiHelper.CreateForm(dataStructure, groupName, panel);
                GeneratedModelElements.Add(form);
            }
        }
	}

	public class CreateCompleteUICommand : AbstractMenuCommand
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
			wiz.IsRoleVisible = true;
			wiz.Entity = Owner as IDataEntity;
			wiz.Role = wiz.Entity.Name;

			if(wiz.ShowDialog() == DialogResult.OK)
			{
				string groupName = null;
				if(wiz.Entity.Group != null) groupName = wiz.Entity.Group.Name;

				DataStructure dataStructure = EntityHelper.CreateDataStructure(wiz.Entity, wiz.Entity.Name, true);
				PanelControlSet panel = GuiHelper.CreatePanel(groupName, wiz.Entity, wiz.SelectedFieldNames);
				FormControlSet form = GuiHelper.CreateForm(dataStructure, groupName, panel);
				FormReferenceMenuItem menu = MenuHelper.CreateMenuItem(wiz.Entity.Caption == null || wiz.Entity.Caption == ""
					? wiz.Entity.Name : wiz.Entity.Caption, wiz.Role, form);
                GeneratedModelElements.Add(dataStructure);
                GeneratedModelElements.Add(dataStructure);
                GeneratedModelElements.Add(panel);
                GeneratedModelElements.Add(form);
                GeneratedModelElements.Add(menu);
				if(wiz.Role != "*" && wiz.Role != "")
				{
					ServiceCommandUpdateScriptActivity activity = CreateRole(wiz.Role);
                    GeneratedModelElements.Add(activity);
				}
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
