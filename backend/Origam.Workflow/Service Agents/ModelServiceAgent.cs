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
                result = GenerateSimpleModel(Parameters.Get<IDataDocument>("Data"));
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
                    Parameters.Get<Guid>("Id"),
                    Parameters.Get<string>("AttributeName")
                );
                break;
            }
            case "ElementListByParent":
            {
                result = ElementList(
                    Parameters.Get<Guid>("ParentId"),
                    Parameters.Get<string>("ItemType")
                );
                break;
            }
            case "SetTrace":
            {
                tracingService.Enabled = Parameters.Get<bool>("Enabled");
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
                Guid objectId = Parameters.Get<Guid>("ObjectId");
                var traceable = persistenceService.SchemaProvider.RetrieveInstance<ITraceable>(
                    objectId,
                    useCache: true
                );
                string traceType = Parameters.Get<string>("TraceType");
                if (!Enum.TryParse(traceType, out Trace trace))
                {
                    throw new ArgumentException(
                        $"\"{traceType}\" cannot be parsed to {nameof(Trace)}"
                    );
                }
                traceable.TraceLevel = trace;
                persistenceService.SchemaProvider.Persist(traceable as IPersistent);
                result = traceable.TraceLevel;
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(
                    nameof(MethodName),
                    MethodName,
                    ResourceUtils.GetString("InvalidMethodName")
                );
            }
        }
    }
    #endregion
    #region Private Methods
    private object GenerateSimpleModel(IDataDocument dataDocument)
    {
        ISchemaService schemaService = ServiceManager.Services.GetService<ISchemaService>();
        schemaService.StorageSchemaExtensionId = new Guid(GENERATED_PACKAGE_ID);
        SimpleModelData model = dataDocument.DataSet as SimpleModelData;
        if (model == null)
        {
            throw new ArgumentOutOfRangeException("Data", dataDocument, "Unknown data");
        }
        foreach (var entity in model.OrigamEntity)
        {
            var table = EntityHelper.CreateTable(entity.Name, group: null, persist: true);
            foreach (var field in entity.GetOrigamFieldRows())
            {
                EntityHelper.CreateColumn(
                    table,
                    field.Name,
                    allowNulls: !field.IsMandatory,
                    ConvertType(field.refOrigamDataTypeId.ToString()),
                    field.IsTextLengthNull() ? 0 : field.TextLength,
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
                nameof(origamDataTypeId),
                origamDataTypeId,
                "Invalid type"
            ),
        };
    }

    private IDataDocument GetDatabaseFieldsMetaData()
    {
        var documentationService = ServiceManager.Services.GetService<IDocumentationService>();
        DataStructure fieldDocumentationStructure =
            persistenceService.SchemaProvider.RetrieveInstance<DataStructure>(
                new Guid("214c9cf7-5459-45e2-b3ff-7ee813bb85f4")
            );
        DataSet fieldDocumentationDataSet = new DatasetGenerator(
            userDefinedParameters: false
        ).CreateDataSet(fieldDocumentationStructure);
        DataTable resultTable = fieldDocumentationDataSet.Tables["OrigamFieldDocumentation"];
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
                    HandleFieldInInheritableEntity(resultTable, field, documentationService);
                    continue;
                }
                case TableMappingItem { DatabaseObjectType: DatabaseMappingObjectType.View }:
                {
                    continue;
                }
                default:
                {
                    AddDatabaseFieldToResultTable(
                        resultTable,
                        field,
                        field.ParentItem as TableMappingItem,
                        documentationService
                    );
                    break;
                }
            }
        }
        return DataDocumentFactory.New(fieldDocumentationDataSet);
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
                if (ancestor.AncestorId.Equals(fieldMappingItem.ParentItem.Id))
                {
                    AddDatabaseFieldToResultTable(
                        resultTable,
                        fieldMappingItem,
                        tableMappingItem,
                        documentationService
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
        row["AllowNulls"] = fieldMappingItem.AllowNulls;
        row["Caption"] = fieldMappingItem.Caption;
        row["DataLength"] = fieldMappingItem.DataLength;
        row["DataType"] = fieldMappingItem.DataType;
        row["DefaultValue"] = fieldMappingItem.DefaultValue;
        row["PackageName"] = fieldMappingItem.PackageName;
        row["ForeignKeyEntity"] =
            (fieldMappingItem.ForeignKeyEntity as TableMappingItem)?.MappedObjectName ?? "";
        row["ForeignKeyField"] =
            (fieldMappingItem.ForeignKeyField as FieldMappingItem)?.MappedColumnName ?? "";
        row["IsPrimaryKey"] = fieldMappingItem.IsPrimaryKey;
        row["Name"] = fieldMappingItem.MappedColumnName;
        row["ParentEntityName"] = parentTableMappingItem.MappedObjectName;
        row["UserShortHelp"] = documentationService.GetDocumentation(
            fieldMappingItem.Id,
            DocumentationType.USER_SHORT_HELP
        );
        row["UserLongHelp"] = documentationService.GetDocumentation(
            fieldMappingItem.Id,
            DocumentationType.USER_LONG_HELP
        );
        resultTable.Rows.Add(row);
    }

    private string ElementAttribute(Guid id, string attributeName)
    {
        var persistence = ServiceManager.Services.GetService<IPersistenceService>();
        var element = persistence.SchemaProvider.RetrieveInstance<ISchemaItem>(id);
        return Reflector.GetValue(element.GetType(), element, attributeName)?.ToString() ?? "";
    }

    private IDataDocument ElementList(Guid parentId, string itemType)
    {
        var persistence = ServiceManager.Services.GetService<IPersistenceService>();
        var parent = persistence.SchemaProvider.RetrieveInstance<ISchemaItem>(parentId);
        DataSet data = new DataSet("ROOT");
        DataTable table = new DataTable("Element");
        table.Columns.Add("Id", typeof(Guid));
        table.Columns.Add("Name", typeof(string));
        data.Tables.Add(table);
        foreach (ISchemaItem item in parent.ChildItemsByType<ISchemaItem>(itemType))
        {
            DataRow row = table.NewRow();
            row["Id"] = item.Id;
            var members = Reflector.FindMembers(
                item.GetType(),
                typeof(System.Xml.Serialization.XmlAttributeAttribute)
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
                        throw new Exception("Unknown type.");
                    }
                }
                if (!table.Columns.Contains(name))
                {
                    table.Columns.Add(name, type);
                }
                row[name] = Reflector.GetValue(memberInfo.MemberInfo, item);
            }
            table.Rows.Add(row);
        }
        return DataDocumentFactory.New(data);
    }
    #endregion
}
