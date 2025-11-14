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
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using log4net;
using Microsoft.Data.SqlClient;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service.Generators;
using Origam.Extensions;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Services;
using Origam.Workbench.Services;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

namespace Origam.DA.Service;

#region Data Loader
internal class DataLoader
{
    private static readonly ILog log = LogManager.GetLogger(
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    public string ConnectionString = null;
    public DataStructureFilterSet FilterSet = null;
    public DataStructureSortSet SortSet = null;
    public DataStructureEntity Entity = null;
    public DataStructureQuery Query = null;
    public DataStructure DataStructure = null;
    public DataSet Dataset = null;
    public AbstractSqlDataService DataService = null;
    public IDbTransaction Transaction = null;
    public string TransactionId;
    public UserProfile CurrentProfile = null;
    public int Timeout;

    public void Fill()
    {
        IDbConnection connection =
            (Transaction == null)
                ? DataService.GetConnection(ConnectionString)
                : Transaction.Connection;
        try
        {
            DbDataAdapter adapter;
            switch (Query.DataSourceType)
            {
                case QueryDataSourceType.DataStructure:
                {
                    var selectParameters = new SelectParameters
                    {
                        DataStructure = DataStructure,
                        Entity = Entity,
                        Filter = FilterSet,
                        SortSet = SortSet,
                        Parameters = Query.Parameters.ToHashtable(),
                        Paging = Query.Paging,
                        ColumnsInfo = Query.ColumnsInfo,
                        AggregatedColumns = Query.AggregatedColumns,
                    };
                    adapter = DataService.GetAdapter(selectParameters, CurrentProfile);
                    break;
                }
                case QueryDataSourceType.DataStructureEntity:
                {
                    adapter = DataService.GetSelectRowAdapter(Entity, FilterSet, Query.ColumnsInfo);
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(
                        "DataSourceType",
                        Query.DataSourceType,
                        ResourceUtils.GetString("UnknownDataSource")
                    );
                }
            }
            IDbDataAdapter dbDataAdapter = adapter;
            dbDataAdapter.SelectCommand.Connection = connection;
            dbDataAdapter.SelectCommand.Transaction = Transaction;
            dbDataAdapter.SelectCommand.CommandTimeout = Timeout;
            // ignore any extra fields returned by the select statement
            // e.g. RowNum returned when paging is turned on
            adapter.MissingMappingAction = MissingMappingAction.Ignore;
            DataService.BuildParameters(
                Query.Parameters,
                dbDataAdapter.SelectCommand.Parameters,
                CurrentProfile
            );
            ProfiledDbDataAdapter profiledAdapter = new ProfiledDbDataAdapter(
                dbDataAdapter,
                MiniProfiler.Current
            );
            try
            {
                if (Transaction == null)
                {
                    connection.Open();
                }
                Dataset.Tables[Entity.Name].BeginLoadData();
                DataService.TraceCommand(profiledAdapter.SelectCommand, TransactionId);
                profiledAdapter.Fill(Dataset);
                Dataset.Tables[Entity.Name].EndLoadData();
            }
            catch (Exception ex)
            {
                HandleException(
                    exception: ex,
                    commandText: dbDataAdapter.SelectCommand.CommandText,
                    logAsDebug: (Entity.Name == "AsapModelVersion")
                        && (ex is SqlException)
                        && (ex.HResult == -2146232060)
                );
            }
            finally
            {
                profiledAdapter.Dispose();
                ((IDbDataAdapter)adapter).SelectCommand.Transaction = null;
                ((IDbDataAdapter)adapter).SelectCommand.Connection = null;
            }
        }
        finally
        {
            try
            {
                if (Transaction == null)
                {
                    connection.Close();
                }
            }
            catch { }
            if (Transaction == null)
            {
                connection.Dispose();
            }
        }
    }

    private void HandleException(Exception exception, string commandText, bool logAsDebug)
    {
        if (log.IsErrorEnabled && !logAsDebug)
        {
            log.LogOrigamError($"{exception.Message}, SQL: {commandText}", exception);
        }
        else if (log.IsDebugEnabled && logAsDebug)
        {
            log.Debug($"{exception.Message}, SQL: {commandText}", exception);
        }
        var standardMessage = ResourceUtils.GetString(
            "ErrorLoadingData",
            (Entity.EntityDefinition as TableMappingItem)?.MappedObjectName,
            Entity.Name,
            Environment.NewLine,
            exception.Message
        );
        DataService.HandleException(exception, standardMessage, null);
    }
}
#endregion
// version of log4net for NetStandard 1.3 does not have the method
// LogManager.GetLogger(string)... have to use the overload with Type
// as parameter
public class ConcurrencyExceptionLogger { }

public abstract class AbstractSqlDataService : AbstractDataService
{
    private readonly Profiler profiler = new Profiler();

    private static readonly ILog log = LogManager.GetLogger(
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );

    // Special logger for concurrency exception detail logging
    private static readonly ILog concurrencyLog = LogManager.GetLogger(
        typeof(ConcurrencyExceptionLogger)
    );
    private IDbDataAdapterFactory adapterFactory;
    private string connectionString = "";
    private const int DATA_VISUALIZATION_MAX_LENGTH = 100;

    #region Constructors
    public AbstractSqlDataService() { }

    public AbstractSqlDataService(string connection, int bulkInsertThreshold, int updateBatchSize)
    {
        connectionString = connection;
        UpdateBatchSize = updateBatchSize;
        BulkInsertThreshold = bulkInsertThreshold;
    }
    #endregion
    public override string ConnectionString
    {
        get => connectionString;
        set => connectionString = value;
    }
    public override IDbDataAdapterFactory DbDataAdapterFactory
    {
        get => adapterFactory;
        internal set => adapterFactory = value;
    }

    #region Public Methods

    public override void DiagnoseConnection()
    {
        using IDbConnection connection = GetConnection(ConnectionString);
        connection.Open();
    }

    public abstract string CreateSystemRole(string roleName);

    public abstract string CreateInsert(int fieldCount);

    internal override IDbTransaction GetTransaction(
        string transactionId,
        IsolationLevel isolationLevel
    )
    {
        IDbTransaction transaction;
        if (transactionId == null)
        {
            var connection = GetConnection(ConnectionString);
            connection.Open();
            transaction = connection.BeginTransaction(isolationLevel);
        }
        else
        {
            if (
                !(
                    ResourceMonitor.GetTransaction(transactionId, ConnectionString)
                    is OrigamDbTransaction origamDbTransaction
                )
            )
            {
                transaction = GetTransaction(null, isolationLevel);
                ResourceMonitor.RegisterTransaction(
                    transactionId,
                    ConnectionString,
                    new OrigamDbTransaction(transaction)
                );
            }
            else
            {
                transaction = origamDbTransaction.Transaction;
                if (transaction.IsolationLevel != isolationLevel)
                {
                    throw new Exception(
                        "Existing transaction has a different isolation level then the current query. When using a different isolation level the query has to be executed under a different or no transaction."
                    );
                }
            }
        }
        return transaction;
    }

    public override DataSet LoadDataSet(
        DataStructureQuery dataStructureQuery,
        IPrincipal principal,
        string transactionId
    )
    {
        var loadedDataSet = LoadDataSet(dataStructureQuery, principal, null, transactionId);
        profiler.LogRememberedExecutionTimes();
        return loadedDataSet;
    }

    public override DataSet LoadDataSet(
        DataStructureQuery query,
        IPrincipal principal,
        DataSet dataset,
        string transactionId
    )
    {
        var settings = ConfigurationManager.GetActiveConfiguration();
        int timeout = settings.DataServiceSelectTimeout;
        UserProfile currentProfile = null;
        if (query.LoadByIdentity)
        {
            currentProfile =
                SecurityManager.GetProfileProvider().GetProfile(principal.Identity) as UserProfile;
        }
        if (PersistenceProvider == null)
        {
            throw new NullReferenceException(ResourceUtils.GetString("NoProviderForMS"));
        }
        List<DataStructureEntity> entities;
        DataStructure dataStructure = null;
        DataStructureFilterSet filterSet = null;
        DataStructureSortSet sortSet = null;
        switch (query.DataSourceType)
        {
            case QueryDataSourceType.DataStructure:
            {
                dataStructure = GetDataStructure(query);
                filterSet = GetFilterSet(query.MethodId);
                sortSet = GetSortSet(query.SortSetId);
                if (string.IsNullOrEmpty(query.Entity))
                {
                    entities = dataStructure.Entities;
                }
                else
                {
                    entities = new List<DataStructureEntity>();
                    foreach (DataStructureEntity dataStructureEntity in dataStructure.Entities)
                    {
                        if (dataStructureEntity.Name == query.Entity)
                        {
                            entities.Add(dataStructureEntity);
                            break;
                        }
                    }
                    if (entities.Count == 0)
                    {
                        throw new ArgumentOutOfRangeException(
                            $"Entity {query.Entity} not found in data structure {dataStructure.Path}."
                        );
                    }
                }
                if (dataset == null)
                {
                    dataset = GetDataset(dataStructure, query.DefaultSetId);
                }
                break;
            }
            case QueryDataSourceType.DataStructureEntity:
            {
                entities = new List<DataStructureEntity>();
                filterSet = GetFilterSet(query.MethodId);
                entities.Add(GetDataStructureEntity(query));
                if (dataset == null)
                {
                    throw new NullReferenceException(
                        ResourceUtils.GetString("DataSetMustBeProvided")
                    );
                }
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(
                    "DataSourceType",
                    query.DataSourceType,
                    ResourceUtils.GetString("UnknownDataSource")
                );
            }
        }
        IDictionary<DataColumn, string> expressions = DatasetTools.RemoveExpressions(dataset, true);
        if ((dataset.Tables.Count > 1) && query.Paging)
        {
            throw new Exception("Paging is allowed only on data structures with a single entity.");
        }
        bool enforceConstraints = dataset.EnforceConstraints;
        dataset.EnforceConstraints = false;
        foreach (DataStructureEntity entity in entities)
        {
            if (LoadWillReturnZeroResults(dataset, entity, query.DataSourceType))
            {
                continue;
            }
            // Skip self joins, they are just relations, not really entities
            if (
                (entity.Columns.Count > 0)
                && !(entity.Entity is IAssociation association && association.IsSelfJoin)
            )
            {
                profiler.ExecuteAndRememberLoadDuration(
                    entity: entity,
                    actionToExecute: () =>
                    {
                        var loader = new DataLoader
                        {
                            ConnectionString = connectionString,
                            DataService = this,
                            Dataset = dataset,
                            TransactionId = transactionId,
                        };
                        if (transactionId != null)
                        {
                            loader.Transaction = GetTransaction(
                                transactionId,
                                query.IsolationLevel
                            );
                        }
                        loader.DataStructure = dataStructure;
                        loader.Entity = entity;
                        loader.FilterSet = filterSet;
                        loader.SortSet = sortSet;
                        loader.Query = query;
                        loader.Timeout = timeout;
                        loader.CurrentProfile = currentProfile;
                        loader.Fill();
                    }
                );
            }
        }
        if (query.EnforceConstraints)
        {
            try
            {
                dataset.EnforceConstraints = enforceConstraints;
            }
            catch (Exception ex)
            {
                try
                {
                    log.LogOrigamError(DebugClass.ListRowErrors(dataset), ex);
                    using (
                        var writer = System.IO.File.CreateText(
                            AppDomain.CurrentDomain.BaseDirectory
                                + @"\debug\"
                                + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-")
                                + DateTime.Now.Ticks
                                + "___MsSqlDataService_error.txt"
                        )
                    )
                    {
                        writer.WriteLine(DebugClass.ListRowErrors(dataset));
                        writer.Close();
                    }
                }
                catch { }
                throw;
            }
        }
        DatasetTools.SetExpressions(expressions);
        return dataset;
    }

    private bool LoadWillReturnZeroResults(
        DataSet dataset,
        DataStructureEntity entity,
        QueryDataSourceType dataSourceType
    )
    {
        DataStructureEntity rootEntity = entity.RootEntity;
        if (dataSourceType == QueryDataSourceType.DataStructureEntity)
        {
            return false;
        }
        if (rootEntity == entity)
        {
            return false;
        }
        if (entity.RelationType != RelationType.Normal)
        {
            return false;
        }
        if (!(rootEntity.Entity is TableMappingItem mappingItem))
        {
            return false;
        }
        string rootEntityTableName = mappingItem.MappedObjectName;
        DataTable rootTable = dataset
            .Tables.Cast<DataTable>()
            .FirstOrDefault(table => table.TableName == rootEntityTableName);
        return (rootTable != null) && (rootTable.Rows.Count == 0);
    }

    public override int UpdateData(
        DataStructureQuery query,
        IPrincipal userProfile,
        DataSet dataset,
        string transactionId
    )
    {
        return UpdateData(query, userProfile, dataset, transactionId, false);
    }

    public override int UpdateData(
        DataStructureQuery query,
        IPrincipal userProfile,
        DataSet dataset,
        string transactionId,
        bool forceBulkInsert
    )
    {
        if (log.IsDebugEnabled)
        {
            log.RunHandled(() =>
            {
                log.DebugFormat("UpdateData; Data Structure Id: {0}", query.DataSourceId);
                var stringBuilder = new StringBuilder();
                var stringWriter = new System.IO.StringWriter(stringBuilder);
                dataset.WriteXml(stringWriter, XmlWriteMode.DiffGram);
                log.DebugFormat("UpdateData; {0}", stringBuilder);
            });
        }
        var newTransaction = (transactionId == null);
        if (transactionId == null)
        {
            transactionId = Guid.NewGuid().ToString();
        }
        // If there is nothing to update, we quit immediately
        if (!dataset.HasChanges())
        {
            return 0;
        }
        UserProfile profile = null;
        if (query.LoadByIdentity)
        {
            profile =
                SecurityManager.GetProfileProvider().GetProfile(userProfile.Identity)
                as UserProfile;
        }
        var stateMachine = StateMachine;
        DataSet changedDataset = dataset;
        DataStructure dataStructure = GetDataStructure(query);
        if (dataStructure.IsLocalized)
        {
            throw new OrigamException(
                $"Couldn't update localized data structure `{dataStructure.Name}' ({dataStructure.Id})"
            );
        }
        IDbTransaction transaction = GetTransaction(transactionId, query.IsolationLevel);
        IDbConnection connection = transaction.Connection;
        var currentEntityName = "";
        var lastTableName = "";
        List<DataStructureEntity> entities = dataStructure.Entities;
        var changedTables = new List<string>();
        var deletedRowIds = new List<Guid>();
        var rowStates = new[] { DataRowState.Deleted, DataRowState.Added, DataRowState.Modified };
        DataTable changedTable = null;
        try
        {
            foreach (var rowState in rowStates)
            {
                var actualEntities = entities;
                // for delete use reverse entity order
                if (rowState == DataRowState.Deleted)
                {
                    actualEntities = new List<DataStructureEntity>(entities.Count);
                    for (var i = entities.Count - 1; i >= 0; i--)
                    {
                        actualEntities.Add(entities[i]);
                    }
                }
                foreach (DataStructureEntity entity in actualEntities)
                {
                    currentEntityName = entity.Name;
                    // We check if the dataset actually contains the entity.
                    // E.g. for self-joins the entity name is not contained
                    // in the dataset but that does not matter
                    // because we save such an entity once anyway.
                    if (
                        !(entity.EntityDefinition is TableMappingItem tableMapping)
                        || !changedDataset.Tables.Contains(currentEntityName)
                    )
                    {
                        continue;
                    }
                    lastTableName = dataset
                        .Tables[currentEntityName]
                        .DisplayExpression.Replace("'", "");
                    if (lastTableName == "")
                    {
                        lastTableName = currentEntityName;
                    }
                    // we clone the table because if without complete dataset,
                    // some expressions might not work if they reference
                    // other entities
                    changedTable = DatasetTools.CloneTable(
                        changedDataset.Tables[entity.Name],
                        false
                    );
                    foreach (DataRow row in changedDataset.Tables[entity.Name].Rows)
                    {
                        if (row.RowState == rowState)
                        {
                            // Constraints might fail right here if the
                            // source dataset has constraints turned off,
                            // e.g. through a flag in a sequential workflow.
                            // That is OK. Otherwise they would fail
                            // in the database.
                            changedTable.ImportRow(row);
                        }
                    }
                    if (tableMapping.DatabaseObjectType == DatabaseMappingObjectType.Table)
                    {
                        if (changedTable != null)
                        {
                            if ((stateMachine != null) && query.FireStateMachineEvents)
                            {
                                stateMachine.OnDataChanging(changedTable, transactionId);
                            }
                        }
                        var rowCount = changedTable.Rows.Count;
                        if (rowCount > 0)
                        {
                            profiler.ExecuteAndLogStoreActionDuration(
                                entity: entity,
                                actionToExecute: () =>
                                {
                                    ExecuteUpdate(
                                        query,
                                        transactionId,
                                        profile,
                                        dataStructure,
                                        transaction,
                                        connection,
                                        deletedRowIds,
                                        changedTable,
                                        rowState,
                                        entity,
                                        rowCount,
                                        forceBulkInsert
                                    );
                                }
                            );
                        }
                    }
                    // finally add the table to a list of changed tables so
                    // changes can be accepted later
                    if (!changedTables.Contains(changedTable.TableName))
                    {
                        changedTables.Add(changedTable.TableName);
                    }
                }
            }
            // execute the state machine events
            if ((stateMachine != null) && query.FireStateMachineEvents)
            {
                stateMachine.OnDataChanged(dataset, changedTables, transactionId);
            }
            // accept changes
            AcceptChanges(dataset, changedTables, query, transactionId, userProfile);
            profiler.LogRememberedExecutionTimes();
            // delete attachments if any
            if ((deletedRowIds.Count > 0) && query.SynchronizeAttachmentsOnDelete)
            {
                var attachmentService = ServiceManager.Services.GetService<IAttachmentService>();
                foreach (Guid recordId in deletedRowIds)
                {
                    attachmentService.RemoveAttachment(recordId, transactionId);
                }
            }
            if (newTransaction)
            {
                ResourceMonitor.Commit(transactionId);
            }
        }
        catch (DBConcurrencyException ex)
        {
            if (log.IsErrorEnabled)
            {
                log.Error(
                    "DBConcurrencyException occurred! See \"Origam.DA.Service.ConcurrencyExceptionLogger\" logger (Debug mode) for more details"
                );
            }
            if (concurrencyLog.IsDebugEnabled)
            {
                concurrencyLog.DebugFormat(
                    "Concurrency exception data structure query details: ds: `{0}', method: `{1}', sortSet: `{2}', default set: `{3}'",
                    query.DataSourceId,
                    query.MethodId,
                    query.SortSetId,
                    query.DefaultSetId
                );
            }
            var errorString = ComposeConcurrencyErrorMessage(
                userProfile,
                dataset,
                transactionId,
                currentEntityName,
                lastTableName,
                ex
            );
            if (newTransaction)
            {
                ResourceMonitor.Rollback(transactionId);
                transactionId = null;
            }

            // log before throw (because there are some place(s)
            // where the exception isn't caught
            if (concurrencyLog.IsDebugEnabled)
            {
                concurrencyLog.DebugFormat("Concurrency exception details: {0}", errorString);
            }
            throw new DBConcurrencyException(errorString, ex);
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError("Update failed", ex);
            }
            if (newTransaction)
            {
                ResourceMonitor.Rollback(transactionId);
            }
            ComposeGeneralErrorMessage(lastTableName, changedTable, ex);
            throw;
        }
        finally
        {
            if (transactionId == null)
            {
                connection.Close();
                connection.Dispose();
            }
        }
        return 0;
    }

    private void ComposeGeneralErrorMessage(
        string lastTableName,
        DataTable changedTable,
        Exception exception
    )
    {
        DataRow errorRow = DatasetTools.GetErrorRow(changedTable);
        if (errorRow == null)
        {
            return;
        }
        string rowErrorMessage = null;
        var operation = "";
        switch (errorRow.RowState)
        {
            case DataRowState.Added:
            {
                operation = ResourceUtils.GetString("ErrorCouldNotAddRow");
                break;
            }
            case DataRowState.Deleted:
            {
                operation = ResourceUtils.GetString("ErrorCouldNotDeleteRow");
                break;
            }
            case DataRowState.Modified:
            {
                operation = ResourceUtils.GetString("ErrorCouldNotModifyRow");
                break;
            }
        }
        var recordDescription = DatasetTools.GetRowDescription(errorRow);
        if (recordDescription != null)
        {
            recordDescription = " " + recordDescription;
        }
        rowErrorMessage = string.Format(operation, lastTableName, recordDescription);
        HandleException(exception, rowErrorMessage, errorRow);
    }

    private string ComposeConcurrencyErrorMessage(
        IPrincipal userProfile,
        DataSet dataset,
        string transactionId,
        string currentEntityName,
        string lastTableName,
        DBConcurrencyException exception
    )
    {
        var concurrentUserName = "";
        var errorString = "";
        try
        {
            DataTable table = dataset.Tables[currentEntityName];
            // if the row in the queue is being deleted we will not find it
            // in the original data
            // in that case we will use the row provided by the event
            // which contains the data
            DataRow row = table.Rows.Find(DatasetTools.PrimaryKey(exception.Row)) ?? exception.Row;
            // load the existing row from the database to see what changes
            // have been made by the other user
            DataSet storedData = CloneDatasetForActualRow(table);
            var entityId = (Guid)table.ExtendedProperties["Id"];
            var entity = GetEntity(entityId);
            var rowName = DatasetTools.PrimaryKey(row)[0].ToString();
            IDataEntityColumn describingField = entity.EntityDefinition.DescribingField;
            if ((describingField != null) && table.Columns.Contains(describingField.Name))
            {
                if (!row.IsNull(describingField.Name))
                {
                    rowName = row[describingField.Name].ToString();
                }
            }
            LoadActualRow(storedData, entityId, Guid.Empty, row, userProfile, transactionId);
            DataTable storedTable = storedData.Tables[currentEntityName];
            if (storedTable.Rows.Count == 0)
            {
                errorString = ResourceUtils.GetString(
                    "DataDeletedByOtherUserException",
                    rowName,
                    lastTableName
                );
            }
            else
            {
                DataRow storedRow = storedTable.Rows[0];
                if (
                    storedTable.Columns.Contains("RecordUpdatedBy")
                    && !storedRow.IsNull("RecordUpdatedBy")
                )
                {
                    var concurrentProfile =
                        SecurityManager
                            .GetProfileProvider()
                            .GetProfile((Guid)storedRow["RecordUpdatedBy"]) as UserProfile;
                    concurrentUserName = concurrentProfile.FullName;
                }
                errorString = ResourceUtils.GetString(
                    "DataChangedByOtherUserException",
                    rowName,
                    lastTableName,
                    concurrentUserName
                );
                foreach (DataColumn column in row.Table.Columns)
                {
                    IDataEntityColumn field = GetField((Guid)column.ExtendedProperties["Id"]);
                    if ((column.ColumnName == "RecordUpdatedBy") || !(field is FieldMappingItem))
                    {
                        continue;
                    }
                    string storedValue = null;
                    string myValue = null;
                    if (!storedRow.IsNull(column.ColumnName))
                    {
                        storedValue = storedRow[column.ColumnName].ToString();
                    }
                    if (!row.IsNull(column, DataRowVersion.Original))
                    {
                        myValue = row[column, DataRowVersion.Original].ToString();
                    }
                    if (
                        (storedValue != null && storedValue.Equals(myValue))
                        || (storedValue == null && myValue == null)
                    )
                    {
                        continue;
                    }
                    if (column.ExtendedProperties.Contains(Const.DefaultLookupIdAttribute))
                    {
                        var lookupService =
                            ServiceManager.Services.GetService<IDataLookupService>();
                        // this is a lookup column,
                        // we pass the looked-up value
                        var lookupId = (Guid)
                            column.ExtendedProperties[Const.DefaultLookupIdAttribute];
                        if (myValue != "")
                        {
                            myValue = lookupService
                                .GetDisplayText(lookupId, myValue, transactionId)
                                .ToString();
                        }
                        if (storedValue != "")
                        {
                            storedValue = lookupService
                                .GetDisplayText(lookupId, storedValue, transactionId)
                                .ToString();
                        }
                    }
                    errorString +=
                        Environment.NewLine
                        + "- "
                        + column.Caption
                        + ": "
                        + (myValue ?? "null")
                        + " > "
                        + (storedValue ?? "null");
                }
            }
            while (row.Table.ParentRelations.Count > 0)
            {
                row = row.GetParentRow(row.Table.ParentRelations[0]);
                row.RowError = ResourceUtils.GetString("ChildErrors");
            }
        }
        catch { }
        return errorString;
    }

    private void ExecuteUpdate(
        DataStructureQuery query,
        string transactionId,
        UserProfile profile,
        DataStructure dataStructure,
        IDbTransaction transaction,
        IDbConnection connection,
        List<Guid> deletedRowIds,
        DataTable changedTable,
        DataRowState rowState,
        DataStructureEntity entity,
        int rowCount,
        bool forceBulkInsert
    )
    {
        // LOGGING
        LogData(changedTable, profile, transactionId, connection, transaction);
        if (
            (forceBulkInsert || ((BulkInsertThreshold != 0) && (rowCount > BulkInsertThreshold)))
            && (rowState == DataRowState.Added)
        )
        {
            BulkInsert(entity, connection, transaction, changedTable);
            if (log.IsInfoEnabled)
            {
                log.Info(
                    "BulkCopy; Entity: "
                        + changedTable?.TableName
                        + ", "
                        + rowState
                        + " "
                        + rowCount
                        + " row(s). Transaction id: "
                        + transactionId
                );
            }
        }
        else
        {
            DataStructureFilterSet filter = GetFilterSet(query.MethodId);
            DataStructureSortSet sortSet = GetSortSet(query.SortSetId);
            // CONFIGURE DATA ADAPTER
            var adapterParameters = new SelectParameters
            {
                DataStructure = dataStructure,
                Entity = entity,
                Filter = filter,
                SortSet = sortSet,
                Parameters = query.Parameters.ToHashtable(),
                Paging = false,
            };
            DbDataAdapter adapter = GetAdapter(adapterParameters, profile);
            SetConnection(adapter, connection);
            SetTransaction(adapter, transaction);
            if (UpdateBatchSize != 0)
            {
                adapter.InsertCommand.UpdatedRowSource = UpdateRowSource.None;
                adapter.UpdateCommand.UpdatedRowSource = UpdateRowSource.None;
                adapter.DeleteCommand.UpdatedRowSource = UpdateRowSource.None;
                adapter.UpdateBatchSize = UpdateBatchSize;
            }
            switch (rowState)
            {
                // EXECUTE THE UPDATE
                case DataRowState.Modified:
                {
                    TraceCommand(((IDbDataAdapter)adapter).UpdateCommand, transactionId);
                    break;
                }
                case DataRowState.Deleted:
                {
                    TraceCommand(((IDbDataAdapter)adapter).DeleteCommand, transactionId);
                    // remember row in order to delete an attachment later
                    // at the end of updateData
                    if (
                        (changedTable.PrimaryKey.Length == 1)
                        && (changedTable.PrimaryKey[0].DataType == typeof(Guid))
                    )
                    {
                        // entity has a primary key Id taken from IOrigamEntity2
                        foreach (DataRow row in changedTable.Rows)
                        {
                            var id = (Guid)
                                row[changedTable.PrimaryKey[0].ColumnName, DataRowVersion.Original];
                            deletedRowIds.Add(id);
                        }
                    }
                    break;
                }
                case DataRowState.Added:
                {
                    TraceCommand(((IDbDataAdapter)adapter).InsertCommand, transactionId);
                    break;
                }
            }
            int result = adapter.Update(changedTable);
            if (log.IsInfoEnabled)
            {
                log.Info(
                    "UpdateData; Entity: "
                        + changedTable.TableName
                        + ", "
                        + rowState
                        + " "
                        + result
                        + " row(s). Transaction id: "
                        + transactionId
                );
            }
            // FREE THE ADAPTER
            SetTransaction(adapter, null);
            SetConnection(adapter, null);
            // CHECK CONCURRENCY VIOLATION
            if (result != rowCount)
            {
                throw new DBConcurrencyException(
                    ResourceUtils.GetString("ConcurrencyViolation", changedTable.TableName)
                );
            }
        }
    }

    internal virtual void BulkInsert(
        DataStructureEntity entity,
        IDbConnection connection,
        IDbTransaction transaction,
        DataTable table
    )
    {
        throw new NotImplementedException();
    }

    private Hashtable GetScalarCommandCache()
    {
        Hashtable context = OrigamUserContext.Context;
        lock (context)
        {
            if (!context.Contains("ScalarCommandCache"))
            {
                context.Add("ScalarCommandCache", new Hashtable());
            }
        }
        return (Hashtable)OrigamUserContext.Context["ScalarCommandCache"];
    }

    public override object GetScalarValue(
        DataStructureQuery query,
        ColumnsInfo columnsInfo,
        IPrincipal principal,
        string transactionId
    )
    {
        IDbCommand command;
        object result = null;
        UserProfile currentProfile = null;
        if (query.LoadByIdentity)
        {
            currentProfile =
                SecurityManager.GetProfileProvider().GetProfile(principal.Identity) as UserProfile;
        }
        DataStructure dataStructure = GetDataStructure(query);
        DataStructureFilterSet filterset = GetFilterSet(query.MethodId);
        var cacheId =
            query.DataSourceId.ToString()
            + query.MethodId.ToString()
            + query.SortSetId.ToString()
            + columnsInfo;
        Hashtable cache = GetScalarCommandCache();
        if (cache.Contains(cacheId) && filterset is not { IsDynamic: true })
        {
            command = (IDbCommand)cache[cacheId];
            command = DbDataAdapterFactory.CloneCommand(command);
        }
        else
        {
            command = DbDataAdapterFactory.ScalarValueCommand(
                dataStructure,
                filterset,
                GetSortSet(query.SortSetId),
                columnsInfo,
                query.Parameters.ToHashtable()
            );
            if (filterset is not { IsDynamic: true })
            {
                lock (cache)
                {
                    cache[cacheId] = command;
                }
            }
        }
        IDbTransaction transaction = null;
        IDbConnection connection;
        if (transactionId == null)
        {
            connection = GetConnection(ConnectionString);
            connection.Open();
        }
        else
        {
            transaction = GetTransaction(transactionId, query.IsolationLevel);
            connection = transaction.Connection;
        }

        ProfiledDbCommand profiledDbCommand = null;
        try
        {
            BuildParameters(query.Parameters, command.Parameters, currentProfile);
            command.Connection = connection;
            command.Transaction = transaction;
            TraceCommand(command, transactionId);
            profiledDbCommand = new ProfiledDbCommand(
                command as DbCommand,
                (command as DbCommand).Connection,
                MiniProfiler.Current
            );
            result = profiledDbCommand.ExecuteScalar();
            var dataType = OrigamDataType.Xml;
            foreach (
                DataStructureColumn column in (
                    dataStructure.Entities[0] as DataStructureEntity
                ).Columns
            )
            {
                if (column.Name == columnsInfo?.ToString())
                {
                    DataStructureColumn finalColumn = column.FinalColumn;
                    dataType =
                        (column.Aggregation == AggregationType.Count)
                            ? OrigamDataType.Long
                            : finalColumn.DataType;
                    break;
                }
            }
            switch (dataType)
            {
                case OrigamDataType.UniqueIdentifier:
                {
                    if (result is string stringResult)
                    {
                        result = new Guid(stringResult);
                    }
                    break;
                }
                case OrigamDataType.Boolean:
                {
                    if (result is int)
                    {
                        result = (result.Equals(1));
                    }
                    break;
                }
                case OrigamDataType.Long:
                {
                    if (result is int intResult)
                    {
                        result = (long)intResult;
                    }
                    break;
                }
            }
            // Reset the transaction isolation level to its default. See the following from MSDN:
            // =====================================================================================
            // Note   After a transaction is committed or rolled back, the isolation level
            // of the transaction persists for all subsequent commands that are in autocommit mode
            // (the Microsoft SQL Server default). This can produce unexpected results,
            // such as an isolation level of Repeatable read persisting and locking other users out
            // of a row. To reset the isolation level to the default (Read committed), execute
            // the Transact-SQL SET TRANSACTION ISOLATION LEVEL READ COMMITTED statement,
            // or call SqlConnection.BeginTransaction followed immediately by SqlTransaction.Commit.
            // For more information about isolation levels, see SQL Server Books Online.
            ResetTransactionIsolationLevel(profiledDbCommand);
        }
        catch (Exception ex)
        {
            throw new DataException(
                ResourceUtils.GetString("ErrorWhenScalar", Environment.NewLine + ex.Message),
                ex
            );
        }
        finally
        {
            profiledDbCommand?.Dispose();
            if (transactionId == null)
            {
                try
                {
                    connection.Close();
                }
                finally
                {
                    connection.Dispose();
                }
            }
        }
        return result;
    }

    protected abstract void ResetTransactionIsolationLevel(IDbCommand command);

    public override DataSet ExecuteProcedure(
        string name,
        string entityOrder,
        DataStructureQuery query,
        string transactionId
    )
    {
        var settings = ConfigurationManager.GetActiveConfiguration();
        DataStructure dataStructure = GetDataStructure(query);
        DataSet result = null;
        if (dataStructure != null)
        {
            result = GetDataset(dataStructure, Guid.Empty);
        }
        IDbTransaction transaction = null;
        IDbConnection connection;
        if (transactionId == null)
        {
            connection = GetConnection(ConnectionString);
            connection.Open();
        }
        else
        {
            transaction = GetTransaction(transactionId, query.IsolationLevel);
            connection = transaction.Connection;
        }
        try
        {
            DbDataAdapter adapter = null;
            IDbCommand command = null;
            UserProfile profile = null;
            if (query.LoadByIdentity)
            {
                profile = SecurityManager.CurrentUserProfile();
            }
            try
            {
                // no output data structure - no results - execute non-query
                if (result == null)
                {
                    command = ExecuteNonQuery(
                        name: name,
                        parameters: query.Parameters,
                        connection: connection,
                        transaction: transaction,
                        timeOut: settings.DataServiceExecuteProcedureTimeout
                    );
                }
                // results present - we take the first entity of the output
                // data structure and fill the results into it
                else
                {
                    // make a sorted list of entities that corresponds to
                    // an output from SP
                    List<DataStructureEntity> entitiesOrdered;
                    if (entityOrder == null)
                    {
                        entitiesOrdered = dataStructure.Entities;
                    }
                    else
                    {
                        entitiesOrdered = new List<DataStructureEntity>();
                        foreach (string entityName in entityOrder.Split(';'))
                        {
                            var found = false;
                            foreach (
                                DataStructureEntity dataStructureEntity in dataStructure.Entities
                            )
                            {
                                if (dataStructureEntity.Name == entityName)
                                {
                                    entitiesOrdered.Add(dataStructureEntity);
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                throw new ArgumentException(
                                    $"Entity `{entityName}' defined in EntityOrder `{entityOrder}'"
                                        + $"not found in a datastructure {dataStructure.Id.ToString()}"
                                );
                            }
                        }
                    }
                    adapter = DbDataAdapterFactory.CreateDataAdapter(
                        name,
                        entitiesOrdered,
                        connection,
                        transaction
                    );
                    command = ((IDbDataAdapter)adapter).SelectCommand;
                    command.CommandTimeout = settings.DataServiceExecuteProcedureTimeout;
                    BuildParameters(query.Parameters, command.Parameters, profile);
                    try
                    {
                        adapter.Fill(result);
                    }
                    catch (ConstraintException)
                    {
                        // make the exception far more verbose
                        throw new ConstraintException(DatasetTools.GetDatasetErrors(result));
                    }
                }
            }
            finally
            {
                var disposableCommand = command as IDisposable;
                disposableCommand?.Dispose();
                var disposableAdapter = adapter as IDisposable;
                disposableAdapter?.Dispose();
            }
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError("Stored Procedure Call failed", ex);
            }
            throw;
        }
        finally
        {
            if (transactionId == null)
            {
                try
                {
                    connection.Close();
                }
                finally
                {
                    connection.Dispose();
                }
            }
        }
        return result;
    }

    protected virtual IDbCommand ExecuteNonQuery(
        string name,
        QueryParameterCollection parameters,
        IDbConnection connection,
        IDbTransaction transaction,
        int timeOut
    )
    {
        IDbCommand command = DbDataAdapterFactory.GetCommand(name, connection);
        command.Transaction = transaction;
        command.CommandTimeout = timeOut;
        foreach (QueryParameter parameter in parameters)
        {
            if (parameter.Value != null)
            {
                IDbDataParameter dataParameter = DbDataAdapterFactory.GetParameter(
                    parameter.Name,
                    parameter.Value.GetType()
                );
                dataParameter.Value = parameter.Value;
                command.Parameters.Add(dataParameter);
            }
        }
        command.CommandType = CommandType.StoredProcedure;
        command.Prepare();
        command.ExecuteNonQuery();
        return command;
    }

    public override int UpdateField(
        Guid entityId,
        Guid fieldId,
        object oldValue,
        object newValue,
        IPrincipal userProfile,
        string transactionId
    )
    {
        var result = 0;
        var profile =
            SecurityManager.GetProfileProvider().GetProfile(userProfile.Identity) as UserProfile;
        IDbTransaction transaction = GetTransaction(transactionId, IsolationLevel.ReadCommitted);
        IDbConnection connection = transaction.Connection;
        try
        {
            TableMappingItem table = GetTable(entityId);
            if (table.DatabaseObjectType != DatabaseMappingObjectType.Table)
            {
                throw new ArgumentOutOfRangeException(
                    "DatabaseObjectType",
                    table.DatabaseObjectType,
                    ResourceUtils.GetString("UpdateFieldUpdate")
                );
            }
            FieldMappingItem field = GetTableColumn(fieldId);
            // get data for audit log
            var datasetGenerator = new DatasetGenerator(true);
            DataSet dataSet = datasetGenerator.CreateUpdateFieldDataSet(table, field);
            DataTable dataTable = dataSet.Tables[table.Name];
            DbDataAdapter adapter = DbDataAdapterFactory.CreateUpdateFieldDataAdapter(table, field);
            ((IDbDataAdapter)adapter).SelectCommand.Connection = connection;
            ((IDbDataAdapter)adapter).SelectCommand.Transaction = transaction;
            ((IDbDataAdapter)adapter).SelectCommand.CommandTimeout = 0;
            var logParameters = new QueryParameterCollection
            {
                new QueryParameter(field.Name, oldValue),
            };
            BuildParameters(
                logParameters,
                ((IDbDataAdapter)adapter).SelectCommand.Parameters,
                profile
            );
            dataTable.BeginLoadData();
            TraceCommand(((IDbDataAdapter)adapter).SelectCommand, transactionId);
            adapter.Fill(dataSet);
            dataTable.EndLoadData();
            if (dataTable.Rows.Count <= 0)
            {
                return result;
            }
            foreach (DataRow row in dataTable.Rows)
            {
                row[field.Name] = newValue;
            }
            LogData(dataTable, profile, transactionId, connection, transaction, 32);
            using (IDbCommand command = DbDataAdapterFactory.UpdateFieldCommand(table, field))
            {
                command.Connection = connection;
                command.Transaction = transaction;
                var parameters = new QueryParameterCollection
                {
                    new QueryParameter("oldValue", oldValue),
                    new QueryParameter("newValue", newValue),
                };
                BuildParameters(parameters, command.Parameters, profile);
                command.CommandTimeout = 0;
                result = command.ExecuteNonQuery();
            }
            return result;
        }
        catch
        {
            if (transactionId == null)
            {
                transaction.Rollback();
            }
            throw;
        }
        finally
        {
            if (transactionId == null)
            {
                connection.Close();
                connection.Dispose();
                transaction.Dispose();
            }
        }
    }

    public override int ReferenceCount(
        Guid entityId,
        Guid fieldId,
        object value,
        IPrincipal userProfile,
        string transactionId
    )
    {
        var profile =
            SecurityManager.GetProfileProvider().GetProfile(userProfile.Identity) as UserProfile;
        IDbTransaction transaction = GetTransaction(transactionId, IsolationLevel.ReadCommitted);
        IDbConnection connection = transaction.Connection;
        try
        {
            TableMappingItem table = GetTable(entityId);
            if (table.DatabaseObjectType != DatabaseMappingObjectType.Table)
            {
                throw new ArgumentOutOfRangeException(
                    "DatabaseObjectType",
                    table.DatabaseObjectType,
                    ResourceUtils.GetString("UpdateFieldUpdate")
                );
            }
            FieldMappingItem field = GetTableColumn(fieldId);
            IDbCommand command = DbDataAdapterFactory.SelectReferenceCountCommand(table, field);
            command.Connection = connection;
            command.Transaction = transaction;
            command.CommandTimeout = 0;
            var parameters = new QueryParameterCollection { new QueryParameter(field.Name, value) };
            BuildParameters(parameters, command.Parameters, profile);
            TraceCommand(command, transactionId);
            var result = (int)command.ExecuteScalar();
            return result;
        }
        finally
        {
            if (transactionId == null)
            {
                connection.Close();
                connection.Dispose();
                transaction.Dispose();
            }
        }
    }

    /// <summary>
    /// Executes an arbitrary SQL command on the database.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="transactionId">Existing transaction id. IMPORTANT:
    /// if transactionId is NULL the command runs without
    /// a transaction!</param>
    /// <returns></returns>
    public override string ExecuteUpdate(string command, string transactionId)
    {
        IDbTransaction transaction = null;
        IDbConnection connection;
        if (transactionId == null)
        {
            connection = GetConnection(ConnectionString);
            connection.Open();
        }
        else
        {
            transaction = GetTransaction(transactionId, IsolationLevel.ReadCommitted);
            connection = transaction.Connection;
        }
        try
        {
            var dataset = new DataSet();
            var records = 0;
            using (
                IDbCommand databaseCommand = DbDataAdapterFactory.GetCommand(
                    command,
                    connection,
                    transaction
                )
            )
            {
                DbDataAdapter adapter = DbDataAdapterFactory.GetAdapter(databaseCommand);
                databaseCommand.CommandTimeout = 0;
                records = adapter.Fill(dataset);
            }
            var stringBuilder = FormatResults(dataset);
            var recordsText = (records == 1) ? "record" : "records";
            stringBuilder.Append(records);
            stringBuilder.Append(" ");
            stringBuilder.Append(recordsText);
            stringBuilder.AppendLine(" affected.");
            return stringBuilder.ToString();
        }
        finally
        {
            if (transactionId == null)
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }

    private static StringBuilder FormatResults(DataSet dataset)
    {
        var stringBuilder = new StringBuilder();
        foreach (DataTable table in dataset.Tables)
        {
            foreach (DataColumn column in table.Columns)
            {
                stringBuilder.Append(column.ColumnName.PadRight(GetLength(column), ' ') + " ");
            }
            stringBuilder.AppendLine();
            foreach (DataColumn column in table.Columns)
            {
                stringBuilder.Append(new string('-', GetLength(column)) + " ");
            }
            stringBuilder.AppendLine();
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    string value;
                    if (row.IsNull(col))
                    {
                        value = "NULL";
                    }
                    else
                    {
                        value = row[col].ToString();
                        if (value.Length > DATA_VISUALIZATION_MAX_LENGTH)
                        {
                            value = value.Substring(0, DATA_VISUALIZATION_MAX_LENGTH);
                        }
                    }
                    stringBuilder.Append(value.PadRight(GetLength(col)) + " ");
                }
                stringBuilder.AppendLine();
            }
            stringBuilder.AppendLine();
        }
        return stringBuilder;
    }

    private static int GetLength(DataColumn column)
    {
        int nameLength = column.ColumnName.Length + 1;
        int dataLength;
        if (column.DataType == typeof(string))
        {
            dataLength = column.MaxLength;
            if (dataLength > DATA_VISUALIZATION_MAX_LENGTH)
            {
                dataLength = DATA_VISUALIZATION_MAX_LENGTH;
            }
        }
        else if (column.DataType == typeof(Guid))
        {
            dataLength = 36;
        }
        else if (column.DataType == typeof(DateTime))
        {
            dataLength = DateTime.MaxValue.ToString().Length;
        }
        else
        {
            dataLength = 10;
        }
        return nameLength > dataLength ? nameLength : dataLength;
    }

    private void LogData(
        DataTable changedTable,
        UserProfile profile,
        string transactionId,
        IDbConnection connection,
        IDbTransaction transaction
    )
    {
        LogData(changedTable, profile, transactionId, connection, transaction, -1);
    }

    private void LogData(
        DataTable changedTable,
        UserProfile profile,
        string transactionId,
        IDbConnection connection,
        IDbTransaction transaction,
        int overrideActionType
    )
    {
        DataAuditLog dataAuditLog = GetLog(
            changedTable,
            profile,
            transactionId,
            overrideActionType
        );
        if ((dataAuditLog == null) || (dataAuditLog.AuditRecord.Count <= 0))
        {
            return;
        }
        DataStructure logDataStructure = GetDataStructure(
            new Guid("530eba45-40db-470d-8e53-8b98ace758ad")
        );
        var adapterParameters = new SelectParameters
        {
            DataStructure = logDataStructure,
            Entity = (DataStructureEntity)logDataStructure.Entities[0],
            Parameters = new Hashtable(),
            Paging = false,
        };
        DbDataAdapter logAdapter = GetAdapter(adapterParameters, profile);
        SetConnection(logAdapter, connection);
        SetTransaction(logAdapter, transaction);
        logAdapter.Update(dataAuditLog);
        SetTransaction(logAdapter, null);
        SetConnection(logAdapter, null);
    }

    public override string DatabaseSchemaVersion()
    {
        try
        {
            var settings = ConfigurationManager.GetActiveConfiguration();
            string result;
            using (IDbConnection connection = GetConnection(connectionString))
            {
                connection.Open();
                result = RunSchemaVersionQuery(connection, settings);
            }
            return result;
        }
        catch (Exception ex)
        {
            HandleException(ex, ex.Message, null);
            throw;
        }
    }

    private string RunSchemaVersionQuery(IDbConnection connection, OrigamSettings settings)
    {
        try
        {
            return TryGetSchemaVersion(
                connection: connection,
                settings: settings,
                versionCommandName: "OrigamDatabaseSchemaVersion"
            );
        }
        catch
        {
            // pre 5 version would have the command named like this
            return TryGetSchemaVersion(
                connection: connection,
                settings: settings,
                versionCommandName: "AsapDatabaseSchemaVersion"
            );
        }
    }

    private string TryGetSchemaVersion(
        IDbConnection connection,
        OrigamSettings settings,
        string versionCommandName
    )
    {
        using (IDbCommand command = DbDataAdapterFactory.GetCommand(versionCommandName, connection))
        {
            command.CommandTimeout = settings.DataServiceExecuteProcedureTimeout;
            command.CommandType = CommandType.StoredProcedure;
            return (string)command.ExecuteScalar();
        }
    }

    public override string EntityDdl(Guid entityId)
    {
        if (
            !(
                PersistenceProvider.RetrieveInstance(
                    typeof(TableMappingItem),
                    new ModelElementKey(entityId)
                )
                is TableMappingItem table
            )
        )
        {
            throw new ArgumentOutOfRangeException(
                "entityId",
                entityId,
                "Element is not a table mapping."
            );
        }
        return DbDataAdapterFactory.TableDefinitionDdl(table);
    }

    public override string[] FieldDdl(Guid fieldId)
    {
        var result = new string[2];
        if (
            !(
                PersistenceProvider.RetrieveInstance(
                    typeof(FieldMappingItem),
                    new ModelElementKey(fieldId)
                )
                is FieldMappingItem column
            )
        )
        {
            throw new ArgumentOutOfRangeException(
                "fieldId",
                fieldId,
                "Element is not a column mapping."
            );
        }
        result[0] = DbDataAdapterFactory.AddColumnDdl(column);
        result[1] = DbDataAdapterFactory.AddForeignKeyConstraintDdl(
            column.ParentItem as TableMappingItem,
            column.ForeignKeyConstraint
        );
        return result;
    }

    public override IDataReader ExecuteDataReader(
        DataStructureQuery query,
        IPrincipal principal,
        string transactionId
    )
    {
        DataSet dataSet = null;
        var settings = ConfigurationManager.GetActiveConfiguration();
        int timeout = settings.DataServiceSelectTimeout;
        UserProfile currentProfile = null;
        if (query.LoadByIdentity)
        {
            currentProfile =
                SecurityManager.GetProfileProvider().GetProfile(principal.Identity) as UserProfile;
        }
        if (PersistenceProvider == null)
        {
            throw new NullReferenceException(ResourceUtils.GetString("NoProviderForMS"));
        }
        DataStructure dataStructure = GetDataStructure(query);
        DataStructureFilterSet filterSet = GetFilterSet(query.MethodId);
        DataStructureSortSet sortSet = GetSortSet(query.SortSetId);
        DataStructureEntity entity = GetEntity(query, dataStructure);
        dataSet = GetDataset(dataStructure, query.DefaultSetId);
        DatasetTools.RemoveExpressions(dataSet, true);
        IDbConnection connection = null;
        IDbTransaction transaction = null;
        var commandBehavior = CommandBehavior.Default;
        if (transactionId != null)
        {
            transaction = GetTransaction(transactionId, query.IsolationLevel);
        }
        if (transaction == null)
        {
            connection = GetConnection(connectionString);
            commandBehavior = CommandBehavior.CloseConnection;
        }
        else
        {
            connection = transaction.Connection;
        }
        var adapterParameters = new SelectParameters
        {
            DataStructure = dataStructure,
            Entity = entity,
            Filter = filterSet,
            SortSet = sortSet,
            Parameters = query.Parameters.ToHashtable(),
            Paging = query.Paging,
            ColumnsInfo = query.ColumnsInfo,
            CustomFilters = query.CustomFilters,
            CustomOrderings = query.CustomOrderings,
            CustomGrouping = query.CustomGrouping,
            Distinct = query.Distinct,
            RowLimit = query.RowLimit,
            RowOffset = query.RowOffset,
            ForceDatabaseCalculation = query.ForceDatabaseCalculation,
            AggregatedColumns = query.AggregatedColumns,
        };
        DbDataAdapter adapter = GetAdapter(adapterParameters, currentProfile);
        ((IDbDataAdapter)adapter).SelectCommand.Connection = connection;
        ((IDbDataAdapter)adapter).SelectCommand.Transaction = transaction;
        ((IDbDataAdapter)adapter).SelectCommand.CommandTimeout = timeout;
        BuildParameters(query.Parameters, adapter.SelectCommand.Parameters, currentProfile);
        if (connection.State == ConnectionState.Closed)
        {
            connection.Open();
        }
        var profiledCommand = new ProfiledDbCommand(
            adapter.SelectCommand,
            adapter.SelectCommand.Connection,
            MiniProfiler.Current
        );
        DbDataReader reader = profiledCommand.ExecuteReader(commandBehavior);
        profiledCommand.Dispose();
        return reader;
    }

    public override IEnumerable<IEnumerable<object>> ExecuteDataReader(DataStructureQuery query)
    {
        return ExecuteDataReaderInternal(query).Select(line => line.Select(pair => pair.Value));
    }

    public override IEnumerable<Dictionary<string, object>> ExecuteDataReaderReturnPairs(
        DataStructureQuery query
    )
    {
        return ExecuteDataReaderInternal(query)
            .Select(line => ExpandAggregationData(line, query))
            .Select(line =>
                line.Where(pair => pair.Key != null)
                    .ToDictionary(pair => pair.Key, pair => pair.Value)
            );
    }

    private List<KeyValuePair<string, object>> ExpandAggregationData(
        IEnumerable<KeyValuePair<string, object>> line,
        DataStructureQuery query
    )
    {
        var processedItems = new List<KeyValuePair<string, object>>();
        var aggregationData = new List<object>();
        foreach (KeyValuePair<string, object> pair in line)
        {
            Aggregation aggregatedColumn = query.AggregatedColumns?.FirstOrDefault(column =>
                column.SqlQueryColumnName == pair.Key
            );
            if (aggregatedColumn != null)
            {
                aggregationData.Add(
                    new
                    {
                        Column = aggregatedColumn.ColumnName,
                        Type = aggregatedColumn.AggregationType.ToString(),
                        Value = pair.Value,
                    }
                );
            }
            else
            {
                processedItems.Add(pair);
            }
        }
        processedItems.Add(new KeyValuePair<string, object>("aggregations", aggregationData));
        return processedItems;
    }

    private IEnumerable<T> ToEnumerable<T>(T item)
    {
        yield return item;
    }

    private List<ColumnData> GetAllQueryColumns(DataStructureQuery query)
    {
        return query
            .GetAllQueryColumns()
            .SelectMany(column =>
            {
                if (
                    !string.IsNullOrWhiteSpace(query.CustomGrouping?.GroupingUnit)
                    && (query.CustomGrouping?.GroupBy == column.Name)
                )
                {
                    return TimeGroupingRenderer
                        .GetColumnNames(column.Name, query.CustomGrouping.GroupingUnit)
                        .Select(columnName => new ColumnData(
                            columnName,
                            column.IsVirtual,
                            column.DefaultValue,
                            column.HasRelation
                        ));
                }
                return ToEnumerable(column);
            })
            .ToList();
    }

    private IEnumerable<IEnumerable<KeyValuePair<string, object>>> ExecuteDataReaderInternal(
        DataStructureQuery query
    )
    {
        using (
            IDataReader reader = ExecuteDataReader(query, SecurityManager.CurrentPrincipal, null)
        )
        {
            List<ColumnData> queryColumns = GetAllQueryColumns(query);
            while (reader.Read())
            {
                var values = new KeyValuePair<string, object>[queryColumns.Count];
                for (var i = 0; i < queryColumns.Count; i++)
                {
                    ColumnData queryColumn = queryColumns[i];
                    if (queryColumn.IsVirtual && !queryColumn.HasRelation)
                    {
                        continue;
                    }
                    object value = reader.GetValue(reader.GetOrdinal(queryColumn.Name));
                    values[i] = new KeyValuePair<string, object>(queryColumn.Name, value);
                }
                yield return ProcessReaderOutput(values, queryColumns);
            }
        }
    }

    private static List<KeyValuePair<string, object>> ProcessReaderOutput(
        KeyValuePair<string, object>[] values,
        List<ColumnData> columnData
    )
    {
        if (columnData == null)
        {
            throw new ArgumentNullException(nameof(columnData));
        }

        var updatedValues = new List<KeyValuePair<string, object>>();
        for (int i = 0; i < columnData.Count; i++)
        {
            if (columnData[i].IsVirtual)
            {
                if (
                    columnData[i].HasRelation
                    && values[i].Value != null
                    && values[i].Value != DBNull.Value
                )
                {
                    updatedValues.Add(
                        new KeyValuePair<string, object>(
                            values[i].Key,
                            ((string)values[i].Value).Split((char)1)
                        )
                    );
                    continue;
                }
                updatedValues.Add(
                    new KeyValuePair<string, object>(values[i].Key, columnData[i].DefaultValue)
                );

                continue;
            }
            updatedValues.Add(new KeyValuePair<string, object>(values[i].Key, values[i].Value));
        }
        return updatedValues;
    }

    private static DataStructureEntity GetEntity(
        DataStructureQuery query,
        DataStructure dataStructure
    )
    {
        DataStructureEntity entity;
        switch (query.DataSourceType)
        {
            case QueryDataSourceType.DataStructure:
            {
                List<DataStructureEntity> entities;
                if (string.IsNullOrEmpty(query.Entity))
                {
                    entities = dataStructure.Entities;
                }
                else
                {
                    entities = new List<DataStructureEntity>();
                    foreach (DataStructureEntity dataStructureEntity in dataStructure.Entities)
                    {
                        if (dataStructureEntity.Name == query.Entity)
                        {
                            entities.Add(dataStructureEntity);
                            break;
                        }
                    }
                    if (entities.Count == 0)
                    {
                        throw new ArgumentOutOfRangeException(
                            $"Entity {query.Entity} not found in data structure {dataStructure.Path}."
                        );
                    }
                }
                entity = entities[0] as DataStructureEntity;
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(
                    "DataSourceType",
                    query.DataSourceType,
                    ResourceUtils.GetString("UnknownDataSource")
                );
            }
        }
        return entity;
    }

    public virtual void CreateSchema(string databaseName) { }

    public void CreateFirstNewWebUser(QueryParameterCollection parameters)
    {
        var transactionId = Guid.NewGuid().ToString();
        try
        {
            ExecuteUpdate(CreateBusinessPartnerInsert(parameters), transactionId);
            ExecuteUpdate(CreateOrigamUserInsert(parameters), transactionId);
            ExecuteUpdate(CreateBusinessPartnerRoleIdInsert(parameters), transactionId);
            ExecuteUpdate(AlreadyCreatedUser(parameters), transactionId);
            ResourceMonitor.Commit(transactionId);
        }
        catch (Exception)
        {
            ResourceMonitor.Rollback(transactionId);
            throw;
        }
    }

    public abstract string CreateBusinessPartnerInsert(QueryParameterCollection parameters);
    public abstract string CreateOrigamUserInsert(QueryParameterCollection parameters);
    public abstract string CreateBusinessPartnerRoleIdInsert(QueryParameterCollection parameters);
    public abstract string AlreadyCreatedUser(QueryParameterCollection parameters);
    #endregion
    #region Is Schema Item in Database
    public override bool IsSchemaItemInDatabase(ISchemaItem schemaItem)
    {
        return (schemaItem is DataEntityIndex dataEntityIndex)
            && IsDataEntityIndexInDatabase(dataEntityIndex);
    }

    internal abstract bool IsDataEntityIndexInDatabase(DataEntityIndex dataEntityIndex);
    #endregion
    #region Compare Schema

    internal abstract string GetAllTablesSql();

    public override List<SchemaDbCompareResult> CompareSchema(IPersistenceProvider provider)
    {
        var results = new List<SchemaDbCompareResult>();
        List<TableMappingItem> schemaTables = GetSchemaTables(provider);
        // tables
        Hashtable schemaTableList = GetSchemaTableList(schemaTables);
        var schemaColumnList = new Hashtable();
        var schemaIndexListAll = new Hashtable();
        Hashtable dbTableList = GetDbTableList();
        DataSet columns = GetData(GetAllColumnsSQL());
        columns.CaseSensitive = true;
        DoCompare(
            results,
            dbTableList,
            schemaTableList,
            columns,
            DbCompareResultType.MissingInDatabase,
            typeof(TableMappingItem),
            provider
        );
        DoCompare(
            results,
            dbTableList,
            schemaTableList,
            columns,
            DbCompareResultType.MissingInSchema,
            typeof(TableMappingItem),
            provider
        );
        // fields
        // model exists in database
        DoCompareModelInDatabase(results, schemaTables, dbTableList, schemaColumnList, columns);
        DoCompareDatabaseInModel(results, schemaTableList, schemaColumnList, columns);
        //End fields
        //indexes
        DataSet indexes = GetData(GetSqlIndexes());
        DataSet indexFields = GetData(GetSqlIndexFields());
        indexFields.CaseSensitive = true;
        Hashtable schemaIndexListGenerate = GetSchemaIndexListGenerate(
            schemaTables,
            dbTableList,
            schemaIndexListAll
        );
        Hashtable dbIndexList = GetDbIndexList(indexes, schemaTableList);
        DoCompare(
            results,
            dbIndexList,
            schemaIndexListGenerate,
            columns,
            DbCompareResultType.MissingInDatabase,
            typeof(DataEntityIndex),
            provider
        );
        DoCompare(
            results,
            dbIndexList,
            schemaIndexListAll,
            columns,
            DbCompareResultType.MissingInSchema,
            typeof(DataEntityIndex),
            provider
        );
        // For index fields we only compare if the whole index is equal,
        // we do not return result for each different field.
        // In case of a different index, we have to re-create
        // the whole index anyway.
        DoCompareIndex(results, schemaTables, indexFields);
        // foreign keys
        DataSet foreignKeys = GetData(GetSqlFk());
        // for each existing table - we skip foreign keys
        // where table does not exist in the database or schema,
        // they will be re-created completely anyway
        DoCompareIndexExistingTables(results, schemaTables, foreignKeys, columns);
        return results;
    }

    private void DoCompareIndexExistingTables(
        List<SchemaDbCompareResult> results,
        List<TableMappingItem> schemaTables,
        DataSet foreignKeys,
        DataSet columns
    )
    {
        foreach (TableMappingItem table in schemaTables)
        {
            // not for views and not for tables where generating script
            // is turned off
            if (
                !table.GenerateDeploymentScript
                || (table.DatabaseObjectType != DatabaseMappingObjectType.Table)
            )
            {
                continue;
            }
            DataRow[] dbRows = foreignKeys
                .Tables[0]
                .Select("FK_Table = '" + table.MappedObjectName + "'");
            // there are some constraints for this table in the database
            if (dbRows.Length > 0)
            {
                // we try to see which of these we don't find in the model
                foreach (DataRow row in dbRows)
                {
                    bool found = table
                        .Constraints.Where(constraint =>
                            (constraint.Type == ConstraintType.ForeignKey)
                            && (constraint.ForeignEntity is TableMappingItem)
                        )
                        .Any(constraint =>
                            (
                                (string)row["PK_Table"]
                                == ((TableMappingItem)constraint.ForeignEntity).MappedObjectName
                            ) && ((string)row["FK_Table"] == table.MappedObjectName)
                        );
                    if (found)
                    {
                        continue;
                    }
                    // constraint found in database but not in the model
                    var result = new SchemaDbCompareResult
                    {
                        ResultType = DbCompareResultType.MissingInSchema,
                        ItemName = (string)row["Constraint"],
                        // TODO: result.SchemaItem = ?
                        ParentSchemaItem = table,
                        SchemaItemType = typeof(DataEntityConstraint),
                    };
                    results.Add(result);
                }
            }
            // we compare what is missing in the database
            foreach (DataEntityConstraint constraint in table.Constraints)
            {
                CompareConstraintMissingInDatabase(
                    constraint,
                    table,
                    foreignKeys,
                    columns,
                    results
                );
            }
        }
    }

    private void CompareConstraintMissingInDatabase(
        DataEntityConstraint constraint,
        TableMappingItem table,
        DataSet foreignKeys,
        DataSet columns,
        List<SchemaDbCompareResult> results
    )
    {
        if (
            (constraint.Type != ConstraintType.ForeignKey)
            || !(constraint.ForeignEntity is TableMappingItem)
            || !(constraint.Fields[0] is FieldMappingItem)
        )
        {
            return;
        }
        DataRow[] rows = foreignKeys
            .Tables[0]
            .Select(
                "PK_Table = '"
                    + (constraint.ForeignEntity as TableMappingItem).MappedObjectName
                    + "' AND FK_Table = '"
                    + table.MappedObjectName
                    + "' AND cKeyCol1 = '"
                    + (constraint.Fields[0] as FieldMappingItem).MappedColumnName
                    + "'"
            );
        if (
            columns
                .Tables[0]
                .Select(
                    "TABLE_NAME = '"
                        + table.MappedObjectName
                        + "' AND COLUMN_NAME = '"
                        + (constraint.Fields[0] as FieldMappingItem).MappedColumnName
                        + "'"
                )
                .Length <= 0
        )
        {
            return;
        }
        if (rows.Length == 0)
        {
            // constraint was not found in the database at all
            var result = new SchemaDbCompareResult
            {
                ResultType = DbCompareResultType.MissingInDatabase,
                ItemName = ConstraintName(table, constraint),
                SchemaItem = table,
                SchemaItemType = typeof(DataEntityConstraint),
                Script = (
                    (AbstractSqlCommandGenerator)DbDataAdapterFactory
                ).AddForeignKeyConstraintDdl(table, constraint),
            };
            results.Add(result);
        }
        else
        {
            var constraintEqual = true;
            // constraint found in database,
            // we check if it has the same fields
            foreach (IDataEntityColumn column in constraint.Fields)
            {
                if (!(column is FieldMappingItem fieldMappingItem))
                {
                    // one of the columns is not a physical column
                    constraintEqual = false;
                    break;
                }
                if (!(column.ForeignKeyField is FieldMappingItem foreignKeyField))
                {
                    continue;
                }
                string primaryKey = foreignKeyField.MappedColumnName;
                string foreignKey = fieldMappingItem.MappedColumnName;
                var foundPair = false;
                for (var i = 1; i < 17; i++)
                {
                    if (
                        (rows[0]["cKeyCol" + i] != DBNull.Value)
                        && ((string)rows[0]["cKeyCol" + i] == foreignKey)
                        && ((string)rows[0]["cRefCol" + i] == primaryKey)
                    )
                    {
                        foundPair = true;
                        break;
                    }
                }
                // if pair was found,
                // we set constraint to equal
                if (!foundPair)
                {
                    constraintEqual = false;
                    break;
                }
            }
            if (!constraintEqual)
            {
                // constraint found but different
                var result = new SchemaDbCompareResult
                {
                    ResultType = DbCompareResultType.ExistingButDifferent,
                    ItemName =
                        "FK_"
                        + table.MappedObjectName
                        + "_"
                        + (constraint.Fields[0] as FieldMappingItem).MappedColumnName
                        + "_"
                        + (constraint.ForeignEntity as TableMappingItem).MappedObjectName,
                    SchemaItem = table,
                    SchemaItemType = typeof(DataEntityConstraint),
                };
                results.Add(result);
            }
        }
    }

    internal abstract string GetSqlFk();

    protected abstract string GetIndexSelectQuery(
        DataEntityIndexField indexField,
        string mappedObjectName,
        string indexName
    );

    private void DoCompareIndex(
        List<SchemaDbCompareResult> results,
        List<TableMappingItem> schemaTables,
        DataSet indexFields
    )
    {
        foreach (TableMappingItem table in schemaTables)
        {
            if (
                !table.GenerateDeploymentScript
                || (table.DatabaseObjectType != DatabaseMappingObjectType.Table)
            )
            {
                continue;
            }
            foreach (DataEntityIndex index in table.EntityIndexes)
            {
                if (index.GenerateDeploymentScript == false)
                {
                    continue;
                }
                var different = false;
                DataRow[] rows = indexFields
                    .Tables[0]
                    .Select(
                        "TableName = '"
                            + table.MappedObjectName
                            + "' AND IndexName = '"
                            + index.Name
                            + "'"
                    );
                // for indexes that exist in both schema and database
                // we compare if they are equal
                if (rows.Length <= 0)
                {
                    continue;
                }
                // if there is a different number of fields,
                // we consider them non-equal without even checking
                // the details
                if (
                    rows.Length
                    != index
                        .ChildItemsByType<DataEntityIndexField>(DataEntityIndexField.CategoryConst)
                        .Count
                )
                {
                    different = true;
                }
                if (!different)
                {
                    foreach (
                        DataEntityIndexField indexField in index.ChildItemsByType<DataEntityIndexField>(
                            DataEntityIndexField.CategoryConst
                        )
                    )
                    {
                        rows = indexFields
                            .Tables[0]
                            .Select(
                                GetIndexSelectQuery(
                                    indexField: indexField,
                                    mappedObjectName: table.MappedObjectName,
                                    indexName: index.Name
                                )
                            );
                        if (rows.Length == 0)
                        {
                            different = true;
                            break;
                        }
                    }
                }
                if (different)
                {
                    // exists in both schema and database,
                    // but they are different
                    var result = new SchemaDbCompareResult
                    {
                        ResultType = DbCompareResultType.ExistingButDifferent,
                        ItemName = table.MappedObjectName + "." + index.Name,
                        SchemaItem = index,
                        SchemaItemType = typeof(DataEntityIndex),
                    };
                    result.Script = (
                        (AbstractSqlCommandGenerator)DbDataAdapterFactory
                    ).IndexDefinitionDdl(table, result.SchemaItem as DataEntityIndex, true);
                    results.Add(result);
                }
            }
        }
    }

    internal abstract Hashtable GetDbIndexList(DataSet indexes, Hashtable schemaTableList);
    internal abstract Hashtable GetSchemaIndexListGenerate(
        List<TableMappingItem> schemaTables,
        Hashtable dbTableList,
        Hashtable schemaIndexListAll
    );

    internal abstract string GetSqlIndexFields();
    internal abstract string GetSqlIndexes();

    private void DoCompareDatabaseInModel(
        List<SchemaDbCompareResult> results,
        Hashtable schemaTableList,
        Hashtable schemaColumnList,
        DataSet columns
    )
    {
        var abstractSqlCommandGenerator = (AbstractSqlCommandGenerator)DbDataAdapterFactory;
        foreach (DataRow row in columns.Tables[0].Rows)
        {
            var key = row["TABLE_NAME"] + "." + row["COLUMN_NAME"];
            // only if the table exists in the model,
            // otherwise we will be creating the whole table later on
            // so it makes no sense to list all the columns with it
            if (schemaTableList[row["TABLE_NAME"]] is not TableMappingItem table)
            {
                continue;
            }
            if (table.DatabaseObjectType == DatabaseMappingObjectType.View)
            {
                continue;
            }
            if (schemaColumnList.ContainsKey(key))
            {
                var fieldMappingItem = schemaColumnList[key] as FieldMappingItem;
                var differenceDescription = "";
                if (((string)row["IS_NULLABLE"] == "YES") && !fieldMappingItem.AllowNulls)
                {
                    differenceDescription =
                        (differenceDescription == "" ? "" : "; ")
                        + "AllowNulls: Schema-NO, Database-YES";
                }
                if (((string)row["IS_NULLABLE"] == "NO") && fieldMappingItem.AllowNulls)
                {
                    differenceDescription =
                        (differenceDescription == "" ? "" : "; ")
                        + "AllowNulls: Schema-YES, Database-NO";
                }
                if (
                    CompareType(
                        row,
                        abstractSqlCommandGenerator
                            .DdlDataType(fieldMappingItem.DataType, fieldMappingItem.MappedDataType)
                            .ToUpper()
                    )
                )
                {
                    differenceDescription =
                        (differenceDescription == "" ? "" : "; ")
                        + "DataType: Schema-"
                        + abstractSqlCommandGenerator.DdlDataType(
                            fieldMappingItem.DataType,
                            fieldMappingItem.MappedDataType
                        )
                        + ", Database-"
                        + (string)row["DATA_TYPE"];
                }
                if (
                    (fieldMappingItem.DataType == OrigamDataType.String)
                    && !row.IsNull("CHARACTER_MAXIMUM_LENGTH")
                    && ((int)row["CHARACTER_MAXIMUM_LENGTH"] != fieldMappingItem.DataLength)
                )
                {
                    differenceDescription =
                        (differenceDescription == "" ? "" : "; ")
                        + "DataLength: Schema-"
                        + fieldMappingItem.DataLength
                        + ", Database-"
                        + row["CHARACTER_MAXIMUM_LENGTH"];
                }
                if (differenceDescription != "")
                {
                    // exists in both schema and database, but they are different
                    var result = new SchemaDbCompareResult
                    {
                        ResultType = DbCompareResultType.ExistingButDifferent,
                        ItemName = key,
                        SchemaItem = fieldMappingItem,
                        Remark = differenceDescription,
                        SchemaItemType = typeof(FieldMappingItem),
                        Script = abstractSqlCommandGenerator.AlterColumnDdl(fieldMappingItem),
                    };
                    results.Add(result);
                }
            }
            else
            {
                // does not exist in schema
                var result = new SchemaDbCompareResult
                {
                    ResultType = DbCompareResultType.MissingInSchema,
                    ItemName = key,
                    SchemaItem = null,
                    ParentSchemaItem = schemaTableList[row["TABLE_NAME"]],
                    SchemaItemType = typeof(FieldMappingItem),
                };
                results.Add(result);
            }
        }
    }

    private bool CompareType(DataRow row, string modelType)
    {
        string columnType = GetColumnType(row);
        if (columnType.Contains("TIMESTAMP") && modelType.Contains("TIMESTAMP"))
        {
            return false;
        }
        if (columnType.Contains("CHARACTER VARYING") && modelType.Contains("VARCHAR"))
        {
            return false;
        }
        if (
            (columnType == "GEOGRAPHY(MAX)" && modelType == "GEOGRAPHY")
            || (columnType == "NVARCHAR(MAX)" && modelType == "NVARCHAR")
        )
        {
            return false;
        }
        return columnType != modelType;
    }

    private string GetColumnType(DataRow row)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append((string)row["DATA_TYPE"]);
        int length = Convert.IsDBNull(row["CHARACTER_MAXIMUM_LENGTH"])
            ? 0
            : (int)row["CHARACTER_MAXIMUM_LENGTH"];
        if (length == -1)
        {
            stringBuilder.Append("(MAX)");
        }
        return stringBuilder.ToString().ToUpper();
    }

    private void DoCompareModelInDatabase(
        List<SchemaDbCompareResult> results,
        List<TableMappingItem> schemaTables,
        Hashtable dbTableList,
        Hashtable schemaColumnList,
        DataSet columns
    )
    {
        foreach (TableMappingItem table in schemaTables)
        {
            // only if the table exists in the database,
            // otherwise we will be creating the whole table later on
            // so it makes no sense to list all the columns with it
            if (!dbTableList.ContainsKey(table.MappedObjectName))
            {
                continue;
            }
            foreach (IDataEntityColumn column in table.EntityColumns)
            {
                if (!(column is FieldMappingItem fieldMappingItem))
                {
                    continue;
                }
                var key = table.MappedObjectName + "." + fieldMappingItem.MappedColumnName;
                if (!schemaColumnList.ContainsKey(key))
                {
                    schemaColumnList.Add(key, column);
                }
                if (
                    columns
                        .Tables[0]
                        .Select(
                            "TABLE_NAME = '"
                                + table.MappedObjectName
                                + "' AND COLUMN_NAME = '"
                                + fieldMappingItem.MappedColumnName
                                + "'"
                        )
                        .Length != 0
                )
                {
                    continue;
                }
                // column does not exist in the database
                var result = new SchemaDbCompareResult
                {
                    ResultType = DbCompareResultType.MissingInDatabase,
                    ItemName = key,
                    SchemaItem = column,
                    SchemaItemType = typeof(FieldMappingItem),
                };
                if (table.DatabaseObjectType == DatabaseMappingObjectType.Table)
                {
                    result.Script = (
                        (AbstractSqlCommandGenerator)DbDataAdapterFactory
                    ).AddColumnDdl(fieldMappingItem);
                }
                else
                {
                    continue;
                }
                results.Add(result);
                // foreign key
                DataEntityConstraint foreignKeyConstraint = column.ForeignKeyConstraint;
                if (
                    (table.DatabaseObjectType == DatabaseMappingObjectType.Table)
                    && (foreignKeyConstraint != null)
                )
                {
                    result = new SchemaDbCompareResult
                    {
                        ResultType = DbCompareResultType.MissingInDatabase,
                        ItemName = ConstraintName(table, foreignKeyConstraint),
                        SchemaItem = column,
                        SchemaItemType = typeof(DataEntityConstraint),
                        Script = (
                            (AbstractSqlCommandGenerator)DbDataAdapterFactory
                        ).AddForeignKeyConstraintDdl(table, foreignKeyConstraint),
                    };
                    results.Add(result);
                }
            }
        }
    }

    internal abstract string GetAllColumnsSQL();

    private Hashtable GetDbTableList()
    {
        DataSet tables = GetData(GetAllTablesSql());
        var dbTableList = new Hashtable();
        foreach (DataRow row in tables.Tables[0].Rows)
        {
            dbTableList.Add(row["TABLE_NAME"], null);
        }
        return dbTableList;
    }

    private Hashtable GetSchemaTableList(List<TableMappingItem> schemaTables)
    {
        var schemaTableList = new Hashtable();
        foreach (TableMappingItem table in schemaTables)
        {
            if (!schemaTableList.Contains(table.MappedObjectName))
            {
                schemaTableList.Add(table.MappedObjectName, table);
            }
        }
        return schemaTableList;
    }

    private List<TableMappingItem> GetSchemaTables(IPersistenceProvider provider)
    {
        List<ISchemaItem> entityList = provider.RetrieveListByCategory<ISchemaItem>(
            AbstractDataEntity.CategoryConst
        );
        var schemaTables = new List<TableMappingItem>();
        foreach (var tableMappingItem in entityList.OfType<TableMappingItem>())
        {
            schemaTables.Add(tableMappingItem);
        }
        return schemaTables;
    }

    private static string ConstraintName(TableMappingItem table, DataEntityConstraint constraint)
    {
        return "FK_"
            + table.MappedObjectName
            + "_"
            + ((FieldMappingItem)constraint.Fields[0]).MappedColumnName
            + "_"
            + ((TableMappingItem)constraint.ForeignEntity).MappedObjectName;
    }

    private void DoCompare(
        List<SchemaDbCompareResult> results,
        Hashtable dbList,
        Hashtable schemaList,
        DataSet columns,
        DbCompareResultType direction,
        Type schemaItemType,
        IPersistenceProvider provider
    )
    {
        switch (direction)
        {
            case DbCompareResultType.MissingInDatabase:
            {
                CompareMissingInDatabase(results, dbList, schemaList, schemaItemType);
                break;
            }
            case DbCompareResultType.MissingInSchema:
            {
                CompareMissingInModel(
                    results,
                    dbList,
                    schemaList,
                    columns,
                    schemaItemType,
                    provider
                );
                break;
            }
        }
    }

    private void CompareMissingInModel(
        List<SchemaDbCompareResult> results,
        Hashtable dbList,
        Hashtable schemaList,
        DataSet columns,
        Type schemaItemType,
        IPersistenceProvider provider
    )
    {
        var sqlGenerator = DbDataAdapterFactory as AbstractSqlCommandGenerator;
        foreach (DictionaryEntry entry in dbList)
        {
            if (schemaList.ContainsKey(entry.Key))
            {
                continue;
            }
            var result = new SchemaDbCompareResult
            {
                ResultType = DbCompareResultType.MissingInSchema,
                ItemName = (string)entry.Key,
                ParentSchemaItem = entry.Value,
                SchemaItemType = schemaItemType,
            };
            // generate a model element
            if (schemaItemType == typeof(TableMappingItem))
            {
                var schemaService = ServiceManager.Services.GetService<ISchemaService>();
                var entity = new TableMappingItem();
                entity.PersistenceProvider = provider;
                entity.SchemaExtensionId = schemaService.ActiveSchemaExtensionId;
                entity.RootProvider = schemaService.GetProvider<EntityModelSchemaItemProvider>();
                entity.Name = result.ItemName;
                foreach (
                    DataRow columnRow in columns
                        .Tables[0]
                        .Select("TABLE_NAME = '" + entity.Name + "'")
                )
                {
                    var fieldMappingItem = entity.NewItem<FieldMappingItem>(
                        schemaService.ActiveSchemaExtensionId,
                        null
                    );
                    fieldMappingItem.Name = (string)columnRow["COLUMN_NAME"];
                    fieldMappingItem.AllowNulls = (string)columnRow["IS_NULLABLE"] == "YES";
                    // find a specific data type
                    var dataTypeSchemaItemProvider =
                        schemaService.GetProvider<DatabaseDataTypeSchemaItemProvider>();
                    var dataTypeName = (string)columnRow["DATA_TYPE"];
                    DatabaseDataType databaseDataType = dataTypeSchemaItemProvider.FindDataType(
                        dataTypeName
                    );
                    if (databaseDataType == null)
                    {
                        // if not found, get a generic data type
                        fieldMappingItem.DataType = sqlGenerator.ToOrigamDataType(dataTypeName);
                    }
                    else
                    {
                        fieldMappingItem.DataType = databaseDataType.DataType;
                        fieldMappingItem.MappedDataType = databaseDataType;
                    }
                    if (!columnRow.IsNull("CHARACTER_MAXIMUM_LENGTH"))
                    {
                        fieldMappingItem.DataLength = (int)columnRow["CHARACTER_MAXIMUM_LENGTH"];
                    }
                }
                result.SchemaItem = entity;
            }
            else
            {
                continue;
            }
            results.Add(result);
        }
    }

    private void CompareMissingInDatabase(
        List<SchemaDbCompareResult> results,
        Hashtable dbList,
        Hashtable schemaList,
        Type schemaItemType
    )
    {
        var sqlGenerator = DbDataAdapterFactory as AbstractSqlCommandGenerator;
        foreach (DictionaryEntry entry in schemaList)
        {
            var process = !(
                (entry.Value is TableMappingItem tableMappingItem)
                && !tableMappingItem.GenerateDeploymentScript
            );
            if (
                (entry.Value is DataEntityIndex dataEntityIndex)
                && (
                    !((TableMappingItem)dataEntityIndex.ParentItem).GenerateDeploymentScript
                    || !dataEntityIndex.GenerateDeploymentScript
                )
            )
            {
                process = false;
            }
            if (dbList.ContainsKey(entry.Key) || !process)
            {
                continue;
            }
            var result = new SchemaDbCompareResult
            {
                ResultType = DbCompareResultType.MissingInDatabase,
                ItemName = (string)entry.Key,
                SchemaItem = (ISchemaItem)entry.Value,
                ParentSchemaItem = ((ISchemaItem)entry.Value).ParentItem,
                SchemaItemType = schemaItemType,
            };
            if (schemaItemType == typeof(TableMappingItem))
            {
                if (
                    ((TableMappingItem)result.SchemaItem).DatabaseObjectType
                    == DatabaseMappingObjectType.Table
                )
                {
                    result.Script = sqlGenerator.TableDefinitionDdl(
                        result.SchemaItem as TableMappingItem
                    );
                    result.Script2 = sqlGenerator.ForeignKeyConstraintsDdl(
                        result.SchemaItem as TableMappingItem
                    );
                }
                else
                {
                    continue;
                }
            }
            if (schemaItemType == typeof(DataEntityIndex))
            {
                result.Script = sqlGenerator.IndexDefinitionDdl(
                    result.ParentSchemaItem as IDataEntity,
                    result.SchemaItem as DataEntityIndex,
                    true
                );
            }
            results.Add(result);
        }
    }

    internal DataSet GetData(string sql)
    {
        using (IDbConnection connection = GetConnection(connectionString))
        {
            connection.Open();
            try
            {
                DbDataAdapter adapter = DbDataAdapterFactory.GetAdapter(
                    DbDataAdapterFactory.GetCommand(sql, connection)
                );
                var schemaCompareDataset = new DataSet("SchemaCompare");
                adapter.Fill(schemaCompareDataset);
                return schemaCompareDataset;
            }
            finally
            {
                connection.Close();
            }
        }
    }
    #endregion

    #region Private Methods
    private void AcceptChanges(
        DataSet dataset,
        List<string> changedTables,
        DataStructureQuery query,
        string transactionId,
        IPrincipal userProfile
    )
    {
        // Retrieve actual values and accept changes.
        // Much faster than DataSet.AcceptChanges()
        foreach (string tableName in changedTables)
        {
            DataTable table = dataset.Tables[tableName];
            int rowCount = table.Rows.Count;
            var rowArray = new DataRow[rowCount];
            table.Rows.CopyTo(rowArray, 0);
            // create dataset of this particular data table
            // for loading new data
            DataSet newData = CloneDatasetForActualRow(table);
            for (var i = 0; i < rowCount; i++)
            {
                if (rowArray[i].RowState == DataRowState.Deleted)
                {
                    rowArray[i].AcceptChanges();
                }
                else if (
                    rowArray[i].RowState != DataRowState.Unchanged
                    && rowArray[i].RowState != DataRowState.Detached
                    && rowArray[i].RowState != DataRowState.Deleted
                )
                {
                    if (query.LoadActualValuesAfterUpdate)
                    {
                        var entityId = (Guid)table.ExtendedProperties["Id"];
                        var entity = GetEntity(entityId);
                        if (entity.EntityDefinition.GetType() == typeof(TableMappingItem))
                        {
                            newData.Clear();
                            LoadActualRow(
                                newData,
                                entityId,
                                query.MethodId,
                                rowArray[i],
                                userProfile,
                                transactionId
                            );
                            if (newData.Tables[0].Rows.Count == 0)
                            {
                                throw new Exception(
                                    ResourceUtils.GetString("NoDataRowAfterUpdate")
                                );
                            }
                            var detachedColumns = new List<string>();
                            foreach (
                                var column in entity.Columns.Where(column =>
                                    !(column.Field is FieldMappingItem)
                                )
                            )
                            {
                                detachedColumns.Add(column.Name);
                            }
                            try
                            {
                                rowArray[i].AcceptChanges();
                                rowArray[i].BeginEdit();
                                foreach (DataColumn column in table.Columns)
                                {
                                    if (
                                        detachedColumns.Contains(column.ColumnName)
                                        || column.ReadOnly
                                    )
                                    {
                                        continue;
                                    }
                                    object newValue = newData.Tables[0].Rows[0][column.ColumnName];
                                    if (!rowArray[i][column].Equals(newValue))
                                    {
                                        rowArray[i][column] = newData.Tables[0].Rows[0][
                                            column.ColumnName
                                        ];
                                    }
                                }
                                rowArray[i].EndEdit();
                                rowArray[i].AcceptChanges();
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(
                                    ResourceUtils.GetString(
                                        "FailedUpdateRow",
                                        rowArray[i].RowState.ToString()
                                    ),
                                    ex
                                );
                            }
                        }
                        else
                        {
                            rowArray[i].AcceptChanges();
                        }
                    }
                    else
                    {
                        rowArray[i].AcceptChanges();
                    }
                }
            }
        }
    }

    private DataStructureEntity GetEntity(Guid entityId)
    {
        return PersistenceProvider.RetrieveInstance(
                typeof(DataStructureEntity),
                new ModelElementKey(entityId)
            ) as DataStructureEntity;
    }

    private IDataEntityColumn GetField(Guid fieldId)
    {
        return PersistenceProvider.RetrieveInstance(
                typeof(ISchemaItem),
                new ModelElementKey(fieldId)
            ) as IDataEntityColumn;
    }

    private DataSet CloneDatasetForActualRow(DataTable table)
    {
        var newData = new DataSet(table.DataSet.DataSetName);
        newData.Tables.Add(new OrigamDataTable(table.TableName));
        foreach (DataColumn column in table.Columns)
        {
            var newColumn = new DataColumn(
                column.ColumnName,
                column.DataType,
                "",
                column.ColumnMapping
            );
            newColumn.MaxLength = column.MaxLength;
            newColumn.AllowDBNull = column.AllowDBNull;
            newColumn.DefaultValue = column.DefaultValue;
            newData.Tables[0].Columns.Add(newColumn);
        }
        return newData;
    }

    private void LoadActualRow(
        DataSet newData,
        Guid entityId,
        Guid filterSetId,
        DataRow row,
        IPrincipal userProfile,
        string transactionId
    )
    {
        var newDataQuery = new DataStructureQuery(entityId, filterSetId)
        {
            DataSourceType = QueryDataSourceType.DataStructureEntity,
        };
        foreach (DataColumn column in row.Table.PrimaryKey)
        {
            newDataQuery.Parameters.Add(
                row.RowState == DataRowState.Deleted
                    ? new QueryParameter(column.ColumnName, row[column, DataRowVersion.Original])
                    : new QueryParameter(column.ColumnName, row[column])
            );
        }
        LoadDataSet(newDataQuery, userProfile, newData, transactionId);
    }

    internal void TraceCommand(IDbCommand command, string transactionId)
    {
        if (!log.IsDebugEnabled)
        {
            return;
        }
        IDbCommand processIdCommand = command.Connection.CreateCommand();
        processIdCommand.CommandText = GetPid();
        processIdCommand.Transaction = command.Transaction;
        object spid = processIdCommand.ExecuteScalar();
        log.DebugFormat(
            "SQL Command; Connection ID: {0}, Transaction ID: {1}, {2}",
            spid,
            transactionId,
            command.CommandText
        );
        foreach (IDbDataParameter parameter in command.Parameters)
        {
            var paramValue = "NULL";
            if (parameter.Value != null)
            {
                paramValue = parameter.Value.ToString();
            }
            log.DebugFormat("Parameter: {0} Value: {1}", parameter.ParameterName, paramValue);
        }
    }

    internal abstract string GetPid();

    private void SetTransaction(DbDataAdapter adapter, IDbTransaction transaction)
    {
        ((IDbDataAdapter)adapter).SelectCommand.Transaction = transaction;
        ((IDbDataAdapter)adapter).UpdateCommand.Transaction = transaction;
        ((IDbDataAdapter)adapter).DeleteCommand.Transaction = transaction;
        ((IDbDataAdapter)adapter).InsertCommand.Transaction = transaction;
    }

    private void SetConnection(DbDataAdapter adapter, IDbConnection connection)
    {
        ((IDbDataAdapter)adapter).SelectCommand.Connection = connection;
        ((IDbDataAdapter)adapter).UpdateCommand.Connection = connection;
        ((IDbDataAdapter)adapter).DeleteCommand.Connection = connection;
        ((IDbDataAdapter)adapter).InsertCommand.Connection = connection;
    }
    #endregion
    #region IDisposable
    public override void Dispose()
    {
        connectionString = null;
        base.Dispose();
    }
    #endregion
}

// version of log4net for NetStandard 1.3 does not have the method
// LogManager.GetLogger(string)... have to use the overload with Type
// as parameter
public class WorkflowProfiling { }

internal class Profiler
{
    private static readonly ILog workflowProfilingLog = LogManager.GetLogger(typeof(Profiler));
    private readonly Dictionary<DataStructureEntity, List<double>> durationsMs =
        new Dictionary<DataStructureEntity, List<double>>();
    private readonly List<DataStructureEntity> entityOrder = new List<DataStructureEntity>();
    private static string currentTaskId;

    public void LogRememberedExecutionTimes()
    {
        var taskPath = (string)ThreadContext.Properties["currentTaskPath"];
        var taskId = (string)ThreadContext.Properties["currentTaskId"];
        var serviceMethodName = (string)ThreadContext.Properties["ServiceMethodName"];
        if (taskId == null)
        {
            return;
        }
        foreach (DataStructureEntity entity in entityOrder)
        {
            LogDuration(
                logEntryType: serviceMethodName,
                path: $"{taskPath}/Load/{entity.Name}",
                id: taskId,
                duration: durationsMs[entity].Sum(),
                rows: durationsMs[entity].Count
            );
        }
        durationsMs.Clear();
        entityOrder.Clear();
    }

    public void ExecuteAndRememberLoadDuration(DataStructureEntity entity, Action actionToExecute)
    {
        ExecuteAndTakeLoggingAction(entity, RememberLoadDuration, actionToExecute);
    }

    public void ExecuteAndLogStoreActionDuration(DataStructureEntity entity, Action actionToExecute)
    {
        ExecuteAndTakeLoggingAction(entity, LogStoreDuration, actionToExecute);
    }

    private static void ExecuteAndTakeLoggingAction(
        DataStructureEntity entity,
        Action<DataStructureEntity, Stopwatch> loggingAction,
        Action actionToExecute
    )
    {
        if (workflowProfilingLog.IsDebugEnabled)
        {
            var taskId = (string)ThreadContext.Properties["currentTaskId"];
            if (taskId != null)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                actionToExecute.Invoke();
                stopwatch.Stop();
                loggingAction.Invoke(entity, stopwatch);
                return;
            }
        }
        actionToExecute.Invoke();
    }

    private void RememberLoadDuration(DataStructureEntity entity, Stopwatch stoppedWatch)
    {
        var taskId = (string)ThreadContext.Properties["currentTaskId"];
        if (currentTaskId != taskId)
        {
            entityOrder.Clear();
            durationsMs.Clear();
            currentTaskId = taskId;
        }
        if (!entityOrder.Contains(entity))
        {
            entityOrder.Add(entity);
            durationsMs[entity] = new List<double>();
        }
        durationsMs[entity].Add(stoppedWatch.Elapsed.TotalMilliseconds);
    }

    private static void LogStoreDuration(DataStructureEntity entity, Stopwatch stoppedWatch)
    {
        var taskPath = (string)ThreadContext.Properties["currentTaskPath"];
        var taskId = (string)ThreadContext.Properties["currentTaskId"];
        var serviceMethodName = (string)ThreadContext.Properties["ServiceMethodName"];
        LogDuration(
            logEntryType: serviceMethodName,
            path: $"{taskPath}/Store/{entity.Name}",
            id: taskId,
            duration: stoppedWatch.Elapsed.TotalMilliseconds
        );
    }

    private static void LogDuration(
        string logEntryType,
        string path,
        string id,
        double duration,
        int rows = 0
    )
    {
        var typeWithDoubleColon = $"{logEntryType}:";
        var message =
            $"{typeWithDoubleColon, -18}{path, -80} Id: {id}  Duration: {duration, 7:0.0} ms";
        if (rows != 0)
        {
            message += " rows: " + rows;
        }
        workflowProfilingLog.Debug(message);
    }
}
