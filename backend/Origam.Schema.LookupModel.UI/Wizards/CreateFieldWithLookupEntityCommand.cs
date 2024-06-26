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
using System.Windows.Forms;
using Origam.Schema.EntityModel;
using Origam.UI;
using Origam.Schema.DeploymentModel;
using System.Text;
using System.Collections.Generic;
using Origam.DA.Service;
using Origam.Workbench;
using Origam.UI.WizardForm;
using System.Collections;
using System.Drawing;

namespace Origam.Schema.LookupModel.UI.Wizards;
/// <summary>
/// Summary description for CreateLookupFromEntityCommand.
/// </summary>
public class CreateFieldWithLookupEntityCommand : AbstractMenuCommand
{
    SchemaBrowser _schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
    CreateFieldWithLookupEntityWizardForm createFieldWith;
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
        ArrayList list = new ArrayList();
        TableMappingItem table1 = new TableMappingItem();
        FieldMappingItem fieldMapping = new FieldMappingItem();
        DataServiceDataLookup data = new DataServiceDataLookup();
        list.Add(new ListViewItem(table1.GetType().SchemaItemDescription().Name, table1.Icon));
        list.Add(new ListViewItem(fieldMapping.GetType().SchemaItemDescription().Name, fieldMapping.Icon));
        list.Add(new ListViewItem(data.GetType().SchemaItemDescription().Name, data.Icon));
        Stack stackPage = new Stack();
        stackPage.Push(PagesList.Finish);
        stackPage.Push(PagesList.SummaryPage);
        stackPage.Push(PagesList.FieldLookup);
        stackPage.Push(PagesList.StartPage);
        createFieldWith = new CreateFieldWithLookupEntityWizardForm
        {
            Title = ResourceUtils.GetString("CreateFieldWithLookupEntityWizard"),
            PageTitle = "",
            Description = ResourceUtils.GetString("CreateFieldWithLookupEntityWizardDescription"),
            ItemTypeList = list,
            Pages = stackPage,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            Command = this
        };
        createFieldWith.EnterAllInfo = ResourceUtils.GetString("EnterAllInfo");
        createFieldWith.LookupWiz = ResourceUtils.GetString("LookupWiz");
        createFieldWith.DefaultValueNotSet = ResourceUtils.GetString("DefaultValueNotSet");
        if (baseField != null)
        {
            createFieldWith.ForceTwoColumns = true;
            createFieldWith.AllowNulls = baseField.AllowNulls;
        }
        createFieldWith.NameFieldName = "Name";
        createFieldWith.NameFieldCaption = ResourceUtils.GetString("LookupWizardNameFieldLabel");
        createFieldWith.KeyFieldName = "Code";
        createFieldWith.KeyFieldCaption = ResourceUtils.GetString("LookupWizardCodeFieldLabel");
        Wizard wiz = new Wizard(createFieldWith);
        if (wiz.ShowDialog() != DialogResult.OK)
        {
            GeneratedModelElements.Clear();
        }
    }
    public override void Execute()
    {
        FieldMappingItem baseField = Owner as FieldMappingItem;
        string listDisplayMember = createFieldWith.NameFieldName;
        IDataEntity baseEntity = Owner as IDataEntity;
        if (baseField != null)
        {
            baseEntity = baseField.ParentItem as IDataEntity;
        }
        // 1. entity
        TableMappingItem table = CreateLookupEntity(createFieldWith.LookupName, baseEntity,
            baseField);
        // 2. field "Name"
        FieldMappingItem nameField = EntityHelper.CreateColumn(table,
            createFieldWith.NameFieldName, false, OrigamDataType.String, 200,
            createFieldWith.NameFieldCaption, null, null, true);
        FieldMappingItem codeField = null;
        // field "Code"
        if (createFieldWith.TwoColumns)
        {
            OrigamDataType dataType = OrigamDataType.String;
            int dataLength = 50;
            DatabaseDataType databaseType = null;
            if (baseField != null)
            {
                dataType = baseField.DataType;
                dataLength = baseField.DataLength;
                databaseType = baseField.MappedDataType;
            }
            codeField = EntityHelper.CreateColumn(table,
                createFieldWith.KeyFieldName, false, dataType, dataLength,
                databaseType, createFieldWith.KeyFieldCaption, null, null, false);
            if (baseField != null)
            {
                codeField.IsPrimaryKey = true;
            }
            codeField.Persist();
            listDisplayMember = createFieldWith.KeyFieldName + ";" + createFieldWith.NameFieldName;
        }
        IDataEntityColumn idField = EntityHelper.DefaultPrimaryKey;
        EntityFilter idFilter = EntityHelper.DefaultPrimaryKeyFilter;
        if (baseField != null)
        {
            idField = codeField;
            idFilter = EntityHelper.CreateFilter(idField, "Equal", "GetBy", true);
        }
        // 3. lookup
        DataServiceDataLookup lookup =
            LookupHelper.CreateDataServiceLookup(table.Name, table,
            idField, nameField, codeField, idFilter, null, listDisplayMember);
        GeneratedModelElements.Add(lookup);
        // 4. foreign key field
        FieldMappingItem fk = null;
        if (baseField == null)
        {
            fk = EntityHelper.CreateForeignKey("ref"
                + table.Name + idField.Name, createFieldWith.LookupCaption,
                createFieldWith.AllowNulls, baseEntity, table, idField, lookup, true);
            GeneratedModelElements.Add(fk);
        }
        else
        {
            fk = baseField;
            fk.ForeignKeyEntity = table;
            fk.ForeignKeyField = idField;
            fk.DefaultLookup = lookup;
        }
        // create a unique index on the Name field
        EntityHelper.CreateIndex(table, nameField, true, true);
        // we do not create a unique index on the Code field
        // if the code field is a primary key
        if (createFieldWith.TwoColumns && baseField == null)
        {
            EntityHelper.CreateIndex(table, codeField, true, true);
        }
        // 5. new table script
        ServiceCommandUpdateScriptActivity script1 = CreateTableScript(
            table.Name, table.Id);
        GeneratedModelElements.Add(script1);
        // 6. initial values
        if (createFieldWith.InitialValues.Count > 0)
        {
            DataConstant defaultConstant = null;
            IDictionary<AbstractSqlDataService, StringBuilder> dict = InitDictionary();
            foreach (var initialValue in createFieldWith.InitialValues)
            {
                string constantName = table.Name + "_" + initialValue.Name.Replace(" ", "_");
                string pkValue = Guid.NewGuid().ToString();
                if (createFieldWith.TwoColumns)
                {
                    if (baseField == null)
                    {
                        foreach (KeyValuePair<AbstractSqlDataService, StringBuilder> item in dict)
                        {
                            item.Value.AppendFormat(item.Key.CreateInsert(3),
                            table.Name, idField.Name, createFieldWith.KeyFieldName,
                            createFieldWith.NameFieldName, pkValue, initialValue.Code,
                            initialValue.Name);
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<AbstractSqlDataService, StringBuilder> item in dict)
                        {
                            item.Value.AppendFormat(item.Key.CreateInsert(2),
                            table.Name, createFieldWith.KeyFieldName,
                            createFieldWith.NameFieldName, initialValue.Code,
                            initialValue.Name);
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<AbstractSqlDataService, StringBuilder> item in dict)
                    {
                        item.Value.Clear().AppendFormat(item.Key.CreateInsert(2),
                        table.Name, idField.Name, nameField.Name, pkValue, initialValue.Name);
                    }
                }
                DataConstant c = EntityHelper.CreateConstant(
                    constantName, lookup, idField.DataType, pkValue,
                    EntityHelper.GetDataConstantGroup(baseEntity.Group.Name), true);
                GeneratedModelElements.Add(c);
                if (initialValue.IsDefault)
                {
                    defaultConstant = c;
                }
                fk.DefaultValue = defaultConstant;
                fk.Persist();
                foreach (KeyValuePair<AbstractSqlDataService, StringBuilder> item in dict)
                {
                    var script2 = DeploymentHelper
                        .CreateDatabaseScript(table.Name + "_values", item.Value.ToString(), item.Key.PlatformName);
                    GeneratedModelElements.Add(script2);
                }
            }
            // 7. new field script (after values because of a default value
            // only if it's not a virtual detached entity
            foreach (KeyValuePair<AbstractSqlDataService, StringBuilder> item in dict)
            {
                string[] fkDdl = item.Key.FieldDdl(fk.Id);
                int i = 0;
                foreach (var ddl in fkDdl)
                {
                    // if the foreign key is based on an existing field 
                    // take only the foreign key ddl
                    if (baseField == null || i == 1)
                    {
                        var script3 = DeploymentHelper.CreateDatabaseScript(baseEntity.Name + "_" + fk.Name, ddl,item.Key.PlatformName);
                        GeneratedModelElements.Add(script3);
                    }
                    i++;
                }
            }
        }
    }
    public override int GetImageIndex(string icon)
    {
        return _schemaBrowser.ImageIndex(icon);
    }
    private TableMappingItem CreateLookupEntity(
       string LookupName, IDataEntity baseEntity,
        IDataEntityColumn baseField)
    {
        bool createAncestor = baseField == null;
        TableMappingItem table = EntityHelper.CreateTable(
            LookupName, baseEntity.Group, false, createAncestor);
        table.Persist();
        GeneratedModelElements.Add(table);
        return table;
    }
    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text = "This Wizard create lookup with this parameters:";
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Lookup Entity Name: \t\t");
        richTextBoxSummary.AppendText(createFieldWith.NameFieldName);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Caption: \t\t\t");
        richTextBoxSummary.AppendText(createFieldWith.LookupCaption);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Allow nulls: \t\t\t");
        richTextBoxSummary.AppendText(createFieldWith.AllowNulls.ToString());
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Two-column: \t\t\t");
        richTextBoxSummary.AppendText(createFieldWith.TwoColumns.ToString()); 
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Name Field Name: \t\t");
        richTextBoxSummary.AppendText(createFieldWith.NameFieldName);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Name Field Name Caption: \t");
        richTextBoxSummary.AppendText(createFieldWith.NameFieldCaption);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("Initial Values:");
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText("\t\tName\t\tDefault");
        richTextBoxSummary.AppendText(Environment.NewLine);
        richTextBoxSummary.AppendText(Environment.NewLine);
        foreach (var row in createFieldWith.InitialValues)
        {
            richTextBoxSummary.AppendText("\t\t" + row.Name+"\t\t"+row.IsDefault.ToString());
            richTextBoxSummary.AppendText(Environment.NewLine);
        }
    }
}
