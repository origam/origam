#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Schema.MenuModel;

public static class MenuHelper
{
    public static FormReferenceMenuItem CreateMenuItem(
        string caption,
        string role,
        FormControlSet form
    )
    {
        if (string.IsNullOrEmpty(caption))
        {
            throw new ArgumentOutOfRangeException("Caption cannot be null for new menu item");
        }
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var menuSchemaItemProvider = schemaService.GetProvider<MenuSchemaItemProvider>();
        if (
            (menuSchemaItemProvider.ChildItems.Count > 0)
            && menuSchemaItemProvider.ChildItems[0].IsPersisted
        )
        {
            var menu = menuSchemaItemProvider.MainMenu;
            var formReferenceMenuItem = menu.NewItem<FormReferenceMenuItem>(
                schemaService.ActiveSchemaExtensionId,
                null
            );
            formReferenceMenuItem.Name = form.Name;
            formReferenceMenuItem.DisplayName = caption;
            formReferenceMenuItem.Screen = form;
            formReferenceMenuItem.Roles = role;
            formReferenceMenuItem.Persist();
            return formReferenceMenuItem;
        }
        throw new Exception("No menu defined. Create a root menu element.");
    }

    public static DataConstantReferenceMenuItem CreateMenuItem(
        string caption,
        string role,
        DataConstant constant
    )
    {
        if (string.IsNullOrEmpty(caption))
        {
            throw new ArgumentOutOfRangeException("Caption cannot be null for new menu item");
        }
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var menuSchemaItemProvider = schemaService.GetProvider<MenuSchemaItemProvider>();
        if (menuSchemaItemProvider.ChildItems.Count > 0)
        {
            var menu = menuSchemaItemProvider.MainMenu;
            var dataConstantReferenceMenuItem = menu.NewItem<DataConstantReferenceMenuItem>(
                schemaService.ActiveSchemaExtensionId,
                null
            );
            dataConstantReferenceMenuItem.Name = constant.Name;
            dataConstantReferenceMenuItem.DisplayName = caption;
            dataConstantReferenceMenuItem.Constant = constant;
            dataConstantReferenceMenuItem.Roles = role;
            dataConstantReferenceMenuItem.Persist();
            return dataConstantReferenceMenuItem;
        }
        throw new Exception("No menu defined. Create a root menu element.");
    }

    public static WorkflowReferenceMenuItem CreateMenuItem(
        string caption,
        string role,
        IWorkflow workflow
    )
    {
        if (string.IsNullOrEmpty(caption))
        {
            throw new ArgumentOutOfRangeException("Caption cannot be null for new menu item");
        }
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var menuSchemaItemProvider = schemaService.GetProvider<MenuSchemaItemProvider>();
        if (menuSchemaItemProvider.ChildItems.Count > 0)
        {
            var menu = menuSchemaItemProvider.MainMenu;
            var workflowReferenceMenuItem = menu.NewItem<WorkflowReferenceMenuItem>(
                schemaService.ActiveSchemaExtensionId,
                null
            );
            workflowReferenceMenuItem.Name = workflow.Name;
            workflowReferenceMenuItem.DisplayName = caption;
            workflowReferenceMenuItem.Workflow = workflow;
            workflowReferenceMenuItem.Roles = role;
            workflowReferenceMenuItem.Persist();
            return workflowReferenceMenuItem;
        }
        throw new Exception("No menu defined. Create a root menu element.");
    }
}
