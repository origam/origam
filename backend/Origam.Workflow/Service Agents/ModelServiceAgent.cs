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
using System.Collections.Generic;
using System.Data;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Service.Core;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Workflow;

public class ModelServiceAgent : AbstractServiceAgent
{
    const string GENERATED_PACKAGE_ID = "3cfc0308-fd23-454c-9d7a-a00054e0b9b1";

    private readonly TracingService tracingService =
        ServiceManager.Services.GetService<TracingService>();
    private readonly IPersistenceService persistenceService =
        ServiceManager.Services.GetService<IPersistenceService>();

    #region IServiceAgent Members
    private object result;
    public override object Result => result;

    public override void Run()
    {
        switch (MethodName)
        {
            case "GenerateSimpleModel":
            {
                result = GenerateSimpleModel(
                    dataDocument: Parameters.Get<IDataDocument>(key: "Data")
                );
                break;
            }
            case "GetDatabaseFieldsMetaData":
            {
                result = GetDatabaseFieldsMetaData();
                break;
            }
            case "ElementAttribute":
            {
                result = ElementAttribute(
                    id: Parameters.Get<Guid>(key: "Id"),
                    attributeName: Parameters.Get<string>(key: "AttributeName")
                );
                break;
            }
            case "ElementListByParent":
            {
                result = ElementList(
                    parentId: Parameters.Get<Guid>(key: "ParentId"),
                    itemType: Parameters.Get<string>(key: "ItemType")
                );
                break;
            }
            case "SetTrace":
            {
                tracingService.Enabled = Parameters.Get<bool>(key: "Enabled");
                result = tracingService.Enabled;
                break;
            }
            case "GetTrace":
            {
                result = tracingService.Enabled;
                break;
            }
            case "TraceObject":
            {
                Guid objectId = Parameters.Get<Guid>(key: "ObjectId");
                var traceable = persistenceService.SchemaProvider.RetrieveInstance<ITraceable>(
                    instanceId: objectId,
                    useCache: true
                );
                string traceType = Parameters.Get<string>(key: "TraceType");
                if (!Enum.TryParse(value: traceType, result: out Trace trace))
                {
                    throw new ArgumentException(
                        message: $"\"{traceType}\" cannot be parsed to {nameof(Trace)}"
                    );
                }
                traceable.TraceLevel = trace;
                persistenceService.SchemaProvider.Persist(obj: traceable as IPersistent);
                result = traceable.TraceLevel;
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(MethodName),
                    actualValue: MethodName,
                    message: ResourceUtils.GetString(key: "InvalidMethodName")
                );
            }
        }
    }
    #endregion
    #region Private Methods
    private object GenerateSimpleModel(IDataDocument dataDocument)
    {
        ISchemaService schemaService = ServiceManager.Services.GetService<ISchemaService>();
        schemaService.StorageSchemaExtensionId = new Guid(g: GENERATED_PACKAGE_ID);
        SimpleModelData model = dataDocument.DataSet as SimpleModelData;
        if (model == null)
        {
            throw new ArgumentOutOfRangeException(
                paramName: "Data",
                actualValue: dataDocument,
                message: "Unknown data"
            );
        }
        foreach (var entity in model.OrigamEntity)
        {
            var table = EntityHelper.CreateTable(name: entity.Name, group: null, persist: true);
            foreach (var field in entity.GetOrigamFieldRows())
            {
                EntityHelper.CreateColumn(
                    entity: table,
                    name: field.Name,
                    allowNulls: !field.IsMandatory,
                    dataType: ConvertType(origamDataTypeId: field.refOrigamDataTypeId.ToString()),
                    dataLength: field.IsTextLengthNull() ? 0 : field.TextLength,
                    caption: field.Label,
                    foreignKeyEntity: null,
                    foreignKeyField: null,
                    persist: true
                );
            }
        }
        return null;
    }

    private OrigamDataType ConvertType(string origamDataTypeId)
    {
        return origamDataTypeId switch
        {
            "0dcc6797-c46e-4774-89fe-113eef732651" => OrigamDataType.String,
            "d51c8102-d783-44fa-93bd-c2b0851ee5f3" => OrigamDataType.Boolean,
            "1c8ad59a-9e65-4668-9436-6e6438e8d841" => OrigamDataType.Integer,
            "2831895e-ae66-4e43-814c-ba09d0b2a2d5" => OrigamDataType.Float,
            "6b7a4139-0eb8-43cd-87cf-368bb404217a" => OrigamDataType.Currency,
            "15813764-c677-4207-8543-61f5edaf27a1" => OrigamDataType.Date,
            "cb4d42d9-ce9e-4824-b0e8-d45b7b77e8a3" => OrigamDataType.UniqueIdentifier,
            "d7483b5f-cb08-4691-a886-67eb548cb3c2" => OrigamDataType.Memo,
            _ => throw new ArgumentOutOfRangeException(
                paramName: nameof(origamDataTypeId),
                actualValue: origamDataTypeId,
                message: "Invalid type"
            ),
        };
    }

    private IDataDocument GetDatabaseFieldsMetaData()
    {
        var documentationService = ServiceManager.Services.GetService<IDocumentationService>();
        DataStructure fieldDocumentationStructure =
            persistenceService.SchemaProvider.RetrieveInstance<DataStructure>(
                instanceId: new Guid(g: "214c9cf7-5459-45e2-b3ff-7ee813bb85f4")
            );
        DataSet fieldDocumentationDataSet = new DatasetGenerator(
            userDefinedParameters: false
        ).CreateDataSet(ds: fieldDocumentationStructure);
        DataTable resultTable = fieldDocumentationDataSet.Tables[name: "OrigamFieldDocumentation"];
        var allFields = persistenceService.SchemaProvider.RetrieveList<FieldMappingItem>();
        foreach (var field in allFields)
        {
            switch (field.ParentItem)
            {
                case DetachedEntity { Inheritable: false }:
                {
                    continue;
                }
                case DetachedEntity { Inheritable: true }:
                {
                    HandleFieldInInheritableEntity(
                        resultTable: resultTable,
                        fieldMappingItem: field,
                        documentationService: documentationService
                    );
                    continue;
                }
                case TableMappingItem { DatabaseObjectType: DatabaseMappingObjectType.View }:
                {
                    continue;
                }
                default:
                {
                    AddDatabaseFieldToResultTable(
                        resultTable: resultTable,
                        fieldMappingItem: field,
                        parentTableMappingItem: field.ParentItem as TableMappingItem,
                        documentationService: documentationService
                    );
                    break;
                }
            }
        }
        return DataDocumentFactory.New(dataSet: fieldDocumentationDataSet);
    }

    private void HandleFieldInInheritableEntity(
        DataTable resultTable,
        FieldMappingItem fieldMappingItem,
        IDocumentationService documentationService
    )
    {
        List<TableMappingItem> tableMappingItems =
            persistenceService.SchemaProvider.RetrieveList<TableMappingItem>();
        foreach (TableMappingItem tableMappingItem in tableMappingItems)
        {
            if (tableMappingItem.DatabaseObjectType == DatabaseMappingObjectType.View)
            {
                continue;
            }
            foreach (SchemaItemAncestor ancestor in tableMappingItem.Ancestors)
            {
                if (ancestor.AncestorId.Equals(g: fieldMappingItem.ParentItem.Id))
                {
                    AddDatabaseFieldToResultTable(
                        resultTable: resultTable,
                        fieldMappingItem: fieldMappingItem,
                        parentTableMappingItem: tableMappingItem,
                        documentationService: documentationService
                    );
                    break;
                }
            }
        }
    }

    private void AddDatabaseFieldToResultTable(
        DataTable resultTable,
        FieldMappingItem fieldMappingItem,
        TableMappingItem parentTableMappingItem,
        IDocumentationService documentationService
    )
    {
        DataRow row = resultTable.NewRow();
        row[columnName: "AllowNulls"] = fieldMappingItem.AllowNulls;
        row[columnName: "Caption"] = fieldMappingItem.Caption;
        row[columnName: "DataLength"] = fieldMappingItem.DataLength;
        row[columnName: "DataType"] = fieldMappingItem.DataType;
        row[columnName: "DefaultValue"] = fieldMappingItem.DefaultValue;
        row[columnName: "PackageName"] = fieldMappingItem.PackageName;
        row[columnName: "ForeignKeyEntity"] =
            (fieldMappingItem.ForeignKeyEntity as TableMappingItem)?.MappedObjectName ?? "";
        row[columnName: "ForeignKeyField"] =
            (fieldMappingItem.ForeignKeyField as FieldMappingItem)?.MappedColumnName ?? "";
        row[columnName: "IsPrimaryKey"] = fieldMappingItem.IsPrimaryKey;
        row[columnName: "Name"] = fieldMappingItem.MappedColumnName;
        row[columnName: "ParentEntityName"] = parentTableMappingItem.MappedObjectName;
        row[columnName: "UserShortHelp"] = documentationService.GetDocumentation(
            schemaItemId: fieldMappingItem.Id,
            docType: DocumentationType.USER_SHORT_HELP
        );
        row[columnName: "UserLongHelp"] = documentationService.GetDocumentation(
            schemaItemId: fieldMappingItem.Id,
            docType: DocumentationType.USER_LONG_HELP
        );
        resultTable.Rows.Add(row: row);
    }

    private string ElementAttribute(Guid id, string attributeName)
    {
        var persistence = ServiceManager.Services.GetService<IPersistenceService>();
        var element = persistence.SchemaProvider.RetrieveInstance<ISchemaItem>(instanceId: id);
        return Reflector
                .GetValue(type: element.GetType(), instance: element, memberName: attributeName)
                ?.ToString() ?? "";
    }

    private IDataDocument ElementList(Guid parentId, string itemType)
    {
        var persistence = ServiceManager.Services.GetService<IPersistenceService>();
        var parent = persistence.SchemaProvider.RetrieveInstance<ISchemaItem>(instanceId: parentId);
        DataSet data = new DataSet(dataSetName: "ROOT");
        DataTable table = new DataTable(tableName: "Element");
        table.Columns.Add(columnName: "Id", type: typeof(Guid));
        table.Columns.Add(columnName: "Name", type: typeof(string));
        data.Tables.Add(table: table);
        foreach (ISchemaItem item in parent.ChildItemsByType<ISchemaItem>(itemType: itemType))
        {
            DataRow row = table.NewRow();
            row[columnName: "Id"] = item.Id;
            var members = Reflector.FindMembers(
                type: item.GetType(),
                primaryAttribute: typeof(System.Xml.Serialization.XmlAttributeAttribute)
            );
            foreach (MemberAttributeInfo memberInfo in members)
            {
                string name = memberInfo.MemberInfo.Name;
                Type type;
                switch (memberInfo.MemberInfo)
                {
                    case System.Reflection.PropertyInfo propertyInfo:
                    {
                        type = propertyInfo.PropertyType;
                        break;
                    }

                    case System.Reflection.FieldInfo fieldInfo:
                    {
                        type = fieldInfo.FieldType;
                        break;
                    }

                    default:
                    {
                        throw new Exception(message: "Unknown type.");
                    }
                }
                if (!table.Columns.Contains(name: name))
                {
                    table.Columns.Add(columnName: name, type: type);
                }
                row[columnName: name] = Reflector.GetValue(
                    memberInfo: memberInfo.MemberInfo,
                    instance: item
                );
            }
            table.Rows.Add(row: row);
        }
        return DataDocumentFactory.New(dataSet: data);
    }
    #endregion
}
