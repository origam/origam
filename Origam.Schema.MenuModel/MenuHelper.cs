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
using Origam.Services;
using Origam.Schema.WorkflowModel;
using Origam.Schema.GuiModel;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;

namespace Origam.Schema.MenuModel
{
	/// <summary>
	/// Summary description for MenuHelper.
	/// </summary>
	public class MenuHelper
	{
		public static FormReferenceMenuItem CreateMenuItem(string caption, string role, FormControlSet form)
		{
			if(caption == "" || caption == null)
			{
				throw new ArgumentOutOfRangeException("Caption cannot be null for new menu item");
			}

			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

			MenuSchemaItemProvider menuProvider = schema.GetProvider(typeof(MenuSchemaItemProvider)) as MenuSchemaItemProvider;
			
			if(menuProvider.ChildItems.Count > 0 && menuProvider.ChildItems[0].IsPersisted)
			{
				Menu menu = menuProvider.ChildItems[0] as Menu;

				FormReferenceMenuItem formMenu = menu.NewItem(typeof(FormReferenceMenuItem), schema.ActiveSchemaExtensionId, null) as FormReferenceMenuItem;
				formMenu.Name = form.Name;
				formMenu.DisplayName = caption;
				formMenu.Screen = form;
				formMenu.Roles = role;

				formMenu.Persist();

				return formMenu;
			}
			else
			{
				throw new Exception("No menu defined. Create a root menu element.");
			}
		}

		public static DataConstantReferenceMenuItem CreateMenuItem(string caption, string role, DataConstant constant)
		{
			if(caption == "" || caption == null)
			{
				throw new ArgumentOutOfRangeException("Caption cannot be null for new menu item");
			}

			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

			MenuSchemaItemProvider menuProvider = schema.GetProvider(typeof(MenuSchemaItemProvider)) as MenuSchemaItemProvider;
			
			if(menuProvider.ChildItems.Count > 0)
			{
				Menu menu = menuProvider.ChildItems[0] as Menu;

				DataConstantReferenceMenuItem constantMenu = menu.NewItem(typeof(DataConstantReferenceMenuItem), schema.ActiveSchemaExtensionId, null) as DataConstantReferenceMenuItem;
				constantMenu.Name = constant.Name;
				constantMenu.DisplayName = caption;
				constantMenu.Constant = constant;
				constantMenu.Roles = role;

				constantMenu.Persist();

				return constantMenu;
			}
			else
			{
				throw new Exception("No menu defined. Create a root menu element.");
			}
		}

		public static WorkflowReferenceMenuItem CreateMenuItem(string caption, string role, IWorkflow wf)
		{
			if(caption == "" || caption == null)
			{
				throw new ArgumentOutOfRangeException("Caption cannot be null for new menu item");
			}

			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

			MenuSchemaItemProvider menuProvider = schema.GetProvider(typeof(MenuSchemaItemProvider)) as MenuSchemaItemProvider;
			
			if(menuProvider.ChildItems.Count > 0)
			{
				Menu menu = menuProvider.ChildItems[0] as Menu;
				WorkflowReferenceMenuItem wfMenu = menu.NewItem(typeof(WorkflowReferenceMenuItem), schema.ActiveSchemaExtensionId, null) as WorkflowReferenceMenuItem;
				wfMenu.Name = wf.Name;
				wfMenu.DisplayName = caption;
				wfMenu.Workflow = wf;
				wfMenu.Roles = role;
				wfMenu.Persist();
				return wfMenu;
			}
			else
			{
				throw new Exception("No menu defined. Create a root menu element.");
			}
		}
	}
}
