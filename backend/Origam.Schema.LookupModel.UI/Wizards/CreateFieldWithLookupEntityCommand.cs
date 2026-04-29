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
using System.Text;
using System.Windows.Forms;
using Origam.DA.Service;
using Origam.Schema.DeploymentModel;
using Origam.Schema.EntityModel;
using Origam.UI;
using Origam.UI.WizardForm;
using Origam.Workbench;

namespace Origam.Schema.LookupModel.UI.Wizards;

/// <summary>
/// Summary description for CreateLookupFromEntityCommand.
/// </summary>
public class CreateFieldWithLookupEntityCommand : AbstractMenuCommand
{
    SchemaBrowser _schemaBrowser =
        WorkbenchSingleton.Workbench.GetPad(type: typeof(SchemaBrowser)) as SchemaBrowser;
    CreateFieldWithLookupEntityWizardForm createFieldWith;
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
        var list = new List<ListViewItem>();
        TableMappingItem table1 = new TableMappingItem();
        FieldMappingItem fieldMapping = new FieldMappingItem();
        DataServiceDataLookup data = new DataServiceDataLookup();
        list.Add(
            item: new ListViewItem(
                text: table1.GetType().SchemaItemDescription().Name,
                imageKey: table1.Icon
            )
        );
        list.Add(
            item: new ListViewItem(
                text: fieldMapping.GetType().SchemaItemDescription().Name,
                imageKey: fieldMapping.Icon
            )
        );
        list.Add(
            item: new ListViewItem(
                text: data.GetType().SchemaItemDescription().Name,
                imageKey: data.Icon
            )
        );
        Stack stackPage = new Stack();
        stackPage.Push(obj: PagesList.Finish);
        stackPage.Push(obj: PagesList.SummaryPage);
        stackPage.Push(obj: PagesList.FieldLookup);
        stackPage.Push(obj: PagesList.StartPage);
        createFieldWith = new CreateFieldWithLookupEntityWizardForm
        {
            Title = ResourceUtils.GetString(key: "CreateFieldWithLookupEntityWizard"),
            PageTitle = "",
            Description = ResourceUtils.GetString(
                key: "CreateFieldWithLookupEntityWizardDescription"
            ),
            ItemTypeList = list,
            Pages = stackPage,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            Command = this,
        };
        createFieldWith.EnterAllInfo = ResourceUtils.GetString(key: "EnterAllInfo");
        createFieldWith.LookupWiz = ResourceUtils.GetString(key: "LookupWiz");
        createFieldWith.DefaultValueNotSet = ResourceUtils.GetString(key: "DefaultValueNotSet");
        if (baseField != null)
        {
            createFieldWith.ForceTwoColumns = true;
            createFieldWith.AllowNulls = baseField.AllowNulls;
        }
        createFieldWith.NameFieldName = "Name";
        createFieldWith.NameFieldCaption = ResourceUtils.GetString(
            key: "LookupWizardNameFieldLabel"
        );
        createFieldWith.KeyFieldName = "Code";
        createFieldWith.KeyFieldCaption = ResourceUtils.GetString(
            key: "LookupWizardCodeFieldLabel"
        );
        Wizard wiz = new Wizard(objectForm: createFieldWith);
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
        TableMappingItem table = CreateLookupEntity(
            LookupName: createFieldWith.LookupName,
            baseEntity: baseEntity,
            baseField: baseField
        );
        // 2. field "Name"
        FieldMappingItem nameField = EntityHelper.CreateColumn(
            entity: table,
            name: createFieldWith.NameFieldName,
            allowNulls: false,
            dataType: OrigamDataType.String,
            dataLength: 200,
            caption: createFieldWith.NameFieldCaption,
            foreignKeyEntity: null,
            foreignKeyField: null,
            persist: true
        );
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
            codeField = EntityHelper.CreateColumn(
                entity: table,
                name: createFieldWith.KeyFieldName,
                allowNulls: false,
                dataType: dataType,
                dataLength: dataLength,
                databaseType: databaseType,
                caption: createFieldWith.KeyFieldCaption,
                foreignKeyEntity: null,
                foreignKeyField: null,
                persist: false
            );
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
            idFilter = EntityHelper.CreateFilter(
                field: idField,
                functionName: "Equal",
                filterPrefix: "GetBy",
                createParameter: true
            );
        }
        // 3. lookup
        DataServiceDataLookup lookup = LookupHelper.CreateDataServiceLookup(
            name: table.Name,
            fromEntity: table,
            idField: idField,
            nameField: nameField,
            codeField: codeField,
            idFilter: idFilter,
            listFilter: null,
            listDisplayMember: listDisplayMember
        );
        GeneratedModelElements.Add(item: lookup);
        // 4. foreign key field
        FieldMappingItem fk = null;
        if (baseField == null)
        {
            fk = EntityHelper.CreateForeignKey(
                name: "ref" + table.Name + idField.Name,
                caption: createFieldWith.LookupCaption,
                allowNulls: createFieldWith.AllowNulls,
                masterEntity: baseEntity,
                foreignEntity: table,
                foreignField: idField,
                lookup: lookup,
                persist: true
            );
            GeneratedModelElements.Add(item: fk);
        }
        else
        {
            fk = baseField;
            fk.ForeignKeyEntity = table;
            fk.ForeignKeyField = idField;
            fk.DefaultLookup = lookup;
        }
        // create a unique index on the Name field
        EntityHelper.CreateIndex(entity: table, field: nameField, unique: true, persist: true);
        // we do not create a unique index on the Code field
        // if the code field is a primary key
        if (createFieldWith.TwoColumns && baseField == null)
        {
            EntityHelper.CreateIndex(entity: table, field: codeField, unique: true, persist: true);
        }
        // 5. new table script
        ServiceCommandUpdateScriptActivity script1 = CreateTableScript(
            name: table.Name,
            guid: table.Id
        );
        GeneratedModelElements.Add(item: script1);
        // 6. initial values
        if (createFieldWith.InitialValues.Count > 0)
        {
            DataConstant defaultConstant = null;
            IDictionary<AbstractSqlDataService, StringBuilder> dict = InitDictionary();
            foreach (var initialValue in createFieldWith.InitialValues)
            {
                string constantName =
                    table.Name + "_" + initialValue.Name.Replace(oldValue: " ", newValue: "_");
                string pkValue = Guid.NewGuid().ToString();
                if (createFieldWith.TwoColumns)
                {
                    if (baseField == null)
                    {
                        foreach (KeyValuePair<AbstractSqlDataService, StringBuilder> item in dict)
                        {
                            item.Value.AppendFormat(
                                format: item.Key.CreateInsert(fieldCount: 3),
                                args: new object[]
                                {
                                    table.Name,
                                    idField.Name,
                                    createFieldWith.KeyFieldName,
                                    createFieldWith.NameFieldName,
                                    pkValue,
                                    initialValue.Code,
                                    initialValue.Name,
                                }
                            );
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<AbstractSqlDataService, StringBuilder> item in dict)
                        {
                            item.Value.AppendFormat(
                                format: item.Key.CreateInsert(fieldCount: 2),
                                args: new object[]
                                {
                                    table.Name,
                                    createFieldWith.KeyFieldName,
                                    createFieldWith.NameFieldName,
                                    initialValue.Code,
                                    initialValue.Name,
                                }
                            );
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<AbstractSqlDataService, StringBuilder> item in dict)
                    {
                        item.Value.Clear()
                            .AppendFormat(
                                format: item.Key.CreateInsert(fieldCount: 2),
                                args: new object[]
                                {
                                    table.Name,
                                    idField.Name,
                                    nameField.Name,
                                    pkValue,
                                    initialValue.Name,
                                }
                            );
                    }
                }
                DataConstant c = EntityHelper.CreateConstant(
                    name: constantName,
                    lookup: lookup,
                    dataType: idField.DataType,
                    value: pkValue,
                    group: EntityHelper.GetDataConstantGroup(name: baseEntity.Group.Name),
                    persist: true
                );
                GeneratedModelElements.Add(item: c);
                if (initialValue.IsDefault)
                {
                    defaultConstant = c;
                }
                fk.DefaultValue = defaultConstant;
                fk.Persist();
                foreach (KeyValuePair<AbstractSqlDataService, StringBuilder> item in dict)
                {
                    var script2 = DeploymentHelper.CreateDatabaseScript(
                        name: table.Name + "_values",
                        script: item.Value.ToString(),
                        platformName: item.Key.PlatformName
                    );
                    GeneratedModelElements.Add(item: script2);
                }
            }
            // 7. new field script (after values because of a default value
            // only if it's not a virtual detached entity
            foreach (KeyValuePair<AbstractSqlDataService, StringBuilder> item in dict)
            {
                string[] fkDdl = item.Key.FieldDdl(fieldId: fk.Id);
                int i = 0;
                foreach (var ddl in fkDdl)
                {
                    // if the foreign key is based on an existing field
                    // take only the foreign key ddl
                    if (baseField == null || i == 1)
                    {
                        var script3 = DeploymentHelper.CreateDatabaseScript(
                            name: baseEntity.Name + "_" + fk.Name,
                            script: ddl,
                            platformName: item.Key.PlatformName
                        );
                        GeneratedModelElements.Add(item: script3);
                    }
                    i++;
                }
            }
        }
    }

    public override int GetImageIndex(string icon)
    {
        return _schemaBrowser.ImageIndex(icon: icon);
    }

    private TableMappingItem CreateLookupEntity(
        string LookupName,
        IDataEntity baseEntity,
        IDataEntityColumn baseField
    )
    {
        bool createAncestor = baseField == null;
        TableMappingItem table = EntityHelper.CreateTable(
            name: LookupName,
            group: baseEntity.Group,
            persist: false,
            useDefaultAncestor: createAncestor
        );
        table.Persist();
        GeneratedModelElements.Add(item: table);
        return table;
    }

    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text = "This Wizard will create a lookup with these parameters:";
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Lookup Entity Name: \t\t");
        richTextBoxSummary.AppendText(text: createFieldWith.NameFieldName);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Caption: \t\t\t");
        richTextBoxSummary.AppendText(text: createFieldWith.LookupCaption);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Allow nulls: \t\t\t");
        richTextBoxSummary.AppendText(text: createFieldWith.AllowNulls.ToString());
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Two-column: \t\t\t");
        richTextBoxSummary.AppendText(text: createFieldWith.TwoColumns.ToString());
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Name Field Name: \t\t");
        richTextBoxSummary.AppendText(text: createFieldWith.NameFieldName);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Name Field Name Caption: \t");
        richTextBoxSummary.AppendText(text: createFieldWith.NameFieldCaption);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Initial Values:");
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "\t\tName\t\tDefault");
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        foreach (var row in createFieldWith.InitialValues)
        {
            richTextBoxSummary.AppendText(
                text: "\t\t" + row.Name + "\t\t" + row.IsDefault.ToString()
            );
            richTextBoxSummary.AppendText(text: Environment.NewLine);
        }
    }
}
