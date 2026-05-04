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
using Origam.Schema.EntityModel;
using Origam.UI;
using Origam.UI.WizardForm;
using Origam.Workbench;

namespace Origam.Schema.LookupModel.UI.Wizards;

/// <summary>
/// Summary description for CreateLookupFromEntityCommand.
/// </summary>
public class CreateLookupFromEntityCommand : AbstractMenuCommand
{
    SchemaBrowser _schemaBrowser =
        WorkbenchSingleton.Workbench.GetPad(type: typeof(SchemaBrowser)) as SchemaBrowser;
    LookupForm lookupForm;
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
        DataServiceDataLookup dd = new DataServiceDataLookup();
        var dataStructure = new DataStructure();
        var list = new List<ListViewItem>
        {
            new ListViewItem(text: dd.GetType().SchemaItemDescription().Name, imageKey: dd.Icon),
            new ListViewItem(
                text: dataStructure.GetType().SchemaItemDescription().Name,
                imageKey: dataStructure.Icon
            ),
        };
        Stack stackPage = new Stack();
        stackPage.Push(obj: PagesList.Finish);
        stackPage.Push(obj: PagesList.SummaryPage);
        stackPage.Push(obj: PagesList.LookupForm);
        stackPage.Push(obj: PagesList.StartPage);
        lookupForm = new LookupForm
        {
            Title = ResourceUtils.GetString(key: "CreateLookupWiz"),
            PageTitle = "",
            Description = ResourceUtils.GetString(key: "CreateLookupWizDescription"),
            Pages = stackPage,
            Entity = Owner as IDataEntity,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            ItemTypeList = list,
            Command = this,
        };
        Wizard wiz = new Wizard(objectForm: lookupForm);
        //CreateLookupFromEntityWizard wizz = new CreateLookupFromEntityWizard();
        //wiz.Entity = Owner as IDataEntity;
        if (wiz.ShowDialog() != DialogResult.OK)
        {
            GeneratedModelElements.Clear();
        }
    }

    public override void Execute()
    {
        var result = LookupHelper.CreateDataServiceLookup(
            name: lookupForm.LookupName,
            fromEntity: lookupForm.Entity,
            idField: lookupForm.IdColumn,
            nameField: lookupForm.NameColumn,
            codeField: null,
            idFilter: lookupForm.IdFilter,
            listFilter: lookupForm.ListFilter,
            listDisplayMember: null
        );
        //var result = LookupHelper.CreateDataServiceLookup(
        //    wiz.LookupName, wiz.Entity, wiz.IdColumn, wiz.NameColumn,
        //    null, wiz.IdFilter, wiz.ListFilter, null);
        GeneratedModelElements.Add(item: result.ListDataStructure);
        GeneratedModelElements.Add(item: result);
    }

    public override int GetImageIndex(string icon)
    {
        return _schemaBrowser.ImageIndex(icon: icon);
    }

    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text = "This Wizard will create a lookup with these parameters:";
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Name: \t\t");
        richTextBoxSummary.AppendText(text: lookupForm.LookupName);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Display Field: \t");
        richTextBoxSummary.AppendText(text: lookupForm.NameColumn.Name);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "List Filter: \t");
        richTextBoxSummary.AppendText(
            text: lookupForm.ListFilter == null ? "none" : lookupForm.ListFilter.Name
        );
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Id Filter: \t\t");
        richTextBoxSummary.AppendText(text: lookupForm.IdFilter.Name);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "And this data structure:");
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Data structure: \t");
        richTextBoxSummary.AppendText(
            text: LookupHelper.GetDataStructureName(lookupName: lookupForm.LookupName)
        );
    }
}
