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
using System.Windows.Forms;
using Origam.Schema.EntityModel.Wizards;
using Origam.UI;
using Origam.UI.WizardForm;
using Origam.Workbench;
using Origam.Workbench.Commands;

namespace Origam.Schema.EntityModel.UI.Wizards;

/// <summary>
/// Summary description for CreateNtoNEntityCommand.
/// </summary>
public class CreateForeignKeyCommand : AbstractMenuCommand
{
    SchemaBrowser _schemaBrowser =
        WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
    ForeignKeyForm keyForm;
    FieldMappingItem fk;
    public override bool IsEnabled
    {
        get { return Owner is IDataEntity; }
        set
        {
            throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
        }
    }

    public override void Run()
    {
        IDataEntity entity = Owner as IDataEntity;
        var list = new List<ListViewItem>();
        FieldMappingItem fmItem = new FieldMappingItem();
        list.Add(new ListViewItem(fmItem.GetType().SchemaItemDescription().Name, fmItem.Icon));
        Stack stackPage = new Stack();
        stackPage.Push(PagesList.Finish);
        stackPage.Push(PagesList.SummaryPage);
        stackPage.Push(PagesList.ForeignForm);
        stackPage.Push(PagesList.StartPage);
        keyForm = new ForeignKeyForm
        {
            ItemTypeList = list,
            Title = ResourceUtils.GetString("CreateForeignKeyWizardTitle"),
            PageTitle = "",
            Description = ResourceUtils.GetString("CreateForeignKeyWizardDescription"),
            Pages = stackPage,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            Command = this,
            SelectForeignEntity = ResourceUtils.GetString("SelectForeignEntity"),
            ForeignKeyWiz = ResourceUtils.GetString("ForeignKeyWiz"),
            SelectForeignField = ResourceUtils.GetString("SelectForeignField"),
            EnterKeyName = ResourceUtils.GetString("EnterKeyName"),
            MasterEntity = entity,
        };
        Wizard wiz = new Wizard(keyForm);
        if (wiz.ShowDialog() == DialogResult.OK)
        {
            EditSchemaItem cmd = new EditSchemaItem();
            cmd.Owner = fk;
            cmd.Run();
        }
        else
        {
            GeneratedModelElements.Clear();
        }
    }

    public override void Execute()
    {
        fk = EntityHelper.CreateForeignKey(
            keyForm.ForeignKeyName,
            keyForm.Caption,
            keyForm.AllowNulls,
            keyForm.MasterEntity,
            keyForm.ForeignEntity,
            keyForm.ForeignField,
            keyForm.Lookup,
            true
        );
        GeneratedModelElements.Add(fk);
    }

    public override int GetImageIndex(string icon)
    {
        return _schemaBrowser.ImageIndex(icon);
    }

    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text =
            ResourceUtils.GetString("CreateForeignKeyWizardDescription") + " with this parameters:";
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Master Entity: \t");
        richTextBoxSummary.AppendText(keyForm.MasterEntity.Name);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Foreign Entity: \t");
        richTextBoxSummary.AppendText(keyForm.ForeignEntity.Name);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Foreign Field: \t");
        richTextBoxSummary.AppendText(keyForm.ForeignField.Name);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Foreign Key: \t");
        richTextBoxSummary.AppendText(keyForm.ForeignKeyName);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Lookup: \t\t");
        richTextBoxSummary.AppendText(keyForm.Lookup == null ? "" : keyForm.Lookup.Name);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Caption : \t");
        richTextBoxSummary.AppendText(keyForm.Caption);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Allow null : \t");
        richTextBoxSummary.AppendText(keyForm.AllowNulls.ToString());
    }
}
