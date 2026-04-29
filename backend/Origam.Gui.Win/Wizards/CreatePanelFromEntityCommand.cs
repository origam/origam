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
using System.Linq;
using System.Windows.Forms;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.UI;
using Origam.UI.WizardForm;
using Origam.Workbench;
using Origam.Workbench.Commands;

namespace Origam.Gui.Win.Wizards;

/// <summary>
/// Summary description for CreatePanelFromEntityCommand.
/// </summary>
public class CreatePanelFromEntityCommand : AbstractMenuCommand
{
    SchemaBrowser _schemaBrowser =
        WorkbenchSingleton.Workbench.GetPad(type: typeof(SchemaBrowser)) as SchemaBrowser;
    ScreenWizardForm screenwizardForm;
    PanelControlSet panel;
    public override bool IsEnabled
    {
        get { return Owner is IDataEntity; }
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
        List<string> listdsName = GetListDatastructure(
            itemTypeConst: PanelControlSet.CategoryConst
        );
        var list = new List<ListViewItem>();
        PanelControlSet pp = new PanelControlSet();
        list.Add(
            item: new ListViewItem(
                text: pp.GetType().SchemaItemDescription().Name,
                imageKey: pp.Icon
            )
        );

        Stack stackPage = new Stack();
        stackPage.Push(obj: PagesList.Finish);
        stackPage.Push(obj: PagesList.SummaryPage);
        stackPage.Push(obj: PagesList.ScreenForm);
        if (listdsName.Any(predicate: name => name == (Owner as IDataEntity).Name))
        {
            stackPage.Push(obj: PagesList.StructureNamePage);
        }
        stackPage.Push(obj: PagesList.StartPage);
        screenwizardForm = new ScreenWizardForm
        {
            ItemTypeList = list,
            Title = ResourceUtils.GetString(key: "CreatePanelFromEntityWizardTitle"),
            PageTitle = "",
            Description = ResourceUtils.GetString(key: "CreatePanelFromEntityWizardDescription"),
            Pages = stackPage,
            StructureList = listdsName,
            Entity = Owner as IDataEntity,
            NameOfEntity = (Owner as IDataEntity).Name,
            IsRoleVisible = false,
            textColumnsOnly = false,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            Command = this,
        };
        Wizard wiz = new Wizard(objectForm: screenwizardForm);
        if (wiz.ShowDialog() == DialogResult.OK)
        {
            EditSchemaItem edit = new EditSchemaItem { Owner = panel };
            edit.Run();
            _schemaBrowser.EbrSchemaBrowser.SelectItem(item: panel);
        }
        else
        {
            GeneratedModelElements.Clear();
        }
    }

    public override void Execute()
    {
        string groupName = null;
        if (screenwizardForm.Entity.Group != null)
        {
            groupName = screenwizardForm.Entity.Group.Name;
        }

        panel = GuiHelper.CreatePanel(
            groupName: groupName,
            entity: screenwizardForm.Entity,
            fieldsToPopulate: screenwizardForm.SelectedFieldNames,
            name: screenwizardForm.NameOfEntity
        );
        GeneratedModelElements.Add(item: panel);
    }

    public override int GetImageIndex(string icon)
    {
        return _schemaBrowser.ImageIndex(icon: icon);
    }

    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text =
            "This Wizard will create a Screen from an Entity with these parameters:";
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Datastructure: \t\t");
        richTextBoxSummary.AppendText(text: screenwizardForm.NameOfEntity);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        ShowListItems(
            richTextBoxSummary: richTextBoxSummary,
            selectedFieldNames: screenwizardForm.SelectedFieldNames
        );
    }
}
