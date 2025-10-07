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
using System.Globalization;
using System.Security.Principal;
using System.Xml;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;
using static Origam.DA.Common.Enums;

namespace Origam.DA.Service;

/// <summary>
/// Abstract implementation of IDataService, based on ORIGAM metadata
/// </summary>
public abstract class AbstractDataService : IDataService
{
    private IPersistenceProvider _persistence = null; // = new OrigamPersistenceProvider();

    public AbstractDataService() { }

    public int BulkInsertThreshold { get; set; }
    public int UpdateBatchSize { get; set; }
    public abstract DatabaseType PlatformName { get; }
    private bool _userDefinedParameters = false;
    public bool UserDefinedParameters
    {
        get { return _userDefinedParameters; }
        set
        {
            _userDefinedParameters = value;

            if (this.DbDataAdapterFactory != null)
            {
                this.DbDataAdapterFactory.UserDefinedParameters = value;
            }
        }
    }
    public IPersistenceProvider PersistenceProvider
    {
        get { return _persistence; }
        set
        {
            lock (this)
            {
                // We flush cache
                //_adapterCache.Clear();
                // We got a persistence provider for our internal schema metadata
                _persistence = value;
            }
        }
    }
    private IStateMachineService _stateMachine;
    public IStateMachineService StateMachine
    {
        get { return _stateMachine; }
        set { _stateMachine = value; }
    }
    private IAttachmentService _attachmentService;
    public IAttachmentService AttachmentService
    {
        get { return _attachmentService; }
        set { _attachmentService = value; }
    }
    public abstract IDbDataAdapterFactory DbDataAdapterFactory { get; internal set; }
    internal abstract IDbConnection GetConnection(string connectionString);
    internal abstract IDbTransaction GetTransaction(string transactionId, IsolationLevel isolation);
    public abstract string BuildConnectionString(
        string serverName,
        int port,
        string databaseName,
        string userName,
        string password,
        bool integratedAuthentication,
        bool pooling
    );
    public abstract void CreateDatabase(string name);
    public abstract void DeleteDatabase(string name);
    public abstract void CreateDatabaseUser(
        string user,
        string password,
        string name,
        bool databaseIntegratedAuthentication
    );
    public abstract void DeleteUser(string user, bool _integratedAuthentication);

    public virtual string EntityDdl(Guid entity)
    {
        throw new NotImplementedException();
    }

    public virtual string[] FieldDdl(Guid field)
    {
        throw new NotImplementedException();
    }

    internal virtual void HandleException(Exception ex, string rowErrorMessage, DataRow row) { }

    internal DbDataAdapter GetAdapter(SelectParameters selectParameters, UserProfile userProfile)
    {
        string identityId = "";
        if (userProfile != null)
        {
            identityId = userProfile.Id.ToString();
        }
        bool hasDynamicFilter =
            selectParameters.Filter != null && selectParameters.Filter.IsDynamic;
        bool hasCustomFilters = !selectParameters.CustomFilters.IsEmpty;
        bool hasCustomOrdering = !selectParameters.CustomOrderings.IsEmpty;
        bool hasAggregateColumns =
            selectParameters.AggregatedColumns != null
            && selectParameters.AggregatedColumns.Count > 0;
        if (hasDynamicFilter || hasCustomFilters || hasCustomOrdering || hasAggregateColumns)
        {
            return GetAdapterNonCached(selectParameters);
        }
        else
        {
            return GetAdapterCached(selectParameters, identityId);
        }
    }

    internal DbDataAdapter GetSelectRowAdapter(
        DataStructureEntity entity,
        DataStructureFilterSet filterSet,
        ColumnsInfo columnsInfo
    )
    {
        return GetSelectRowAdapterCached(entity, filterSet, columnsInfo);
    }

    internal DbDataAdapter GetSelectRowAdapterNonCached(
        DataStructureEntity entity,
        DataStructureFilterSet filterSet,
        ColumnsInfo columnsInfo
    )
    {
        return DbDataAdapterFactory.CreateSelectRowDataAdapter(
            entity,
            filterSet,
            columnsInfo,
            true
        );
    }

    private DbDataAdapter GetSelectRowAdapterCached(
        DataStructureEntity entity,
        DataStructureFilterSet filterSet,
        ColumnsInfo columnsInfo
    )
    {
        string id = "selectRow_" + entity.PrimaryKey["Id"].ToString();
        if (filterSet != null)
        {
            id += "_" + filterSet.Id.ToString();
        }
        if (columnsInfo != null && !columnsInfo.IsEmpty)
        {
            id += "_" + columnsInfo;
        }
        Hashtable adapterCache = GetCache();
        // Caching adapters
        DbDataAdapter adapter;
        lock (adapterCache)
        {
            if (adapterCache.ContainsKey(id))
            {
                // adapter is in the cache
                adapter = adapterCache[id] as DbDataAdapter;
                adapter = this.DbDataAdapterFactory.CloneAdapter(adapter);
            }
            else
            {
                // adapter is not in the cache, yet
                adapter = GetSelectRowAdapterNonCached(entity, filterSet, columnsInfo);
                // so we add it there
                if (!adapterCache.ContainsKey(id))
                {
                    adapterCache.Add(id, adapter);
                }
            }
        }
        return adapter;
    }

    private DbDataAdapter GetAdapterNonCached(SelectParameters adParameters)
    {
        return DbDataAdapterFactory.CreateDataAdapter(
            adParameters,
            adParameters.ForceDatabaseCalculation
        );
    }

    private Hashtable GetCache()
    {
        Hashtable context = OrigamUserContext.Context;
        lock (context)
        {
            if (!context.Contains("DataAdapterCache"))
            {
                context.Add("DataAdapterCache", new Hashtable());
            }
        }
        return (Hashtable)OrigamUserContext.Context["DataAdapterCache"];
    }

    private DbDataAdapter GetAdapterCached(SelectParameters adParameters, string identityId)
    {
        string id = adParameters.Entity.PrimaryKey["Id"].ToString();
        if (adParameters.Filter != null)
        {
            id += adParameters.Filter.PrimaryKey["Id"].ToString();
        }
        if (adParameters.SortSet != null)
        {
            id += adParameters.SortSet.PrimaryKey["Id"].ToString();
        }
        if (adParameters.Paging)
        {
            id += "_paging";
        }
        if (!adParameters.ColumnsInfo.IsEmpty)
        {
            id += "_" + adParameters.ColumnsInfo;
        }
        Hashtable adapterCache = GetCache();
        // Caching adapters
        DbDataAdapter adapter;
        lock (adapterCache)
        {
            if (adapterCache.ContainsKey(id))
            {
                // adapter is in the cache
                adapter = adapterCache[id] as DbDataAdapter;
                adapter = this.DbDataAdapterFactory.CloneAdapter(adapter);
            }
            else
            {
                // adapter is not in the cache, yet
                adapter = GetAdapterNonCached(adParameters);
                // so we add it there
                if (!adapterCache.ContainsKey(id))
                {
                    adapterCache.Add(id, adapter);
                }
            }
        }
        return adapter;
    }

    internal DataStructureEntity GetDataStructureEntity(DataStructureQuery query)
    {
        DataStructureEntity result;
        try
        {
            result =
                this.PersistenceProvider.RetrieveInstance(
                    typeof(DataStructureEntity),
                    new ModelElementKey(query.DataSourceId)
                ) as DataStructureEntity;
        }
        catch
        {
            throw new ArgumentOutOfRangeException(
                "id",
                query.DataSourceId,
                ResourceUtils.GetString("DataStructureNotFound")
            );
        }
        if (result == null)
        {
            throw new ArgumentOutOfRangeException(
                "id",
                query.DataSourceId,
                ResourceUtils.GetString("DataStructureNotFound")
            );
        }
        return result;
    }

    internal DataStructure GetDataStructure(DataStructureQuery query)
    {
        return GetDataStructure(query.DataSourceId);
    }

    internal DataStructure GetDataStructure(Guid id)
    {
        if (id == Guid.Empty)
            return null;
        DataStructure result =
            this.PersistenceProvider.RetrieveInstance(
                typeof(DataStructure),
                new ModelElementKey(id)
            ) as DataStructure;
        if (result == null)
        {
            throw new ArgumentException(
                ResourceUtils.GetString("StructureNotInModel", id.ToString())
            );
        }
        return result;
    }

    internal TableMappingItem GetTable(Guid id)
    {
        TableMappingItem result =
            this.PersistenceProvider.RetrieveInstance(
                typeof(TableMappingItem),
                new ModelElementKey(id)
            ) as TableMappingItem;
        if (result == null)
        {
            throw new ArgumentException(
                ResourceUtils.GetString("TableMappingNotInModel", id.ToString())
            );
        }
        return result;
    }

    internal FieldMappingItem GetTableColumn(Guid id)
    {
        FieldMappingItem result =
            this.PersistenceProvider.RetrieveInstance(
                typeof(FieldMappingItem),
                new ModelElementKey(id)
            ) as FieldMappingItem;
        if (result == null)
        {
            throw new ArgumentException(
                ResourceUtils.GetString("FieldMappingNotInModel", id.ToString())
            );
        }
        return result;
    }

    internal DataStructureFilterSet GetFilterSet(Guid id)
    {
        if (id == Guid.Empty)
        {
            return null;
        }
        Object retrievedInstance;
        try
        {
            retrievedInstance = this.PersistenceProvider.RetrieveInstance(
                typeof(DataStructureFilterSet),
                new ModelElementKey(id)
            );
        }
        catch
        {
            throw new ArgumentOutOfRangeException(
                "id",
                id,
                ResourceUtils.GetString("FilterSetNotFound")
            );
        }
        DataStructureFilterSet result = retrievedInstance as DataStructureFilterSet;
        if (result == null)
        {
            throw new ArgumentException(
                ResourceUtils.GetString(
                    "InvalidTypeRetrieved",
                    id,
                    "DataStructureFilterSet",
                    retrievedInstance.GetType().FullName
                )
            );
        }
        return result;
    }

    internal DataStructureSortSet GetSortSet(Guid id)
    {
        DataStructureSortSet result;
        try
        {
            result =
                this.PersistenceProvider.RetrieveInstance(
                    typeof(DataStructureSortSet),
                    new ModelElementKey(id)
                ) as DataStructureSortSet;
        }
        catch
        {
            throw new ArgumentOutOfRangeException(
                "id",
                id,
                ResourceUtils.GetString("SortSetNotFound")
            );
        }
        return result;
    }

    internal DataStructureDefaultSet GetDefaultSet(Guid id)
    {
        DataStructureDefaultSet result;
        try
        {
            result =
                this.PersistenceProvider.RetrieveInstance(
                    typeof(DataStructureDefaultSet),
                    new ModelElementKey(id)
                ) as DataStructureDefaultSet;
        }
        catch
        {
            throw new ArgumentOutOfRangeException(
                "id",
                id,
                ResourceUtils.GetString("DefaultSetNotFound")
            );
        }
        return result;
    }

    internal DataSet GetDataset(DataStructure ds, Guid defaultSetId, bool includeCalculatedColumns)
    {
        return new DatasetGenerator(UserDefinedParameters).CreateDataSet(
            ds,
            includeCalculatedColumns,
            GetDefaultSet(defaultSetId)
        );
    }

    internal DataSet GetDataset(DataStructure ds, Guid defaultSetId)
    {
        return new DatasetGenerator(UserDefinedParameters).CreateDataSet(
            ds,
            GetDefaultSet(defaultSetId)
        );
    }

    internal void BuildParameters(
        QueryParameterCollection parameters,
        IDataParameterCollection dsParameters,
        UserProfile currentProfile
    )
    {
        foreach (IDbDataParameter dbParam in dsParameters)
        {
            if (dbParam.IsNullable && dbParam.Value == null)
            {
                dbParam.Value = DBNull.Value;
            }
            ICustomParameter customParameter = CustomParameterService.MatchParameter(
                dbParam.ParameterName
            );
            if (customParameter != null)
            {
                // this is a system parameter
                dbParam.Value = customParameter.Evaluate(currentProfile);
            }
            else
            {
                // this is a standard parameter, we assign it from the input parameters
                QueryParameter param = null;
                foreach (QueryParameter p in parameters)
                {
                    if (
                        p.Name
                        == dbParam.ParameterName.Substring(
                            this.DbDataAdapterFactory.ParameterDeclarationChar.Length
                        )
                    )
                    {
                        param = p;
                        break;
                    }
                }
                if (param != null)
                {
                    object value = null;
                    try
                    {
                        // setting null value parameter
                        if (param.Value == null)
                        {
                            if (dbParam.IsNullable)
                            {
                                dbParam.Value = DBNull.Value;
                            }
                            else
                            {
                                dbParam.Value = null;
                            }
                        }
                        // setting all other parameters value (type conversion)
                        else
                        {
                            switch (dbParam.DbType)
                            {
                                case DbType.Guid:
                                    if (param.Value is Guid)
                                        value = param.Value;
                                    else if (
                                        param.Value is string & dbParam.IsNullable
                                        && ((string)param.Value == "")
                                    )
                                        value = DBNull.Value;
                                    else if (param.Value is String)
                                        value = new Guid(param.Value.ToString());
                                    else if (param.Value == DBNull.Value)
                                        value = DBNull.Value;
                                    break;
                                case DbType.DateTime:
                                    //							dbParam.Size = 8;
                                    //							dbParam.DbType = DbType.String;
                                    //							DateTimeFormatInfo myDTFI = new CultureInfo( "en-US", false ).DateTimeFormat;
                                    //							value = System.Xml.XmlConvert.ToDateTime(param.Value.ToString()).ToString("MMM dd yyyy hh:mm:sstt", myDTFI);
                                    if (param.Value is DateTime)
                                    {
                                        value = param.Value;
                                    }
                                    else if (param.Value == DBNull.Value)
                                    {
                                        value = DBNull.Value;
                                    }
                                    // if number is passed to the DateTime parameter, it will be automatically computed as number of days
                                    // relative to the current date
                                    else if (param.Value is int)
                                    {
                                        value = DateTime.Now.AddDays(
                                            Convert.ToDouble((int)param.Value)
                                        );
                                    }
                                    else
                                    {
                                        value = System.Xml.XmlConvert.ToDateTime(
                                            param.Value.ToString(),
                                            XmlDateTimeSerializationMode.RoundtripKind
                                        );
                                    }
                                    break;
                                case DbType.String:
                                    if (param.Value == DBNull.Value)
                                    {
                                        value = DBNull.Value;
                                    }
                                    else
                                    {
                                        var a = param.Value as ArrayList;
                                        if (a != null)
                                        {
                                            string delimiter = "|";
                                            System.Text.StringBuilder sb =
                                                new System.Text.StringBuilder();
                                            foreach (object v in a)
                                            {
                                                sb.Append(delimiter);
                                                sb.Append(v.ToString());
                                            }
                                            sb.Append(delimiter);
                                            value = sb.ToString();
                                        }
                                        else
                                        {
                                            value = param.Value.ToString();
                                        }
                                    }
                                    break;
                                case DbType.Int32:
                                    if (param.Value == DBNull.Value)
                                    {
                                        value = DBNull.Value;
                                    }
                                    else if (param.Value is string)
                                    {
                                        value = System.Xml.XmlConvert.ToInt32(
                                            param.Value as string
                                        );
                                    }
                                    else
                                    {
                                        value = param.Value;
                                    }
                                    break;
                                case DbType.Int64:
                                    if (param.Value == DBNull.Value)
                                    {
                                        value = DBNull.Value;
                                    }
                                    else if (param.Value is string)
                                    {
                                        value = System.Xml.XmlConvert.ToInt64(
                                            param.Value as string
                                        );
                                    }
                                    else
                                    {
                                        value = param.Value;
                                    }
                                    break;
                                case DbType.Decimal:
                                case DbType.Currency:
                                    if (param.Value == DBNull.Value)
                                    {
                                        value = DBNull.Value;
                                    }
                                    else if (param.Value is string)
                                    {
                                        value = System.Xml.XmlConvert.ToDecimal(
                                            param.Value as string
                                        );
                                    }
                                    else
                                    {
                                        value = param.Value;
                                    }
                                    break;
                                default:
                                    if (
                                        dbParam.DbType == DbType.Object
                                        && !(param.Value is ICollection)
                                    )
                                    {
                                        // support passing single value to an array parameter
                                        param.Value = new ArrayList { param.Value };
                                    }
                                    ICollection ar = param.Value as ICollection;
                                    if (ar != null)
                                    {
                                        value = FillParameterArrayData(ar);
                                    }
                                    else
                                    {
                                        value = param.Value;
                                    }
                                    break;
                            }
                            dbParam.Value = value;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            ResourceUtils.GetString(
                                "ErrorWhenSettingParam",
                                dbParam.ParameterName,
                                param.Value
                            ),
                            ex
                        );
                    }
                }
            }
        }
    }

    internal abstract object FillParameterArrayData(ICollection ar);

    internal DataAuditLog GetLog(
        DataTable table,
        UserProfile profile,
        string transactionId,
        int overrideActionType
    )
    {
        if (!table.ExtendedProperties.Contains(Const.EntityAuditingAttribute))
        {
            return null;
        }
        if (
            (EntityAuditingType)table.ExtendedProperties[Const.EntityAuditingAttribute]
            == EntityAuditingType.None
        )
        {
            return null;
        }
        DataAuditLog log = new DataAuditLog();
        var timestamp = DateTime.Now;
        foreach (DataRow row in table.Rows)
        {
            foreach (DataColumn column in table.Columns)
            {
                if (
                    !column.ExtendedProperties.Contains(Const.TemporaryColumnAttribute)
                    && !column.ExtendedProperties.Contains(Const.ArrayRelation)
                    && (
                        (EntityAuditingType)column.ExtendedProperties[Const.EntityAuditingAttribute]
                        != EntityAuditingType.None
                    )
                    && (
                        (
                            (row.RowState == DataRowState.Added)
                            && (
                                (EntityAuditingType)
                                    column.ExtendedProperties[Const.EntityAuditingAttribute]
                                == EntityAuditingType.All
                            )
                        )
                        || (row.RowState == DataRowState.Deleted)
                        || (
                            (row.RowState == DataRowState.Modified)
                            && !row[column, DataRowVersion.Current]
                                .Equals(row[column, DataRowVersion.Original])
                        )
                    )
                )
                {
                    DataAuditLog.AuditRecordRow logRow = GetNewLogRow(
                        log,
                        row,
                        column,
                        overrideActionType
                    );
                    if (
                        (row.RowState != DataRowState.Deleted)
                        && table.Columns.Contains("RecordUpdated")
                        && (row["RecordUpdated"] != DBNull.Value)
                    )
                    {
                        logRow.RecordCreated = (DateTime)row["RecordUpdated"];
                    }
                    else
                    {
                        logRow.RecordCreated = timestamp;
                    }
                    if (
                        (row.RowState == DataRowState.Modified)
                        && table.Columns.Contains("RecordUpdatedBy")
                        && (row["RecordUpdatedBy"] != DBNull.Value)
                    )
                    {
                        logRow.RecordCreatedBy = (Guid)row["RecordUpdatedBy"];
                    }
                    else
                    {
                        if (profile != null)
                        {
                            logRow.RecordCreatedBy = profile.Id;
                        }
                    }
                    if (
                        (row.RowState == DataRowState.Added)
                        && table.Columns.Contains("RecordCreatedBy")
                        && (row["RecordCreatedBy"] != DBNull.Value)
                    )
                    {
                        logRow.RecordCreatedBy = (Guid)row["RecordCreatedBy"];
                    }
                    else
                    {
                        if (profile != null)
                        {
                            logRow.RecordCreatedBy = profile.Id;
                        }
                    }
                    if (row.RowState == DataRowState.Deleted)
                    {
                        if (profile != null)
                        {
                            logRow.RecordCreatedBy = profile.Id;
                        }
                        if (
                            (
                                table.ExtendedProperties[
                                    Const.AuditingSecondReferenceKeyColumnAttribute
                                ]
                                is IDataEntityColumn dataEntityColumn
                            ) && (table.Columns.Contains(dataEntityColumn.Name))
                        )
                        {
                            logRow.SecondReferenceKey = (Guid)
                                row[dataEntityColumn.Name, DataRowVersion.Original];
                        }
                    }
                    if (column.ExtendedProperties.Contains(Const.DefaultLookupIdAttribute))
                    {
                        IDataLookupService lookupService =
                            ServiceManager.Services.GetService(typeof(IDataLookupService))
                            as IDataLookupService;
                        // this is a lookup column, we pass the looked-up value
                        Guid lookupId = (Guid)
                            column.ExtendedProperties[Const.DefaultLookupIdAttribute];
                        if (row.RowState != DataRowState.Deleted)
                        {
                            logRow.NewValue = lookupService
                                .GetDisplayText(
                                    lookupId,
                                    row[column, DataRowVersion.Current],
                                    transactionId
                                )
                                .ToString();
                            logRow.NewValueId = row[column, DataRowVersion.Current].ToString();
                        }
                        if (row.RowState != DataRowState.Added)
                        {
                            logRow.OldValue = lookupService
                                .GetDisplayText(
                                    lookupId,
                                    row[column, DataRowVersion.Original],
                                    transactionId
                                )
                                .ToString();
                            logRow.OldValueId = row[column, DataRowVersion.Original].ToString();
                        }
                    }
                    else
                    {
                        // this is not a lookup column, we pass the original value
                        if (row.RowState != DataRowState.Deleted)
                        {
                            logRow.NewValue = row[column, DataRowVersion.Current].ToString();
                        }
                        if (row.RowState != DataRowState.Added)
                        {
                            logRow.OldValue = row[column, DataRowVersion.Original].ToString();
                        }
                    }
                    log.AuditRecord.AddAuditRecordRow(logRow);
                }
            }
        }
        return log;
    }

    private DataAuditLog.AuditRecordRow GetNewLogRow(
        DataAuditLog log,
        DataRow row,
        DataColumn column,
        int overrideActionType
    )
    {
        string pkName = row.Table.PrimaryKey[0].ColumnName;
        DataAuditLog.AuditRecordRow logRow = log.AuditRecord.NewAuditRecordRow();
        logRow.Id = Guid.NewGuid();
        if (row.RowState == DataRowState.Deleted)
        {
            logRow.refParentRecordId = (Guid)row[pkName, DataRowVersion.Original];
        }
        else
        {
            logRow.refParentRecordId = (Guid)row[pkName];
        }
        logRow.refParentRecordEntityId = (Guid)row.Table.ExtendedProperties["EntityId"];
        logRow.refColumnId = (Guid)column.ExtendedProperties["Id"];
        if (overrideActionType == -1)
        {
            logRow.ActionType = (int)row.RowState;
        }
        else
        {
            logRow.ActionType = overrideActionType;
        }

        return logRow;
    }

    #region IDataService Members
    public virtual string Info
    {
        get { return this.ToString(); }
    }
    public abstract string ConnectionString { get; set; }
    public abstract string DbUser { get; set; }

    public string Xsd(Guid dataStructureId)
    {
        System.Text.StringBuilder mysb = new System.Text.StringBuilder();
        // Create the StringWriter object with the StringBuilder object.
        System.IO.StringWriter myStringWriter = new System.IO.StringWriter(mysb);
        // Write the schema into the StringWriter.
        this.GetEmptyDataSet(dataStructureId).WriteXmlSchema(myStringWriter);
        return myStringWriter.ToString();
    }

    public DataSet GetEmptyDataSet(Guid dataStructureId)
    {
        return GetEmptyDataSet(dataStructureId, null);
    }

    public DataSet GetEmptyDataSet(Guid dataStructureId, CultureInfo culture)
    {
        ModelElementKey key = new ModelElementKey();
        key.Id = dataStructureId;
        DataStructure ds =
            _persistence.RetrieveInstance(typeof(DataStructure), key) as DataStructure;
        return new DatasetGenerator(UserDefinedParameters).CreateDataSet(ds, culture);
    }

    public abstract DataSet LoadDataSet(
        DataStructureQuery dataStructureQuery,
        IPrincipal userProfile,
        string transactionId
    );
    public abstract DataSet LoadDataSet(
        DataStructureQuery dataStructureQuery,
        IPrincipal userProfile,
        DataSet dataSet,
        string transactionId
    );
    public abstract int UpdateData(
        DataStructureQuery dataStructureQuery,
        IPrincipal userProfile,
        DataSet ds,
        string transactionid
    );
    public abstract int UpdateData(
        DataStructureQuery dataStructureQuery,
        IPrincipal userProfile,
        DataSet ds,
        string transactionid,
        bool forceBulkInsert
    );
    public abstract object GetScalarValue(
        DataStructureQuery query,
        ColumnsInfo columnsInfo,
        IPrincipal userProfile,
        string transactionId
    );
    public abstract DataSet ExecuteProcedure(
        string name,
        string EntityOrder,
        DataStructureQuery query,
        string transactionid
    );
    public abstract List<SchemaDbCompareResult> CompareSchema(IPersistenceProvider provider);
    public abstract bool IsSchemaItemInDatabase(ISchemaItem schemaItem);

    public virtual string ExecuteUpdate(string command, string transactionId)
    {
        throw new NotImplementedException(
            "ExecuteUpdate() is not implemented by this data service"
        );
    }

    public virtual string DatabaseSchemaVersion()
    {
        throw new NotImplementedException(
            "DatabaseSchemaVersion() is not implemented by this data service"
        );
    }

    public virtual void UpdateDatabaseSchemaVersion(string version, string transactionId)
    {
        throw new NotImplementedException(
            "UpdateDatabaseSchemaVersion() is not implemented by this data service"
        );
    }

    public abstract int UpdateField(
        Guid entityId,
        Guid fieldId,
        object oldValue,
        object newValue,
        IPrincipal userProfile,
        string transactionId
    );
    public abstract int ReferenceCount(
        Guid entityId,
        Guid fieldId,
        object value,
        IPrincipal userProfile,
        string transactionId
    );
    public abstract string[] DatabaseSpecificDatatypes();
    public abstract IDataReader ExecuteDataReader(
        DataStructureQuery dataStructureQuery,
        IPrincipal userProfile,
        string transactionId
    );

    public abstract IEnumerable<IEnumerable<object>> ExecuteDataReader(
        DataStructureQuery dataStructureQuery
    );
    public abstract IEnumerable<Dictionary<string, object>> ExecuteDataReaderReturnPairs(
        DataStructureQuery query
    );

    public abstract void DiagnoseConnection();

    #endregion
    #region IDisposable Members
    public virtual void Dispose()
    {
        _persistence = null;
        DbDataAdapterFactory.Dispose();
    }
    #endregion
}
