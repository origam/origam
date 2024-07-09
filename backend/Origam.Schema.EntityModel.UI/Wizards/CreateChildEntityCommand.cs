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
using Origam.Workbench.Services;

namespace Origam.Schema.EntityModel.UI.Wizards;
/// <summary>
/// Summary description for CreateNtoNEntityCommand.
/// </summary>
public class CreateChildEntityCommand : AbstractMenuCommand
{
	WorkbenchSchemaService _schema = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;
    SchemaBrowser _schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
    ChildEntityForm childEntityForm;
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
		IDataEntity entity = Owner as IDataEntity;
        var list = new List<ListViewItem>();
        TableMappingItem table = new TableMappingItem();
        DataEntityIndex entityIndex = new DataEntityIndex();
        EntityRelationItem entityRelation = new EntityRelationItem();
        list.Add(new ListViewItem(table.GetType().SchemaItemDescription().Name, table.Icon));
        list.Add(new ListViewItem(entityIndex.GetType().SchemaItemDescription().Name, entityIndex.Icon));
        list.Add(new ListViewItem(entityRelation.GetType().SchemaItemDescription().Name, entityRelation.Icon));
       
        Stack stackPage = new Stack();
        stackPage.Push(PagesList.Finish);
        stackPage.Push(PagesList.SummaryPage);
        stackPage.Push(PagesList.ChildEntity);
        stackPage.Push(PagesList.StartPage);
        childEntityForm = new ChildEntityForm()
        {
            ItemTypeList = list,
            Title = ResourceUtils.GetString("CreateChildEntityWizardTitle"),
            PageTitle = "",
            Description = ResourceUtils.GetString("CreateChildEntityWizardDescription"),
            Pages = stackPage,
            Entity1 = entity,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            Command = this,
            EnterAllInfo = ResourceUtils.GetString("EnterAllInfo"),
            ChildEntityWiz = ResourceUtils.GetString("ChildEntityWiz")
        };
        Wizard wiz = new Wizard(childEntityForm);
        if (wiz.ShowDialog() != DialogResult.OK)
		{
            GeneratedModelElements.Clear();
        }
	}
    public override void Execute()
    {
        IDataEntity entity1 = childEntityForm.Entity1;
        // 1. Create N:N Entity with reference to both entities
        TableMappingItem newEntity = EntityHelper.CreateTable(childEntityForm.EntityName, childEntityForm.Entity1.Group, false);
        newEntity.Persist();
        GeneratedModelElements.Add(newEntity);
        // Create index by parent entity
        DataEntityIndex index = newEntity.NewItem<DataEntityIndex>(
            _schema.ActiveSchemaExtensionId, null);
        index.Name = "ix_" + entity1.Name;
        index.Persist();
        GeneratedModelElements.Add(index);
        // Create relation from the parent entity
        EntityRelationItem parentRelation = EntityHelper.CreateRelation(entity1, newEntity, true, true);
        GeneratedModelElements.Add(parentRelation);
        var entity1keys = new List<FieldMappingItem>();
        // Create reference columns
        foreach (IDataEntityColumn pk in entity1.EntityPrimaryKey)
        {
            if (!pk.ExcludeFromAllFields)
            {
                FieldMappingItem refEntity1 = EntityHelper.CreateColumn(newEntity, "ref" + entity1.Name + pk.Name, false, pk.DataType, pk.DataLength, entity1.Caption, entity1, pk, true);
                EntityRelationColumnPairItem key = EntityHelper.CreateRelationKey(parentRelation, pk, refEntity1, true);
                entity1keys.Add(refEntity1);
            }
        }
        if (childEntityForm.Entity2 != null)
        {
            foreach (IDataEntityColumn pk in childEntityForm.Entity2.EntityPrimaryKey)
            {
                if (!pk.ExcludeFromAllFields)
                {
                    EntityHelper.CreateColumn(newEntity, "ref" + childEntityForm.Entity2.Name + pk.Name, false, 
                        pk.DataType, pk.DataLength, childEntityForm.Entity2.Caption, childEntityForm.Entity2, pk, true);
                }
            }
        }
        int i = 0;
        foreach (FieldMappingItem col in entity1keys)
        {
            DataEntityIndexField field 
                = index.NewItem<DataEntityIndexField>(
                    _schema.ActiveSchemaExtensionId, null);
            field.Field = col;
            field.OrdinalPosition = i;
            field.Persist();
            i++;
        }
        newEntity.Persist(); 
        entity1.Persist();
    }
    public override int GetImageIndex(string icon)
    {
        return _schemaBrowser.ImageIndex(icon);
    }
    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text = ResourceUtils.GetString("CreateChildEntityWizardDescription") + " with this parameters:";
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Child Entity: \t");
        richTextBoxSummary.AppendText(childEntityForm.EntityName);
    }
}
