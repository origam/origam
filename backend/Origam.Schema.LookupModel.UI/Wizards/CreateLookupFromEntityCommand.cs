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
    SchemaBrowser _schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
    LookupForm lookupForm;
    public override bool IsEnabled
	{
		get
		{
			return Owner is IDataEntity;
		}
		set
		{
			throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
		}
	}
	public override void Run()
	{
        DataServiceDataLookup dd = new DataServiceDataLookup();
        var dataStructure = new DataStructure();
        var list = new List<ListViewItem>
        {
	        new ListViewItem(dd.GetType().SchemaItemDescription().Name,
		        dd.Icon),
	        new ListViewItem(
		        dataStructure.GetType().SchemaItemDescription().Name,
		        dataStructure.Icon)
        };
        Stack stackPage = new Stack();
        stackPage.Push(PagesList.Finish);
        stackPage.Push(PagesList.SummaryPage);
        stackPage.Push(PagesList.LookupForm);
        stackPage.Push(PagesList.StartPage);
        lookupForm = new LookupForm
        {
            Title = ResourceUtils.GetString("CreateLookupWiz"),
            PageTitle = "",
            Description = ResourceUtils.GetString("CreateLookupWizDescription"),
            Pages = stackPage,
            Entity = Owner as IDataEntity,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            ItemTypeList = list,
            Command = this
        };
        Wizard wiz = new Wizard(lookupForm);
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
                lookupForm.LookupName, lookupForm.Entity, lookupForm.IdColumn, lookupForm.NameColumn,
                null, lookupForm.IdFilter, lookupForm.ListFilter, null);
        //var result = LookupHelper.CreateDataServiceLookup(
        //    wiz.LookupName, wiz.Entity, wiz.IdColumn, wiz.NameColumn,
        //    null, wiz.IdFilter, wiz.ListFilter, null);
        GeneratedModelElements.Add(result.ListDataStructure);
        GeneratedModelElements.Add(result);
    }
    public override int GetImageIndex(string icon)
    {
        return _schemaBrowser.ImageIndex(icon);
    }
    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text = "This Wizard will create a lookup with these parameters:";
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Name: \t\t");
        richTextBoxSummary.AppendText(lookupForm.LookupName);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Display Field: \t");
        richTextBoxSummary.AppendText(lookupForm.NameColumn.Name);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("List Filter: \t");
        richTextBoxSummary.AppendText(lookupForm.ListFilter==null?"none": lookupForm.ListFilter.Name);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Id Filter: \t\t");
        richTextBoxSummary.AppendText(lookupForm.IdFilter.Name);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("And this data structure:");
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Data structure: \t");
        richTextBoxSummary.AppendText(LookupHelper.GetDataStructureName(lookupForm.LookupName));
    }
}
