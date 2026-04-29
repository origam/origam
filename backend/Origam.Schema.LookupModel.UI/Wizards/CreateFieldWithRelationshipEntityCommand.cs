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
    SchemaBrowser _schemaBrowser =
        WorkbenchSingleton.Workbench.GetPad(type: typeof(SchemaBrowser)) as SchemaBrowser;
    public override bool IsEnabled
    {
        get { return Owner is IDataEntity || Owner is IDataEntityColumn; }
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
        list.Add(
            item: new ListViewItem(
                text: table1.GetType().SchemaItemDescription().Name,
                imageKey: table1.Icon
            )
        );
        list.Add(
            item: new ListViewItem(
                text: entityRelation.GetType().SchemaItemDescription().Name,
                imageKey: entityRelation.Icon
            )
        );

        Stack stackPage = new Stack();
        stackPage.Push(obj: PagesList.Finish);
        stackPage.Push(obj: PagesList.SummaryPage);
        stackPage.Push(obj: PagesList.FieldEntity);
        stackPage.Push(obj: PagesList.StartPage);
        wizardForm = new CreateFieldWithRelationshipEntityWizardForm
        {
            Title = ResourceUtils.GetString(key: "CreateFieldWithRelationshipEntityWizardTitle"),
            PageTitle = "",
            Description = ResourceUtils.GetString(
                key: "CreateFieldWithRelationshipEntityWizardDescription"
            ),
            Entity = baseEntity,
            ItemTypeList = list,
            Pages = stackPage,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            Command = this,
            EnterAllInfo = ResourceUtils.GetString(key: "EnterAllInfo"),
            LookupWiz = ResourceUtils.GetString(key: "LookupWiz"),
        };
        Wizard wiz = new Wizard(objectForm: wizardForm);
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
        GeneratedModelElements.Add(item: table);
        EntityRelationItem relation = EntityHelper.CreateRelation(
            parentEntity: table,
            relatedEntity: (IDataEntity)wizardForm.RelatedEntity,
            masterDetail: wizardForm.ParentChildCheckbox,
            persist: true
        );
        EntityHelper.CreateRelationKey(
            relation: relation,
            baseField: (AbstractDataEntityColumn)wizardForm.BaseEntityFieldSelect,
            relatedField: (AbstractDataEntityColumn)wizardForm.RelatedEntityFieldSelect,
            persist: true,
            nameOfKey: wizardForm.LookupKeyName
        );
        GeneratedModelElements.Add(item: relation);
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
        return _schemaBrowser.ImageIndex(icon: icon);
    }

    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text =
            "This Wizard will create a Field With a Relationship Entity with these parameters:";
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Table: \t\t\t");
        richTextBoxSummary.AppendText(text: GetIDataEntity().Name);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Parent Child:");
        richTextBoxSummary.AppendText(text: "\t\t" + wizardForm.ParentChildCheckbox.ToString());
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Related Entity:");
        richTextBoxSummary.AppendText(text: "\t\t" + ((IDataEntity)wizardForm.RelatedEntity).Name);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Lookup Key Name:");
        richTextBoxSummary.AppendText(text: "\t" + wizardForm.LookupKeyName);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Base Entity Field:");
        richTextBoxSummary.AppendText(text: "\t\t" + wizardForm.BaseEntityFieldSelect.Name);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Related Entity Field:");
        richTextBoxSummary.AppendText(text: "\t" + wizardForm.RelatedEntityFieldSelect.Name);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
    }
}
