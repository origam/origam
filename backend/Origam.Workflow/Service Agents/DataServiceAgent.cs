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
using System.Data;
using System.IO;
using System.Security.Principal;
using Origam.DA;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.LookupModel;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;
using Origam.Workbench.Services;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.Workflow;

/// <summary>
/// Summary description for DataService.
/// </summary>
public class DataServiceAgent : AbstractServiceAgent
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    private IDataService _dataService = null;

    public DataServiceAgent()
    {
        _dataService = core.DataServiceFactory.GetDataService();
    }

    public IDataService DataService
    {
        get { return _dataService; }
    }

    public override void SetDataService(IDataService dataService)
    {
        _dataService = dataService;
    }

    private IDataDocument LoadData(
        Guid dataStructureId,
        Hashtable parameters,
        Guid methodId,
        Guid sortSetId
    )
    {
        // (_dataService as MsSqlDataService).PersistenceProvider = this.PersistenceProvider;
        DataStructureQuery query = new DataStructureQuery(
            dataStructureId: dataStructureId,
            methodId: methodId,
            defaultSetId: Guid.Empty,
            sortSetId: sortSetId
        );
        // Fill parameters
        if (parameters != null)
        {
            foreach (DictionaryEntry entry in parameters)
            {
                query.Parameters.Add(
                    value: new QueryParameter(
                        _parameterName: entry.Key as string,
                        value: entry.Value
                    )
                );
            }
        }
        return DataDocumentFactory.New(dataSet: LoadData(query: query, data: null));
    }

    private DataSet LoadData(DataStructureQuery query)
    {
        return LoadData(query: query, data: null);
    }

    private DataSet LoadData(DataStructureQuery query, DataSet data)
    {
        DataStructureMethod method;
        method =
            this.PersistenceProvider.RetrieveInstance(
                type: typeof(DataStructureMethod),
                primaryKey: new ModelElementKey(id: query.MethodId)
            ) as DataStructureMethod;
        if (method == null || method is DataStructureFilterSet)
        {
            if (data == null)
            {
                return _dataService.LoadDataSet(
                    dataStructureQuery: query,
                    userProfile: SecurityManager.CurrentPrincipal,
                    transactionId: this.TransactionId
                );
            }

            return _dataService.LoadDataSet(
                dataStructureQuery: query,
                userProfile: SecurityManager.CurrentPrincipal,
                dataSet: data,
                transactionId: this.TransactionId
            );
        }

        if (method is DataStructureWorkflowMethod)
        {
            // call workflow and fill dataset with workflow result
            DataStructureWorkflowMethod dataStructureWorkflowMethod =
                (DataStructureWorkflowMethod)method;
            ContextStore returnContext =
                dataStructureWorkflowMethod.LoadWorkflow.GetReturnContext();
            if (returnContext == null)
            {
                // return contextstore not found
                throw new ArgumentException(
                    message: String.Format(
                        format: "The input data was sent to method {0}"
                            + "while there is no context store defined with `IsReturnValue' set for workflow {1},",
                        arg0: method.Id,
                        arg1: dataStructureWorkflowMethod.LoadWorkflowId
                    )
                );
            }
            if (data != null)
            // current data was sent, we have to pass them into main (output) contextstore of our workflow
            {
                query.Parameters.Add(
                    value: new QueryParameter(_parameterName: returnContext.Name, value: data)
                );
            }
            Object res = core.WorkflowService.ExecuteWorkflow(
                workflowId: dataStructureWorkflowMethod.LoadWorkflowId,
                parameters: query.Parameters,
                transactionId: this.TransactionId
            );
            if (res is IDataDocument)
            {
                DataSet res2 = new DataSet();
                // make deep copy of dataset in order remove connection between dataset and XmlDataDocument
                // TODO - either return DataSet from WF,
                // or change everything UP to XSLT page handler to XmlDataDocument
                res2 = ((IDataDocument)res).DataSet.Copy();
                return res2;
            }
            try
            {
                return (DataSet)res;
            }
            catch (System.InvalidCastException e)
            {
                throw new System.InvalidCastException(
                    message: String.Format(
                        format: "{0} (Try to change "
                            + " your workflow connected to DataStructureWorkflowMethod to return "
                            + "the same datastructure as the WorkflowMethod is connected to.)",
                        arg0: e.Message
                    )
                );
            }
        }

        throw new ArgumentException(
            message: String.Format(
                format: "MethodId ({0}) schema item element not found or not of proper type",
                arg0: method.Id
            )
        );
    }

    private object GetScalarValue(DataStructureQuery query, ColumnsInfo columnsInfo)
    {
        DataStructureMethod method;
        method =
            this.PersistenceProvider.RetrieveInstance(
                type: typeof(DataStructureMethod),
                primaryKey: new ModelElementKey(id: query.MethodId)
            ) as DataStructureMethod;
        if (method == null || method is DataStructureFilterSet)
        {
            return _dataService.GetScalarValue(
                query: query,
                columnsInfo: columnsInfo,
                userProfile: SecurityManager.CurrentPrincipal,
                transactionId: this.TransactionId
            );
        }

        if (method is DataStructureWorkflowMethod)
        {
            // call workflow and fill dataset with workflow result
            DataStructureWorkflowMethod dataStructureWorkflowMethod =
                (DataStructureWorkflowMethod)method;
            ContextStore returnContext =
                dataStructureWorkflowMethod.LoadWorkflow.GetReturnContext();
            if (returnContext == null)
            {
                // return contextstore not found
                throw new ArgumentException(
                    message: String.Format(
                        format: "Return context store not found for workflow {0}.",
                        arg0: dataStructureWorkflowMethod.LoadWorkflowId
                    )
                );
            }

            if (!returnContext.isScalar())
            {
                // return contextstore is not scalar
                throw new ArgumentException(
                    message: String.Format(
                        format: "Return context store is not of scalar type for workflow {0}",
                        arg0: dataStructureWorkflowMethod.LoadWorkflowId
                    )
                );
            }
            // call workflow
            return core.WorkflowService.ExecuteWorkflow(
                workflowId: dataStructureWorkflowMethod.LoadWorkflowId,
                parameters: query.Parameters,
                transactionId: this.TransactionId
            );
        }

        throw new ArgumentException(
            message: String.Format(
                format: "MethodId ({0}) schema item element not found or not of proper type",
                arg0: method.Id
            )
        );
    }

    private IDataDocument ExecuteProcedure(string name, Hashtable parameters, string entityOrder)
    {
        DataStructureQuery query = new DataStructureQuery();
        if (this.OutputStructure != null)
        {
            query.DataSourceId = (Guid)this.OutputStructure.PrimaryKey[key: "Id"];
        }

        if (parameters != null)
        {
            foreach (DictionaryEntry entry in parameters)
            {
                query.Parameters.Add(
                    value: new QueryParameter(
                        _parameterName: entry.Key as string,
                        value: entry.Value
                    )
                );
            }
        }
        DataSet result = _dataService.ExecuteProcedure(
            name: name,
            entityOrder: entityOrder,
            query: query,
            transactionId: this.TransactionId
        );
        if (result == null)
        {
            return null;
        }

        return DataDocumentFactory.New(dataSet: result);
    }

    private IDataDocument SaveData(
        Guid dataStructureId,
        Guid methodId,
        Guid sortSetId,
        IDataDocument data,
        bool forceBulkInsert
    )
    {
        DataSet dataset = data.DataSet;
        //			(_dataService as MsSqlDataService).PersistenceProvider = this.PersistenceProvider;
        DataStructureQuery query = new DataStructureQuery(
            dataStructureId: dataStructureId,
            methodId: methodId,
            defaultSetId: Guid.Empty,
            sortSetId: sortSetId
        );

        if (this.OutputMethod == ServiceOutputMethod.Ignore)
        {
            query.LoadActualValuesAfterUpdate = false;
        }
        else
        {
            query.LoadActualValuesAfterUpdate = true;
        }
        try
        {
            _dataService.UpdateData(
                dataStructureQuery: query,
                userProfile: SecurityManager.CurrentPrincipal,
                ds: dataset,
                transactionId: TransactionId,
                forceBulkInsert: forceBulkInsert
            );
        }
        catch (ConstraintException)
        {
            // make the exception far more verbose
            throw new ConstraintException(s: DatasetTools.GetDatasetErrors(dataset: dataset));
        }
        return data;
    }

    private DataSet SaveData(DataStructureQuery query, DataSet data)
    {
        try
        {
            IPrincipal principal = null;
            if (query.LoadByIdentity)
            {
                principal = SecurityManager.CurrentPrincipal;
            }
            _dataService.UpdateData(
                dataStructureQuery: query,
                userProfile: principal,
                ds: data,
                transactionId: TransactionId
            );
        }
        catch (ConstraintException)
        {
            // make the exception far more verbose
            throw new ConstraintException(s: DatasetTools.GetDatasetErrors(dataset: data));
        }
        return data;
    }

    private int UpdateReferences(Guid entityId, object oldValue, object newValue)
    {
        TableMappingItem originalEntity =
            this.PersistenceProvider.RetrieveInstance(
                type: typeof(TableMappingItem),
                primaryKey: new ModelElementKey(id: entityId)
            ) as TableMappingItem;
        if (originalEntity == null)
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(
                    key: "ErrorTableMappingNotFound",
                    args: entityId.ToString()
                )
            );
        }
        List<IDataEntityColumn> pkList = originalEntity.EntityPrimaryKey;
        if (pkList.Count == 0)
        {
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(
                    key: "ErrorNoPrimaryKey",
                    args: originalEntity.Path
                )
            );
        }

        if (pkList.Count > 1)
        {
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(
                    key: "ErrorMultiColumnPrimaryKey",
                    args: originalEntity.Path
                )
            );
        }

        FieldMappingItem originalKey = pkList[index: 0] as FieldMappingItem;
        if (originalKey == null)
        {
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(
                    key: "ErrorPrimaryKeyNotMapped",
                    args: originalEntity.Path
                )
            );
        }

        SchemaService schema =
            ServiceManager.Services.GetService(serviceType: typeof(SchemaService)) as SchemaService;
        EntityModelSchemaItemProvider entities =
            schema.GetProvider(type: typeof(EntityModelSchemaItemProvider))
            as EntityModelSchemaItemProvider;
        int result = 0;
        string transactionId = this.TransactionId;
        if (transactionId == null)
        {
            transactionId = Guid.NewGuid().ToString();
        }

        ITracingService trace =
            ServiceManager.Services.GetService(serviceType: typeof(ITracingService))
            as ITracingService;
        if (this.Trace)
        {
            trace.TraceStep(
                workflowInstanceId: this.TraceWorkflowId,
                stepPath: this.TraceStepName,
                stepId: this.TraceStepId,
                category: "Deduplication",
                subCategory: "Start",
                remark: originalEntity.MappedObjectName,
                data1: "",
                data2: "",
                message: "Old Id: "
                    + oldValue.ToString()
                    + Environment.NewLine
                    + "New Id: "
                    + newValue.ToString()
            );
        }
        try
        {
            foreach (IDataEntity entity in entities.ChildItems)
            {
                TableMappingItem table = entity as TableMappingItem;
                if (
                    table != null
                    && table.DatabaseObjectType == DatabaseMappingObjectType.Table
                    && table.GenerateDeploymentScript
                )
                {
                    foreach (IDataEntityColumn column in table.EntityColumns)
                    {
                        if (
                            column.ForeignKeyEntity != null
                            && column.ForeignKeyEntity.PrimaryKey.Equals(
                                obj: originalEntity.PrimaryKey
                            )
                            && column is FieldMappingItem
                        )
                        {
                            if (
                                column.ForeignKeyField != null
                                && column.ForeignKeyField.PrimaryKey.Equals(
                                    obj: originalKey.PrimaryKey
                                )
                            )
                            {
                                int records = this.DataService.UpdateField(
                                    entityId: table.Id,
                                    fieldId: (Guid)column.PrimaryKey[key: "Id"],
                                    oldValue: oldValue,
                                    newValue: newValue,
                                    userProfile: SecurityManager.CurrentPrincipal,
                                    transactionId: transactionId
                                );
                                result += records;
                                if (records > 0)
                                {
                                    if (this.Trace || log.IsInfoEnabled)
                                    {
                                        string logText =
                                            this.DataService.GetType().ToString()
                                            + " updated "
                                            + records
                                            + " references on "
                                            + table.MappedObjectName
                                            + "."
                                            + (column as FieldMappingItem).MappedColumnName;
                                        if (log.IsInfoEnabled)
                                        {
                                            log.Info(message: logText);
                                        }
                                        if (this.Trace)
                                        {
                                            trace?.TraceStep(
                                                workflowInstanceId: this.TraceWorkflowId,
                                                stepPath: this.TraceStepName,
                                                stepId: this.TraceStepId,
                                                category: "Deduplication",
                                                subCategory: "Progress",
                                                remark: originalEntity.MappedObjectName,
                                                data1: "Audit: " + table.AuditingType,
                                                data2: "",
                                                message: logText
                                            );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError(message: "Updating references failed.", ex: ex);
            }
            if (this.TransactionId == null)
            {
                ResourceMonitor.Rollback(transactionId: transactionId);
            }
            throw;
        }
        if (this.TransactionId == null)
        {
            ResourceMonitor.Commit(transactionId: transactionId);
        }

        return result;
    }

    private int ReferenceCount(Guid entityId, object value)
    {
        TableMappingItem originalEntity =
            this.PersistenceProvider.RetrieveInstance(
                type: typeof(TableMappingItem),
                primaryKey: new ModelElementKey(id: entityId)
            ) as TableMappingItem;
        if (originalEntity == null)
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(
                    key: "ErrorTableMappingNotFound",
                    args: entityId.ToString()
                )
            );
        }
        List<IDataEntityColumn> pkList = originalEntity.EntityPrimaryKey;
        if (pkList.Count == 0)
        {
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(
                    key: "ErrorNoPrimaryKey",
                    args: originalEntity.Path
                )
            );
        }

        if (pkList.Count > 1)
        {
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(
                    key: "ErrorMultiColumnPrimaryKey",
                    args: originalEntity.Path
                )
            );
        }

        FieldMappingItem originalKey = pkList[index: 0] as FieldMappingItem;
        if (originalKey == null)
        {
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(
                    key: "ErrorPrimaryKeyNotMapped",
                    args: originalEntity.Path
                )
            );
        }

        List<IDataEntity> skipRelationships = originalEntity.ChildEntitiesRecursive;
        SchemaService schema =
            ServiceManager.Services.GetService(serviceType: typeof(SchemaService)) as SchemaService;
        EntityModelSchemaItemProvider entities =
            schema.GetProvider(type: typeof(EntityModelSchemaItemProvider))
            as EntityModelSchemaItemProvider;
        int result = 0;
        string transactionId = this.TransactionId;
        ITracingService trace =
            ServiceManager.Services.GetService(serviceType: typeof(ITracingService))
            as ITracingService;
        if (this.Trace)
        {
            trace.TraceStep(
                workflowInstanceId: this.TraceWorkflowId,
                stepPath: this.TraceStepName,
                stepId: this.TraceStepId,
                category: "ReferenceCount",
                subCategory: "",
                remark: originalEntity.MappedObjectName,
                data1: "",
                data2: "",
                message: "Id: " + value.ToString()
            );
        }
        foreach (IDataEntity entity in entities.ChildItems)
        {
            if (!skipRelationships.Contains(item: entity))
            {
                TableMappingItem table = entity as TableMappingItem;
                if (
                    table != null
                    && table.DatabaseObjectType == DatabaseMappingObjectType.Table
                    && table.GenerateDeploymentScript
                )
                {
                    foreach (IDataEntityColumn column in table.EntityColumns)
                    {
                        if (
                            column.ForeignKeyEntity != null
                            && column.ForeignKeyEntity.PrimaryKey.Equals(
                                obj: originalEntity.PrimaryKey
                            )
                            && column is FieldMappingItem
                        )
                        {
                            if (
                                column.ForeignKeyField != null
                                && column.ForeignKeyField.PrimaryKey.Equals(
                                    obj: originalKey.PrimaryKey
                                )
                            )
                            {
                                int records = this.DataService.ReferenceCount(
                                    entityId: table.Id,
                                    fieldId: (Guid)column.PrimaryKey[key: "Id"],
                                    value: value,
                                    userProfile: SecurityManager.CurrentPrincipal,
                                    transactionId: transactionId
                                );
                                result += records;
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    private string PrintStream(System.IO.Stream s)
    {
        string result = "";
        //set position to beginning of the stream
        s.Position = 0;
        using (StreamReader sr = new StreamReader(stream: s))
        {
            char[] buf = new char[s.Length];
            sr.ReadBlock(buffer: buf, index: 0, count: (int)s.Length - 1);
            result = new string(value: buf);
        }
        return result;
    }

    #region IServiceAgent Members
    public override string Info
    {
        get
        {
            string result = "Adapter Assembly: " + this.GetType().ToString() + Environment.NewLine;
            result +=
                "Data Service Assembly: " + _dataService.GetType().ToString() + Environment.NewLine;
            return result + _dataService.Info;
        }
    }
    private object _result;
    public override object Result
    {
        get
        {
            object temp = _result;
            _result = null;

            return temp;
        }
    }

    public override string ExecuteUpdate(string command, string transactionId)
    {
        return _dataService.ExecuteUpdate(command: command, transactionId: transactionId);
    }

    public override void Run()
    {
        DataStructureReference dsRef;
        switch (this.MethodName)
        {
            case "LoadData":
            case "StoreData":
            {
                dsRef = this.Parameters[key: "DataStructure"] as DataStructureReference;
                Guid dsId;
                Guid methodId = Guid.Empty;
                Guid sortId = Guid.Empty;
                bool forceBulkInsert = false;
                // Check input parameters
                if (dsRef == null)
                {
                    if (this.Parameters[key: "DataStructure"] != null)
                    {
                        throw new InvalidCastException(
                            message: ResourceUtils.GetString(key: "ErrorNotDataStructureReference")
                        );
                    }
                    dsId = (Guid)this.OutputStructure.PrimaryKey[key: "Id"];
                }
                else
                {
                    dsId = dsRef.DataStructureId;
                    methodId = dsRef.DataStructureMethodId;
                    sortId = dsRef.DataStructureSortSetId;
                }
                if (this.MethodName == "LoadData")
                {
                    if (
                        !(
                            this.Parameters[key: "Parameters"] == null
                            || this.Parameters[key: "Parameters"] is Hashtable
                        )
                    )
                    {
                        throw new InvalidCastException(
                            message: ResourceUtils.GetString(key: "ErrorNotHashtable")
                        );
                    }

                    _result = this.LoadData(
                        dataStructureId: dsId,
                        parameters: this.Parameters[key: "Parameters"] as Hashtable,
                        methodId: methodId,
                        sortSetId: sortId
                    );
                }
                else // StoreData
                {
                    if (!(Parameters[key: "Data"] is IDataDocument))
                    {
                        throw new InvalidCastException(
                            message: ResourceUtils.GetString(key: "ErrorNotXmlDataDocument")
                        );
                    }

                    if (Parameters.Contains(key: "ForceBulkInsert"))
                    {
                        forceBulkInsert = (bool)Parameters[key: "ForceBulkInsert"];
                    }
                    _result = SaveData(
                        dataStructureId: dsId,
                        methodId: methodId,
                        sortSetId: sortId,
                        data: Parameters[key: "Data"] as IDataDocument,
                        forceBulkInsert: forceBulkInsert
                    );
                }
                break;
            }

            case "LoadDataByQuery":
            {
                if (this.Parameters.Contains(key: "Data"))
                {
                    if (!(this.Parameters[key: "Data"] is DataSet))
                    {
                        throw new InvalidCastException(message: "Data is not of type DataSet");
                    }

                    _result = this.LoadData(
                        query: this.Parameters[key: "Query"] as DataStructureQuery,
                        data: this.Parameters[key: "Data"] as DataSet
                    );
                }
                else
                {
                    _result = this.LoadData(
                        query: this.Parameters[key: "Query"] as DataStructureQuery
                    );
                }
                break;
            }
            case "GetScalarValueByQuery":
            {
                _result = this.GetScalarValue(
                    query: this.Parameters[key: "Query"] as DataStructureQuery,
                    columnsInfo: new ColumnsInfo(
                        columnName: (string)this.Parameters[key: "ColumnName"]
                    )
                );
                break;
            }
            case "StoreDataByQuery":
            {
                // Check input parameters
                if (!(this.Parameters[key: "Query"] is DataStructureQuery))
                {
                    throw new InvalidCastException(
                        message: "Query is not of type DataStructureQuery"
                    );
                }

                if (!(this.Parameters[key: "Data"] is DataSet))
                {
                    throw new InvalidCastException(message: "Data is not of type DataSet");
                }

                _result = this.SaveData(
                    query: this.Parameters[key: "Query"] as DataStructureQuery,
                    data: this.Parameters[key: "Data"] as DataSet
                );
                break;
            }

            case "ExecuteProcedure":
            {
                // Check input parameters
                if (!(this.Parameters[key: "Name"] is string))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorNameNotString")
                    );
                }

                if (
                    !(
                        this.Parameters[key: "Parameters"] == null
                        || this.Parameters[key: "Parameters"] is Hashtable
                    )
                )
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorNotHashtable")
                    );
                }

                string entityOrder = null;
                if (this.Parameters.Contains(key: "EntityOrder"))
                {
                    entityOrder = (string)this.Parameters[key: "EntityOrder"];
                }
                _result = this.ExecuteProcedure(
                    name: (string)this.Parameters[key: "Name"],
                    parameters: this.Parameters[key: "Parameters"] as Hashtable,
                    entityOrder: entityOrder
                );
                break;
            }

            case "UpdateReferences":
            {
                // Check input parameters
                if (!(this.Parameters[key: "EntityId"] is Guid))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorEntityIdNotGuid")
                    );
                }

                _result = this.UpdateReferences(
                    entityId: (Guid)this.Parameters[key: "EntityId"],
                    oldValue: this.Parameters[key: "oldValue"],
                    newValue: this.Parameters[key: "newValue"]
                );
                break;
            }

            case "ReferenceCount":
            {
                // Check input parameters
                if (!(this.Parameters[key: "EntityId"] is Guid))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorEntityIdNotGuid")
                    );
                }

                _result = this.ReferenceCount(
                    entityId: (Guid)this.Parameters[key: "EntityId"],
                    value: this.Parameters[key: "Value"]
                );
                break;
            }

            case "EntityDdl":
            {
                _result = DataService.EntityDdl(entityId: (Guid)Parameters[key: "EntityId"]);
                break;
            }

            case "FieldDdl":
            {
                _result = DataService.FieldDdl(fieldId: (Guid)Parameters[key: "FieldId"]);
                break;
            }

            case "DatabaseSpecificDatatypes":
            {
                _result = DataService.DatabaseSpecificDatatypes();
                break;
            }

            case "ExecuteSql":
            {
                _result = DataService.ExecuteUpdate(
                    command: (string)Parameters[key: "Command"],
                    transactionId: this.TransactionId
                );
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "MethodName",
                    actualValue: this.MethodName,
                    message: ResourceUtils.GetString(key: "InvalidMethodName")
                );
            }
        }
    }

    public override IList<string> ExpectedParameterNames(
        ISchemaItem item,
        string method,
        string parameter
    )
    {
        var result = new List<string>();
        ServiceMethodCallTask call = item as ServiceMethodCallTask;
        XsltDataPage dataPage = item as XsltDataPage;
        FileDownloadPage downloadPage = item as FileDownloadPage;
        FormReferenceMenuItem formMenu = item as FormReferenceMenuItem;
        AbstractReport report = item as AbstractReport;
        EntityFilterLookupReference filterLookup = item as EntityFilterLookupReference;
        AbstractDataLookup abstractDataLookup = item as AbstractDataLookup;
        string prefix = null;
        if (method == "LoadData" && parameter == "Parameters")
        {
            DataStructure ds = null;
            DataStructureMethod dsMethod = null;
            if (call != null)
            {
                ResolveServiceMethodCallTask(task: call, ds: out ds, method: out dsMethod);
            }
            else if (dataPage != null)
            {
                ResolveXsltDataPage(page: dataPage, ds: out ds, method: out dsMethod);
            }
            else if (downloadPage != null)
            {
                ResolveFileDownloadPage(page: downloadPage, ds: out ds, method: out dsMethod);
            }
            else if (formMenu != null)
            {
                ResolveFormReferenceMenuItem(menu: formMenu, ds: out ds, method: out dsMethod);
            }
            else if (report != null)
            {
                ResolveReport(report: report, ds: out ds, method: out dsMethod);
            }
            else if (filterLookup != null)
            {
                ResolveFilterLookup(reference: filterLookup, ds: out ds, method: out dsMethod);
                // add "lookup" prefix to all parameters because as sub-queries they will
                // be prefixed
                prefix = "lookup";
            }
            else if (abstractDataLookup != null)
            {
                ResolveLookup(reference: abstractDataLookup, ds: out ds, method: out dsMethod);
            }
            if (ds != null)
            {
                DataStructureWorkflowMethod wm = dsMethod as DataStructureWorkflowMethod;
                DataStructureFilterSet fs = dsMethod as DataStructureFilterSet;
                if (wm != null)
                {
                    IBusinessServicesService agents =
                        ServiceManager.Services.GetService(
                            serviceType: typeof(IBusinessServicesService)
                        ) as IBusinessServicesService;
                    IServiceAgent agent = agents.GetAgent(
                        serviceType: "WorkflowService",
                        ruleEngine: null,
                        workflowEngine: null
                    );
                    return agent.ExpectedParameterNames(
                        item: wm.LoadWorkflow,
                        method: "ExecuteWorkflow",
                        parameter: "Parameters"
                    );
                }
                var sqlGenerator = ((AbstractDataService)_dataService).DbDataAdapterFactory;
                sqlGenerator.ResolveAllFilters = true;

                foreach (DataStructureEntity entity in ds.Entities)
                {
                    List<string> parameters = sqlGenerator.Parameters(
                        ds: ds,
                        entity: entity,
                        filter: fs,
                        sort: null,
                        paging: false,
                        columnName: null
                    );
                    foreach (string newParameter in parameters)
                    {
                        if (!result.Contains(item: newParameter))
                        {
                            result.Add(item: newParameter);
                        }
                    }
                }
            }
        }
        result.Sort();
        if (prefix != null)
        {
            for (int i = 0; i < result.Count; i++)
            {
                result[index: i] = prefix + result[index: i];
            }
        }
        return result;
    }
    #endregion
    private void ResolveServiceMethodCallTask(
        ServiceMethodCallTask task,
        out DataStructure ds,
        out DataStructureMethod method
    )
    {
        ISchemaItem dsParam = task.GetChildByName(name: "DataStructure");
        if (dsParam.ChildItems.Count == 1)
        {
            DataStructureReference dsRef = dsParam.ChildItems[index: 0] as DataStructureReference;
            if (dsRef != null)
            {
                ds = dsRef.DataStructure;
                method = dsRef.Method;
                return;
            }
        }
        ds = null;
        method = null;
    }

    private void ResolveXsltDataPage(
        XsltDataPage page,
        out DataStructure ds,
        out DataStructureMethod method
    )
    {
        ds = page.DataStructure;
        method = page.Method;
    }

    private void ResolveFileDownloadPage(
        FileDownloadPage page,
        out DataStructure ds,
        out DataStructureMethod method
    )
    {
        ds = page.DataStructure;
        method = page.Method;
    }

    private void ResolveFormReferenceMenuItem(
        FormReferenceMenuItem menu,
        out DataStructure ds,
        out DataStructureMethod method
    )
    {
        ds = menu.ListDataStructure ?? menu.Screen?.DataStructure;
        method = menu.ListMethod ?? menu.Method;
    }

    private void ResolveFilterLookup(
        EntityFilterLookupReference reference,
        out DataStructure ds,
        out DataStructureMethod method
    )
    {
        ds = reference.Lookup.ListDataStructure;
        method = null;
        DataServiceDataLookup dl = reference.Lookup as DataServiceDataLookup;
        if (dl != null)
        {
            method = dl.ListMethod;
        }
    }

    private void ResolveLookup(
        AbstractDataLookup reference,
        out DataStructure ds,
        out DataStructureMethod method
    )
    {
        ds = reference.ListDataStructure;
        method = null;
        DataServiceDataLookup dl = reference as DataServiceDataLookup;
        if (dl != null)
        {
            method = dl.ListMethod;
        }
    }

    private void ResolveReport(
        AbstractReport report,
        out DataStructure ds,
        out DataStructureMethod method
    )
    {
        AbstractDataReport dr = report as AbstractDataReport;
        if (dr != null)
        {
            ds = dr.DataStructure;
            method = dr.Method;
            return;
        }
        ds = null;
        method = null;
    }
}
