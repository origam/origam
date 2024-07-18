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
/// Summary description for CreateFieldWithLookupRelationshipEntityCommand.
/// </summary>
public class CreateFieldWithRelationshipEntityCommand : AbstractMenuCommand
{
    CreateFieldWithRelationshipEntityWizardForm wizardForm;
    SchemaBrowser _schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
    public override bool IsEnabled
	{
		get
		{
            return Owner is IDataEntity
                || Owner is IDataEntityColumn;
		}
		set
		{
			throw new ArgumentException("Cannot set this property", "IsEnabled");
		}
	}
	public override void Run()
	{
        FieldMappingItem baseField = Owner as FieldMappingItem;
        IDataEntity baseEntity = Owner as IDataEntity;
        if (baseField != null)
        {
            baseEntity = baseField.ParentItem as IDataEntity;
        }
        //CreateFieldWithRelationshipEntityWizard wiz = new CreateFieldWithRelationshipEntityWizard
        //{
        //    Entity = baseEntity
        //};
        var list = new List<ListViewItem>();
        TableMappingItem table1 = new TableMappingItem();
        EntityRelationItem entityRelation = new EntityRelationItem();
        list.Add(new ListViewItem(table1.GetType().SchemaItemDescription().Name, table1.Icon));
        list.Add(new ListViewItem(entityRelation.GetType().SchemaItemDescription().Name, entityRelation.Icon));
        
        Stack stackPage = new Stack();
        stackPage.Push(PagesList.Finish);
        stackPage.Push(PagesList.SummaryPage);
        stackPage.Push(PagesList.FieldEntity);
        stackPage.Push(PagesList.StartPage);
        wizardForm = new CreateFieldWithRelationshipEntityWizardForm
        {
            Title = ResourceUtils.GetString("CreateFieldWithRelationshipEntityWizardTitle"),
            PageTitle = "",
            Description = ResourceUtils.GetString("CreateFieldWithRelationshipEntityWizardDescription"),
            Entity = baseEntity,
            ItemTypeList = list,
            Pages = stackPage,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            Command = this,
            EnterAllInfo = ResourceUtils.GetString("EnterAllInfo"),
            LookupWiz = ResourceUtils.GetString("LookupWiz")
        };
        Wizard wiz = new Wizard(wizardForm);
        if (wiz.ShowDialog() != DialogResult.OK)
        {
            GeneratedModelElements.Clear();
        }
    }
    public override void Execute()
    {
        IDataEntity baseEntity = GetIDataEntity();
        // 1. entity
        TableMappingItem table = (TableMappingItem)baseEntity;
        GeneratedModelElements.Add(table);
        EntityRelationItem relation = EntityHelper.CreateRelation(table, (IDataEntity)wizardForm.RelatedEntity, wizardForm.ParentChildCheckbox, true);
        EntityHelper.CreateRelationKey(relation,
            (AbstractDataEntityColumn)wizardForm.BaseEntityFieldSelect,
            (AbstractDataEntityColumn)wizardForm.RelatedEntityFieldSelect, true,wizardForm.LookupKeyName);
        GeneratedModelElements.Add(relation);
    }
    private IDataEntity GetIDataEntity()
    {
        FieldMappingItem baseField = Owner as FieldMappingItem;
        IDataEntity baseEntity = Owner as IDataEntity;
        if (baseField != null)
        {
            baseEntity = baseField.ParentItem as IDataEntity;
        }
        return baseEntity;
    }
    public override int GetImageIndex(string icon)
    {
        return _schemaBrowser.ImageIndex(icon);
    }
    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text = "This Wizard create Field With Relationship Entity with this parameters:";
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Table: \t\t\t");
        richTextBoxSummary.AppendText(GetIDataEntity().Name);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Parent Child:");
        richTextBoxSummary.AppendText("\t\t" + wizardForm.ParentChildCheckbox.ToString());
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Related Entity:");
        richTextBoxSummary.AppendText("\t\t" + ((IDataEntity)wizardForm.RelatedEntity).Name);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Lookup Key Name:");
        richTextBoxSummary.AppendText("\t" + wizardForm.LookupKeyName);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Base Entity Field:");
        richTextBoxSummary.AppendText("\t\t" + wizardForm.BaseEntityFieldSelect.Name);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Related Entity Field:");
        richTextBoxSummary.AppendText("\t" + wizardForm.RelatedEntityFieldSelect.Name);
        richTextBoxSummary.AppendText(Environment.NewLine);
    }
}
