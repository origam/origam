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
        WorkbenchSingleton.Workbench.GetPad(type: typeof(SchemaBrowser)) as SchemaBrowser;
    ForeignKeyForm keyForm;
    FieldMappingItem fk;
    public override bool IsEnabled
    {
        get { return Owner is IDataEntity; }
        set
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(key: "ErrorSetProperty"),
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        IDataEntity entity = Owner as IDataEntity;
        var list = new List<ListViewItem>();
        FieldMappingItem fmItem = new FieldMappingItem();
        list.Add(
            item: new ListViewItem(
                text: fmItem.GetType().SchemaItemDescription().Name,
                imageKey: fmItem.Icon
            )
        );
        Stack stackPage = new Stack();
        stackPage.Push(obj: PagesList.Finish);
        stackPage.Push(obj: PagesList.SummaryPage);
        stackPage.Push(obj: PagesList.ForeignForm);
        stackPage.Push(obj: PagesList.StartPage);
        keyForm = new ForeignKeyForm
        {
            ItemTypeList = list,
            Title = ResourceUtils.GetString(key: "CreateForeignKeyWizardTitle"),
            PageTitle = "",
            Description = ResourceUtils.GetString(key: "CreateForeignKeyWizardDescription"),
            Pages = stackPage,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            Command = this,
            SelectForeignEntity = ResourceUtils.GetString(key: "SelectForeignEntity"),
            ForeignKeyWiz = ResourceUtils.GetString(key: "ForeignKeyWiz"),
            SelectForeignField = ResourceUtils.GetString(key: "SelectForeignField"),
            EnterKeyName = ResourceUtils.GetString(key: "EnterKeyName"),
            MasterEntity = entity,
        };
        Wizard wiz = new Wizard(objectForm: keyForm);
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
            name: keyForm.ForeignKeyName,
            caption: keyForm.Caption,
            allowNulls: keyForm.AllowNulls,
            masterEntity: keyForm.MasterEntity,
            foreignEntity: keyForm.ForeignEntity,
            foreignField: keyForm.ForeignField,
            lookup: keyForm.Lookup,
            persist: true
        );
        GeneratedModelElements.Add(item: fk);
    }

    public override int GetImageIndex(string icon)
    {
        return _schemaBrowser.ImageIndex(icon: icon);
    }

    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text =
            ResourceUtils.GetString(key: "CreateForeignKeyWizardDescription")
            + " with this parameters:";
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Master Entity: \t");
        richTextBoxSummary.AppendText(text: keyForm.MasterEntity.Name);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Foreign Entity: \t");
        richTextBoxSummary.AppendText(text: keyForm.ForeignEntity.Name);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Foreign Field: \t");
        richTextBoxSummary.AppendText(text: keyForm.ForeignField.Name);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Foreign Key: \t");
        richTextBoxSummary.AppendText(text: keyForm.ForeignKeyName);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Lookup: \t\t");
        richTextBoxSummary.AppendText(text: keyForm.Lookup == null ? "" : keyForm.Lookup.Name);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Caption : \t");
        richTextBoxSummary.AppendText(text: keyForm.Caption);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Allow null : \t");
        richTextBoxSummary.AppendText(text: keyForm.AllowNulls.ToString());
    }
}
