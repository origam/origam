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
    WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;
    SchemaBrowser _schemaBrowser =
        WorkbenchSingleton.Workbench.GetPad(type: typeof(SchemaBrowser)) as SchemaBrowser;
    ChildEntityForm childEntityForm;
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
        TableMappingItem table = new TableMappingItem();
        DataEntityIndex entityIndex = new DataEntityIndex();
        EntityRelationItem entityRelation = new EntityRelationItem();
        list.Add(
            item: new ListViewItem(
                text: table.GetType().SchemaItemDescription().Name,
                imageKey: table.Icon
            )
        );
        list.Add(
            item: new ListViewItem(
                text: entityIndex.GetType().SchemaItemDescription().Name,
                imageKey: entityIndex.Icon
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
        stackPage.Push(obj: PagesList.ChildEntity);
        stackPage.Push(obj: PagesList.StartPage);
        childEntityForm = new ChildEntityForm()
        {
            ItemTypeList = list,
            Title = ResourceUtils.GetString(key: "CreateChildEntityWizardTitle"),
            PageTitle = "",
            Description = ResourceUtils.GetString(key: "CreateChildEntityWizardDescription"),
            Pages = stackPage,
            Entity1 = entity,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            Command = this,
            EnterAllInfo = ResourceUtils.GetString(key: "EnterAllInfo"),
            ChildEntityWiz = ResourceUtils.GetString(key: "ChildEntityWiz"),
        };
        Wizard wiz = new Wizard(objectForm: childEntityForm);
        if (wiz.ShowDialog() != DialogResult.OK)
        {
            GeneratedModelElements.Clear();
        }
    }

    public override void Execute()
    {
        IDataEntity entity1 = childEntityForm.Entity1;
        // 1. Create N:N Entity with reference to both entities
        TableMappingItem newEntity = EntityHelper.CreateTable(
            name: childEntityForm.EntityName,
            group: childEntityForm.Entity1.Group,
            persist: false
        );
        newEntity.Persist();
        GeneratedModelElements.Add(item: newEntity);
        // Create index by parent entity
        DataEntityIndex index = newEntity.NewItem<DataEntityIndex>(
            schemaExtensionId: _schema.ActiveSchemaExtensionId,
            group: null
        );
        index.Name = "ix_" + entity1.Name;
        index.Persist();
        GeneratedModelElements.Add(item: index);
        // Create relation from the parent entity
        EntityRelationItem parentRelation = EntityHelper.CreateRelation(
            parentEntity: entity1,
            relatedEntity: newEntity,
            masterDetail: true,
            persist: true
        );
        GeneratedModelElements.Add(item: parentRelation);
        var entity1keys = new List<FieldMappingItem>();
        // Create reference columns
        foreach (IDataEntityColumn pk in entity1.EntityPrimaryKey)
        {
            if (!pk.ExcludeFromAllFields)
            {
                FieldMappingItem refEntity1 = EntityHelper.CreateColumn(
                    entity: newEntity,
                    name: "ref" + entity1.Name + pk.Name,
                    allowNulls: false,
                    dataType: pk.DataType,
                    dataLength: pk.DataLength,
                    caption: entity1.Caption,
                    foreignKeyEntity: entity1,
                    foreignKeyField: pk,
                    persist: true
                );
                EntityRelationColumnPairItem key = EntityHelper.CreateRelationKey(
                    relation: parentRelation,
                    baseField: pk,
                    relatedField: refEntity1,
                    persist: true
                );
                entity1keys.Add(item: refEntity1);
            }
        }
        if (childEntityForm.Entity2 != null)
        {
            foreach (IDataEntityColumn pk in childEntityForm.Entity2.EntityPrimaryKey)
            {
                if (!pk.ExcludeFromAllFields)
                {
                    EntityHelper.CreateColumn(
                        entity: newEntity,
                        name: "ref" + childEntityForm.Entity2.Name + pk.Name,
                        allowNulls: false,
                        dataType: pk.DataType,
                        dataLength: pk.DataLength,
                        caption: childEntityForm.Entity2.Caption,
                        foreignKeyEntity: childEntityForm.Entity2,
                        foreignKeyField: pk,
                        persist: true
                    );
                }
            }
        }
        int i = 0;
        foreach (FieldMappingItem col in entity1keys)
        {
            DataEntityIndexField field = index.NewItem<DataEntityIndexField>(
                schemaExtensionId: _schema.ActiveSchemaExtensionId,
                group: null
            );
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
        return _schemaBrowser.ImageIndex(icon: icon);
    }

    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text =
            ResourceUtils.GetString(key: "CreateChildEntityWizardDescription")
            + " with this parameters:";
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Child Entity: \t");
        richTextBoxSummary.AppendText(text: childEntityForm.EntityName);
    }
}
