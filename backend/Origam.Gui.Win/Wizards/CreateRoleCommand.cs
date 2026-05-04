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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Origam.Schema.DeploymentModel;
using Origam.Schema.MenuModel;
using Origam.UI;
using Origam.UI.WizardForm;
using Origam.Workbench;

namespace Origam.Gui.Win.Wizards;

class CreateRoleCommand : AbstractMenuCommand
{
    RoleForm roleForm;
    SchemaBrowser _schemaBrowser =
        WorkbenchSingleton.Workbench.GetPad(type: typeof(SchemaBrowser)) as SchemaBrowser;
    public override bool IsEnabled
    {
        get
        {
            IAuthorizationContextContainer obj = Owner as IAuthorizationContextContainer;
            return obj != null
                && obj.AuthorizationContext != ""
                && obj.AuthorizationContext != null
                && obj.AuthorizationContext != "*";
        }
        set
        {
            throw new ArgumentException(
                message: "Cannot set this property",
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        ServiceCommandUpdateScriptActivity scriptActivity =
            new ServiceCommandUpdateScriptActivity();
        var list = new List<ListViewItem>();
        list.Add(
            item: new ListViewItem(
                text: scriptActivity.ModelDescription(),
                imageKey: scriptActivity.Icon
            )
        );
        Stack stackPage = new Stack();
        stackPage.Push(obj: PagesList.Finish);
        stackPage.Push(obj: PagesList.SummaryPage);
        stackPage.Push(obj: PagesList.StartPage);
        roleForm = new RoleForm()
        {
            Title = ResourceUtils.GetString(key: "CreateDataStructureFromEntityWizardTitle"),
            Description = ResourceUtils.GetString(
                key: "CreateDataStructureFromEntityWizardDescription"
            ),
            ItemTypeList = list,
            Pages = stackPage,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            Command = this,
            Roles = ((AbstractMenuItem)Owner).Roles,
            NameOfMenu = ((AbstractMenuItem)Owner).DisplayName,
        };
        Wizard wizardscreen = new Wizard(objectForm: roleForm);
        if (wizardscreen.ShowDialog() != DialogResult.OK)
        {
            GeneratedModelElements.Clear();
        }
    }

    public override void Execute()
    {
        IAuthorizationContextContainer obj = Owner as IAuthorizationContextContainer;
        ServiceCommandUpdateScriptActivity activity = CreateRole(role: obj.AuthorizationContext);
        GeneratedModelElements.Add(item: activity);
    }

    public override int GetImageIndex(string icon)
    {
        return _schemaBrowser.ImageIndex(icon: icon);
    }

    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text = "This Wizard will create a Role with these parameters:";
        richTextBoxSummary.AppendText(text: "");
        richTextBoxSummary.AppendText(text: "Create Role ");
        richTextBoxSummary.SelectionFont = new Font(
            prototype: richTextBoxSummary.Font,
            newStyle: FontStyle.Bold
        );
        richTextBoxSummary.AppendText(text: roleForm.Roles);
        richTextBoxSummary.SelectionFont = new Font(
            prototype: richTextBoxSummary.Font,
            newStyle: FontStyle.Regular
        );
        richTextBoxSummary.AppendText(text: " for ");
        richTextBoxSummary.SelectionFont = new Font(
            prototype: richTextBoxSummary.Font,
            newStyle: FontStyle.Italic
        );
        richTextBoxSummary.AppendText(text: roleForm.NameOfMenu);
    }
}
