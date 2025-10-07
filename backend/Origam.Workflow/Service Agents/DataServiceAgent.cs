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
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
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
            dataStructureId,
            methodId,
            Guid.Empty,
            sortSetId
        );
        // Fill parameters
        if (parameters != null)
        {
            foreach (DictionaryEntry entry in parameters)
            {
                query.Parameters.Add(new QueryParameter(entry.Key as string, entry.Value));
            }
        }
        return DataDocumentFactory.New(LoadData(query, null));
    }

    private DataSet LoadData(DataStructureQuery query)
    {
        return LoadData(query, null);
    }

    private DataSet LoadData(DataStructureQuery query, DataSet data)
    {
        DataStructureMethod method;
        method =
            this.PersistenceProvider.RetrieveInstance(
                typeof(DataStructureMethod),
                new ModelElementKey(query.MethodId)
            ) as DataStructureMethod;
        if (method == null || method is DataStructureFilterSet)
        {
            if (data == null)
            {
                return _dataService.LoadDataSet(
                    query,
                    SecurityManager.CurrentPrincipal,
                    this.TransactionId
                );
            }
            else
            {
                return _dataService.LoadDataSet(
                    query,
                    SecurityManager.CurrentPrincipal,
                    data,
                    this.TransactionId
                );
            }
        }
        else if (method is DataStructureWorkflowMethod)
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
                    String.Format(
                        "The input data was sent to method {0}"
                            + "while there is no context store defined with `IsReturnValue' set for workflow {1},",
                        method.Id,
                        dataStructureWorkflowMethod.LoadWorkflowId
                    )
                );
            }
            if (data != null)
            // current data was sent, we have to pass them into main (output) contextstore of our workflow
            {
                query.Parameters.Add(new QueryParameter(returnContext.Name, data));
            }
            Object res = core.WorkflowService.ExecuteWorkflow(
                dataStructureWorkflowMethod.LoadWorkflowId,
                query.Parameters,
                this.TransactionId
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
                    String.Format(
                        "{0} (Try to change "
                            + " your workflow connected to DataStructureWorkflowMethod to return "
                            + "the same datastructure as the WorkflowMethod is connected to.)",
                        e.Message
                    )
                );
            }
        }
        else
        {
            throw new ArgumentException(
                String.Format(
                    "MethodId ({0}) schema item element not found or not of proper type",
                    method.Id
                )
            );
        }
    }

    private object GetScalarValue(DataStructureQuery query, ColumnsInfo columnsInfo)
    {
        DataStructureMethod method;
        method =
            this.PersistenceProvider.RetrieveInstance(
                typeof(DataStructureMethod),
                new ModelElementKey(query.MethodId)
            ) as DataStructureMethod;
        if (method == null || method is DataStructureFilterSet)
        {
            return _dataService.GetScalarValue(
                query,
                columnsInfo,
                SecurityManager.CurrentPrincipal,
                this.TransactionId
            );
        }
        else if (method is DataStructureWorkflowMethod)
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
                    String.Format(
                        "Return context store not found for workflow {0}.",
                        dataStructureWorkflowMethod.LoadWorkflowId
                    )
                );
            }
            else if (!returnContext.isScalar())
            {
                // return contextstore is not scalar
                throw new ArgumentException(
                    String.Format(
                        "Return context store is not of scalar type for workflow {0}",
                        dataStructureWorkflowMethod.LoadWorkflowId
                    )
                );
            }
            // call workflow
            return core.WorkflowService.ExecuteWorkflow(
                dataStructureWorkflowMethod.LoadWorkflowId,
                query.Parameters,
                this.TransactionId
            );
        }
        else
        {
            throw new ArgumentException(
                String.Format(
                    "MethodId ({0}) schema item element not found or not of proper type",
                    method.Id
                )
            );
        }
    }

    private IDataDocument ExecuteProcedure(string name, Hashtable parameters, string entityOrder)
    {
        DataStructureQuery query = new DataStructureQuery();
        if (this.OutputStructure != null)
        {
            query.DataSourceId = (Guid)this.OutputStructure.PrimaryKey["Id"];
        }

        if (parameters != null)
        {
            foreach (DictionaryEntry entry in parameters)
            {
                query.Parameters.Add(new QueryParameter(entry.Key as string, entry.Value));
            }
        }
        DataSet result = _dataService.ExecuteProcedure(
            name,
            entityOrder,
            query,
            this.TransactionId
        );
        if (result == null)
        {
            return null;
        }
        else
        {
            return DataDocumentFactory.New(result);
        }
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
            dataStructureId,
            methodId,
            Guid.Empty,
            sortSetId
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
                query,
                SecurityManager.CurrentPrincipal,
                dataset,
                TransactionId,
                forceBulkInsert
            );
        }
        catch (ConstraintException)
        {
            // make the exception far more verbose
            throw new ConstraintException(DatasetTools.GetDatasetErrors(dataset));
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
            _dataService.UpdateData(query, principal, data, TransactionId);
        }
        catch (ConstraintException)
        {
            // make the exception far more verbose
            throw new ConstraintException(DatasetTools.GetDatasetErrors(data));
        }
        return data;
    }

    private int UpdateReferences(Guid entityId, object oldValue, object newValue)
    {
        TableMappingItem originalEntity =
            this.PersistenceProvider.RetrieveInstance(
                typeof(TableMappingItem),
                new ModelElementKey(entityId)
            ) as TableMappingItem;
        if (originalEntity == null)
        {
            throw new ArgumentException(
                ResourceUtils.GetString("ErrorTableMappingNotFound", entityId.ToString())
            );
        }
        List<IDataEntityColumn> pkList = originalEntity.EntityPrimaryKey;
        if (pkList.Count == 0)
            throw new InvalidOperationException(
                ResourceUtils.GetString("ErrorNoPrimaryKey", originalEntity.Path)
            );
        if (pkList.Count > 1)
            throw new InvalidOperationException(
                ResourceUtils.GetString("ErrorMultiColumnPrimaryKey", originalEntity.Path)
            );
        FieldMappingItem originalKey = pkList[0] as FieldMappingItem;
        if (originalKey == null)
            throw new InvalidOperationException(
                ResourceUtils.GetString("ErrorPrimaryKeyNotMapped", originalEntity.Path)
            );
        SchemaService schema =
            ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
        EntityModelSchemaItemProvider entities =
            schema.GetProvider(typeof(EntityModelSchemaItemProvider))
            as EntityModelSchemaItemProvider;
        int result = 0;
        string transactionId = this.TransactionId;
        if (transactionId == null)
            transactionId = Guid.NewGuid().ToString();
        ITracingService trace =
            ServiceManager.Services.GetService(typeof(ITracingService)) as ITracingService;
        if (this.Trace)
        {
            trace.TraceStep(
                this.TraceWorkflowId,
                this.TraceStepName,
                this.TraceStepId,
                "Deduplication",
                "Start",
                originalEntity.MappedObjectName,
                "",
                "",
                "Old Id: "
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
                            && column.ForeignKeyEntity.PrimaryKey.Equals(originalEntity.PrimaryKey)
                            && column is FieldMappingItem
                        )
                        {
                            if (
                                column.ForeignKeyField != null
                                && column.ForeignKeyField.PrimaryKey.Equals(originalKey.PrimaryKey)
                            )
                            {
                                int records = this.DataService.UpdateField(
                                    table.Id,
                                    (Guid)column.PrimaryKey["Id"],
                                    oldValue,
                                    newValue,
                                    SecurityManager.CurrentPrincipal,
                                    transactionId
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
                                            log.Info(logText);
                                        }
                                        if (this.Trace)
                                        {
                                            trace?.TraceStep(
                                                this.TraceWorkflowId,
                                                this.TraceStepName,
                                                this.TraceStepId,
                                                "Deduplication",
                                                "Progress",
                                                originalEntity.MappedObjectName,
                                                "Audit: " + table.AuditingType,
                                                "",
                                                logText
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
                log.LogOrigamError("Updating references failed.", ex);
            }
            if (this.TransactionId == null)
            {
                ResourceMonitor.Rollback(transactionId);
            }
            throw;
        }
        if (this.TransactionId == null)
        {
            ResourceMonitor.Commit(transactionId);
        }

        return result;
    }

    private int ReferenceCount(Guid entityId, object value)
    {
        TableMappingItem originalEntity =
            this.PersistenceProvider.RetrieveInstance(
                typeof(TableMappingItem),
                new ModelElementKey(entityId)
            ) as TableMappingItem;
        if (originalEntity == null)
        {
            throw new ArgumentException(
                ResourceUtils.GetString("ErrorTableMappingNotFound", entityId.ToString())
            );
        }
        List<IDataEntityColumn> pkList = originalEntity.EntityPrimaryKey;
        if (pkList.Count == 0)
            throw new InvalidOperationException(
                ResourceUtils.GetString("ErrorNoPrimaryKey", originalEntity.Path)
            );
        if (pkList.Count > 1)
            throw new InvalidOperationException(
                ResourceUtils.GetString("ErrorMultiColumnPrimaryKey", originalEntity.Path)
            );
        FieldMappingItem originalKey = pkList[0] as FieldMappingItem;
        if (originalKey == null)
            throw new InvalidOperationException(
                ResourceUtils.GetString("ErrorPrimaryKeyNotMapped", originalEntity.Path)
            );
        List<IDataEntity> skipRelationships = originalEntity.ChildEntitiesRecursive;
        SchemaService schema =
            ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
        EntityModelSchemaItemProvider entities =
            schema.GetProvider(typeof(EntityModelSchemaItemProvider))
            as EntityModelSchemaItemProvider;
        int result = 0;
        string transactionId = this.TransactionId;
        ITracingService trace =
            ServiceManager.Services.GetService(typeof(ITracingService)) as ITracingService;
        if (this.Trace)
        {
            trace.TraceStep(
                this.TraceWorkflowId,
                this.TraceStepName,
                this.TraceStepId,
                "ReferenceCount",
                "",
                originalEntity.MappedObjectName,
                "",
                "",
                "Id: " + value.ToString()
            );
        }
        foreach (IDataEntity entity in entities.ChildItems)
        {
            if (!skipRelationships.Contains(entity))
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
                            && column.ForeignKeyEntity.PrimaryKey.Equals(originalEntity.PrimaryKey)
                            && column is FieldMappingItem
                        )
                        {
                            if (
                                column.ForeignKeyField != null
                                && column.ForeignKeyField.PrimaryKey.Equals(originalKey.PrimaryKey)
                            )
                            {
                                int records = this.DataService.ReferenceCount(
                                    table.Id,
                                    (Guid)column.PrimaryKey["Id"],
                                    value,
                                    SecurityManager.CurrentPrincipal,
                                    transactionId
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
        using (StreamReader sr = new StreamReader(s))
        {
            char[] buf = new char[s.Length];
            sr.ReadBlock(buf, 0, (int)s.Length - 1);
            result = new string(buf);
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
        return _dataService.ExecuteUpdate(command, transactionId);
    }

    public override void Run()
    {
        DataStructureReference dsRef;
        switch (this.MethodName)
        {
            case "LoadData":
            case "StoreData":
                dsRef = this.Parameters["DataStructure"] as DataStructureReference;
                Guid dsId;
                Guid methodId = Guid.Empty;
                Guid sortId = Guid.Empty;
                bool forceBulkInsert = false;
                // Check input parameters
                if (dsRef == null)
                {
                    if (this.Parameters["DataStructure"] != null)
                    {
                        throw new InvalidCastException(
                            ResourceUtils.GetString("ErrorNotDataStructureReference")
                        );
                    }
                    dsId = (Guid)this.OutputStructure.PrimaryKey["Id"];
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
                            this.Parameters["Parameters"] == null
                            || this.Parameters["Parameters"] is Hashtable
                        )
                    )
                        throw new InvalidCastException(
                            ResourceUtils.GetString("ErrorNotHashtable")
                        );
                    _result = this.LoadData(
                        dsId,
                        this.Parameters["Parameters"] as Hashtable,
                        methodId,
                        sortId
                    );
                }
                else // StoreData
                {
                    if (!(Parameters["Data"] is IDataDocument))
                        throw new InvalidCastException(
                            ResourceUtils.GetString("ErrorNotXmlDataDocument")
                        );
                    if (Parameters.Contains("ForceBulkInsert"))
                    {
                        forceBulkInsert = (bool)Parameters["ForceBulkInsert"];
                    }
                    _result = SaveData(
                        dsId,
                        methodId,
                        sortId,
                        Parameters["Data"] as IDataDocument,
                        forceBulkInsert
                    );
                }
                break;
            case "LoadDataByQuery":
                if (this.Parameters.Contains("Data"))
                {
                    if (!(this.Parameters["Data"] is DataSet))
                        throw new InvalidCastException("Data is not of type DataSet");
                    _result = this.LoadData(
                        this.Parameters["Query"] as DataStructureQuery,
                        this.Parameters["Data"] as DataSet
                    );
                }
                else
                {
                    _result = this.LoadData(this.Parameters["Query"] as DataStructureQuery);
                }
                break;
            case "GetScalarValueByQuery":
                _result = this.GetScalarValue(
                    this.Parameters["Query"] as DataStructureQuery,
                    new ColumnsInfo((string)this.Parameters["ColumnName"])
                );
                break;
            case "StoreDataByQuery":
                // Check input parameters
                if (!(this.Parameters["Query"] is DataStructureQuery))
                    throw new InvalidCastException("Query is not of type DataStructureQuery");
                if (!(this.Parameters["Data"] is DataSet))
                    throw new InvalidCastException("Data is not of type DataSet");
                _result = this.SaveData(
                    this.Parameters["Query"] as DataStructureQuery,
                    this.Parameters["Data"] as DataSet
                );
                break;
            case "ExecuteProcedure":
                // Check input parameters
                if (!(this.Parameters["Name"] is string))
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorNameNotString"));
                if (
                    !(
                        this.Parameters["Parameters"] == null
                        || this.Parameters["Parameters"] is Hashtable
                    )
                )
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorNotHashtable"));
                string entityOrder = null;
                if (this.Parameters.Contains("EntityOrder"))
                {
                    entityOrder = (string)this.Parameters["EntityOrder"];
                }
                _result = this.ExecuteProcedure(
                    (string)this.Parameters["Name"],
                    this.Parameters["Parameters"] as Hashtable,
                    entityOrder
                );
                break;
            case "UpdateReferences":
                // Check input parameters
                if (!(this.Parameters["EntityId"] is Guid))
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorEntityIdNotGuid"));
                _result = this.UpdateReferences(
                    (Guid)this.Parameters["EntityId"],
                    this.Parameters["oldValue"],
                    this.Parameters["newValue"]
                );
                break;
            case "ReferenceCount":
                // Check input parameters
                if (!(this.Parameters["EntityId"] is Guid))
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorEntityIdNotGuid"));
                _result = this.ReferenceCount(
                    (Guid)this.Parameters["EntityId"],
                    this.Parameters["Value"]
                );
                break;
            case "EntityDdl":
                _result = DataService.EntityDdl((Guid)Parameters["EntityId"]);
                break;
            case "FieldDdl":
                _result = DataService.FieldDdl((Guid)Parameters["FieldId"]);
                break;
            case "DatabaseSpecificDatatypes":
                _result = DataService.DatabaseSpecificDatatypes();
                break;
            case "ExecuteSql":
                _result = DataService.ExecuteUpdate(
                    (string)Parameters["Command"],
                    this.TransactionId
                );
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    "MethodName",
                    this.MethodName,
                    ResourceUtils.GetString("InvalidMethodName")
                );
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
                ResolveServiceMethodCallTask(call, out ds, out dsMethod);
            }
            else if (dataPage != null)
            {
                ResolveXsltDataPage(dataPage, out ds, out dsMethod);
            }
            else if (downloadPage != null)
            {
                ResolveFileDownloadPage(downloadPage, out ds, out dsMethod);
            }
            else if (formMenu != null)
            {
                ResolveFormReferenceMenuItem(formMenu, out ds, out dsMethod);
            }
            else if (report != null)
            {
                ResolveReport(report, out ds, out dsMethod);
            }
            else if (filterLookup != null)
            {
                ResolveFilterLookup(filterLookup, out ds, out dsMethod);
                // add "lookup" prefix to all parameters because as sub-queries they will
                // be prefixed
                prefix = "lookup";
            }
            else if (abstractDataLookup != null)
            {
                ResolveLookup(abstractDataLookup, out ds, out dsMethod);
            }
            if (ds != null)
            {
                DataStructureWorkflowMethod wm = dsMethod as DataStructureWorkflowMethod;
                DataStructureFilterSet fs = dsMethod as DataStructureFilterSet;
                if (wm != null)
                {
                    IBusinessServicesService agents =
                        ServiceManager.Services.GetService(typeof(IBusinessServicesService))
                        as IBusinessServicesService;
                    IServiceAgent agent = agents.GetAgent("WorkflowService", null, null);
                    return agent.ExpectedParameterNames(
                        wm.LoadWorkflow,
                        "ExecuteWorkflow",
                        "Parameters"
                    );
                }
                else
                {
                    var sqlGenerator = ((AbstractDataService)_dataService).DbDataAdapterFactory;
                    sqlGenerator.ResolveAllFilters = true;
                    foreach (DataStructureEntity entity in ds.Entities)
                    {
                        List<string> parameters = sqlGenerator.Parameters(
                            ds,
                            entity,
                            fs,
                            null,
                            false,
                            null
                        );
                        foreach (string newParameter in parameters)
                        {
                            if (!result.Contains(newParameter))
                            {
                                result.Add(newParameter);
                            }
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
                result[i] = prefix + result[i];
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
        ISchemaItem dsParam = task.GetChildByName("DataStructure");
        if (dsParam.ChildItems.Count == 1)
        {
            DataStructureReference dsRef = dsParam.ChildItems[0] as DataStructureReference;
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
