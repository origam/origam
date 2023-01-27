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
using System.Xml;
using Origam.Workflow;
using Origam.Schema.EntityModel;
using Origam.Schema;
using Origam.Services;
using Origam.Workbench.Services;
using System.Data;
using Origam.DA.ObjectPersistence;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;

namespace Origam.Workflow
{
    /// <summary>
    /// Summary description for WorkflowServiceAgent.
    /// </summary>
    public class ModelServiceAgent : AbstractServiceAgent
    {
        const string GENERATED_PACKAGE_ID = "3cfc0308-fd23-454c-9d7a-a00054e0b9b1";
        const string GENERATED_PACKAGE_NAME = "_generated";
        public ModelServiceAgent()
        {
            persistenceService = ServiceManager
                .Services
                .GetService<IPersistenceService>();
            tracingService = ServiceManager
                .Services
                .GetService<TracingService>();
        }

        #region Private Methods
        private object GenerateSimpleModel(IDataDocument dataDocument)
        {
            ISchemaService schemaService = ServiceManager.Services.GetService(
                typeof(ISchemaService)) as ISchemaService;
            schemaService.StorageSchemaExtensionId = new Guid(GENERATED_PACKAGE_ID);
            SimpleModelData model = dataDocument.DataSet as SimpleModelData;
            if (model == null)
            {
                throw new ArgumentOutOfRangeException("Data", dataDocument, "Unknown data");
            }
            SchemaItemGroup entityGroup = null;
            foreach (var entity in model.OrigamEntity)
            {
                var table = EntityHelper.CreateTable(entity.Name, entityGroup, true);
                foreach (var field in entity.GetOrigamFieldRows())
                {
                    var column = EntityHelper.CreateColumn(table, field.Name,
                        !field.IsMandatory, ConvertType(field.refOrigamDataTypeId.ToString()),
                        field.IsTextLengthNull() ? 0 : field.TextLength, field.Label, null, null, true);
                }
            }
            return null;
        }

        private OrigamDataType ConvertType(string origamDataTypeId)
        {
            switch (origamDataTypeId)
            {
                case "0dcc6797-c46e-4774-89fe-113eef732651":
                    return OrigamDataType.String;
                case "d51c8102-d783-44fa-93bd-c2b0851ee5f3":
                    return OrigamDataType.Boolean;
                case "1c8ad59a-9e65-4668-9436-6e6438e8d841":
                    return OrigamDataType.Integer;
                case "2831895e-ae66-4e43-814c-ba09d0b2a2d5":
                    return OrigamDataType.Float;
                case "6b7a4139-0eb8-43cd-87cf-368bb404217a":
                    return OrigamDataType.Currency;
                case "15813764-c677-4207-8543-61f5edaf27a1":
                    return OrigamDataType.Date;
                case "cb4d42d9-ce9e-4824-b0e8-d45b7b77e8a3":
                    return OrigamDataType.UniqueIdentifier;
                case "d7483b5f-cb08-4691-a886-67eb548cb3c2":
                    return OrigamDataType.Memo;
                default:
                    throw new ArgumentOutOfRangeException("origamDataTypeId",
                        origamDataTypeId, "Invalid type");
            }
        }
        #endregion

        #region IServiceAgent Members
        private object _result;
        private readonly TracingService tracingService;
        private IPersistenceService persistenceService;

        public override object Result
        {
            get
            {
                return _result;
            }
        }

        public override void Run()
        {
            switch (MethodName)
            {
                case "GenerateSimpleModel":
                    _result = GenerateSimpleModel(
                        Parameters.Get<IDataDocument>("Data"));
                    break;

                case "ElementAttribute":
                    _result = ElementAttribute(
                        Parameters.Get<Guid>("Id"),
                        Parameters.Get<string>("AttributeName"));
                    break;

                case "ElementListByParent":
                    _result = ElementList(
                        Parameters.Get<Guid>("ParentId"),
                        Parameters.Get<string>("ItemType"));
                    break;  
                
                case "SetTrace":
                    tracingService.Enabled = Parameters.Get<bool>("Enabled");
                    _result = tracingService.Enabled;
                    break;  
                
                case "GetTrace":
                    _result = tracingService.Enabled;
                    break;             
                
                case "TraceObject":
                    Guid objectId = Parameters.Get<Guid>("ObjectId");
                    var traceable = persistenceService.SchemaProvider
                        .RetrieveInstance<ITraceable>(
                            objectId, 
                            useCache: true);
                    string traceType = Parameters.Get<string>("TraceType");
                    if (!Enum.TryParse(traceType, out Trace trace))
                    {
                        throw new ArgumentException($"\"{traceType}\" cannot be parsed to {nameof(Trace)}");
                    }
                    traceable.TraceLevel = trace;
                    persistenceService.SchemaProvider.Persist(traceable as IPersistent);
                    _result = traceable.TraceLevel;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("MethodName",
                        MethodName, ResourceUtils.GetString("InvalidMethodName"));
            }
        }

        private string ElementAttribute(Guid id, string attributeName)
        {
            var persistence = ServiceManager.Services.GetService
                <IPersistenceService>();
            var element = persistence.SchemaProvider.RetrieveInstance
                <AbstractSchemaItem>(id);
            return Reflector.GetValue(element.GetType(), element, attributeName)?.ToString() ?? "";
        }

        private IDataDocument ElementList(Guid parentId, string itemType)
        {
            var persistence = ServiceManager.Services.GetService
                <IPersistenceService>();
            var parent = persistence.SchemaProvider.RetrieveInstance
                <AbstractSchemaItem>(parentId);

            DataSet data = new DataSet("ROOT");
            DataTable table = new DataTable("Element");
            table.Columns.Add("Id", typeof(Guid));
            table.Columns.Add("Name", typeof(string));
            data.Tables.Add(table);
            foreach (AbstractSchemaItem item in parent.ChildItemsByType(itemType))
            {
                DataRow row = table.NewRow();
                row["Id"] = item.Id;
                var members = Reflector.FindMembers(item.GetType(), typeof(System.Xml.Serialization.XmlAttributeAttribute));
                foreach (MemberAttributeInfo memberInfo in members)
                {
                    string name = memberInfo.MemberInfo.Name;
                    Type type;
                    switch (memberInfo.MemberInfo)
                    {
                        case System.Reflection.PropertyInfo propertyInfo:
                            type = propertyInfo.PropertyType;
                            break;
                        case System.Reflection.FieldInfo fieldInfo:
                            type = fieldInfo.FieldType;
                            break;
                        default:
                            throw new Exception("Unknown type.");
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
}
