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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using MoreLinq;
using Newtonsoft.Json.Linq;
using Origam.DA;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.Gui;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Schema.GuiModel;
using Origam.Server.Common;
using Origam.Service.Core;
using CoreServices = Origam.Workbench.Services.CoreServices;
#pragma warning disable CS1572 // XML comment has a param tag, but there is no parameter by that name
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

namespace Origam.Server;

public abstract class SessionStore : IDisposable
{
    protected readonly bool dataRequested;
    internal static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    private IBasicUIService _service;
    private static Ascii85 _ascii85 = new Ascii85();
    private string _transactionId = null;
    private bool _isDelayedLoading = false;
    private Guid _id;
    private SessionStore _parentSession;
    private Guid _formId;
    private string _name;
    private string _title;
    private DataStructureRuleSet _ruleSet;
    private DataStructureSortSet _sortSet;
    private RuleEngine _ruleEngine;
    private DatasetRuleHandler _ruleHandler;
    private DataSet _data;
    private DataSet _dataList;
    private string _dataListEntity;
    private Guid _dataListDataStructureEntityId;
    private Guid _dataListFilterSetId;
    private IList<string> _dataListLoadedColumns = new List<string>();
    private DateTime _cacheExpiration;
    private UIRequest _request;
    private IList<SessionStore> _childSessions = new List<SessionStore>();
    private SessionStore _activeSession;
    private IList<FormNotification> _notifications = new List<FormNotification>();
    private bool _refreshOnInitUI;
    private SaveRefreshType _refreshAfterSaveType;
    internal object _lock = new object();
    private bool _eventsRegistered;
    private object _currentRecordId = null;
    private bool _isPagedLoading = false;
    private Dictionary<string, bool> _entityHasRuleDependencies = new Dictionary<string, bool>();
    private IList<string> _dirtyEnabledEntities = new List<string>();
    private bool _isModalDialog = false;
    private List<ChangeInfo> _pendingChanges = null;
    private bool _isModalDialogCommited = false;
    private IEndRule _confirmationRule = null;
    private IDictionary<string, IDictionary> _variables = new Dictionary<string, IDictionary>();
    private bool _supressSave = false;
    private bool _refreshPortalAfterSave = false;
    private bool _isExclusive = false;
    private readonly Analytics analytics;
    private bool _isDisposed;
    private IDataDocument _xmlData;
    private bool _isProcessing;
    public bool IsProcessing
    {
        get { return _isProcessing; }
        set { _isProcessing = value; }
    }
    public const string LIST_LOADED_COLUMN_NAME = "___ORIGAM_IsLoaded";
    public const string ACTION_SAVE = "SAVE";
    public const string ACTION_REFRESH = "REFRESH";
    public const string ACTION_NEXT = "NEXT";
    public const string ACTION_QUERYNEXT = "QUERYNEXT";
    public const string ACTION_ABORT = "ABORT";
    public const string ACTION_REPEAT = "REPEAT";

    public SessionStore(
        IBasicUIService service,
        UIRequest request,
        string name,
        Analytics analytics
    )
    {
        this.analytics = analytics;
        this.Name = name;
        this.Service = service;
        if (request.FormSessionId == null)
        {
            this.Id = Guid.NewGuid();
        }
        else
        {
            this.Id = new Guid(g: request.FormSessionId);
        }
        this.Request = request;
        this.IsModalDialog = request.IsModalDialog;
        _ruleHandler = new DatasetRuleHandler();
        _ruleEngine = RuleEngine.Create(contextStores: null, transactionId: null);
        this.CacheExpiration = DateTime.Now.AddMinutes(value: 5);
        dataRequested = request.DataRequested || request.IsSingleRecordEdit;
    }

    public string TransationId
    {
        get { return _transactionId; }
        set
        {
            _transactionId = value;
            if (this.RuleEngine != null)
            {
                this.RuleEngine.TransactionId = _transactionId;
            }
        }
    }
    public IBasicUIService Service
    {
        get
        {
            if (_isDisposed)
            {
                throw new ServerObjectDisposedException(message: Strings.SessionStoreDisposed);
            }
            return _service;
        }
        set { _service = value; }
    }
    public SessionStore ParentSession
    {
        get
        {
            if (_isDisposed)
            {
                throw new ServerObjectDisposedException(message: Strings.SessionStoreDisposed);
            }
            return _parentSession;
        }
        set { _parentSession = value; }
    }
    public List<ChangeInfo> PendingChanges
    {
        get { return _pendingChanges; }
        set { _pendingChanges = value; }
    }
    public Guid Id
    {
        get { return _id; }
        set { _id = value; }
    }
    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }
    public virtual string Title
    {
        get { return _title ?? this.Request.Caption; }
        set { _title = value; }
    }
    public object CurrentRecordId
    {
        get { return _currentRecordId; }
        set { _currentRecordId = value; }
    }
    public bool IsPagedLoading
    {
        get { return _isPagedLoading; }
        set { _isPagedLoading = value; }
    }
    public SessionStore ActiveSession
    {
        get
        {
            if (_isDisposed)
            {
                throw new ServerObjectDisposedException(message: Strings.SessionStoreDisposed);
            }
            return _activeSession;
        }
        set { _activeSession = value; }
    }
    public RuleEngine RuleEngine
    {
        get
        {
            if (_isDisposed)
            {
                throw new ServerObjectDisposedException(message: Strings.SessionStoreDisposed);
            }
            return _ruleEngine;
        }
    }
    public DatasetRuleHandler RuleHandler
    {
        get
        {
            if (_isDisposed)
            {
                throw new ServerObjectDisposedException(message: Strings.SessionStoreDisposed);
            }
            return _ruleHandler;
        }
    }
    public DataSet Data
    {
        get
        {
            if (_isDisposed)
            {
                throw new ServerObjectDisposedException(message: Strings.SessionStoreDisposed);
            }
            return _data;
        }
    }
    public DataSet DataList
    {
        get { return _dataList; }
    }
    public string DataListEntity
    {
        get { return _dataListEntity; }
        set { _dataListEntity = value; }
    }
    public Guid DataListDataStructureEntityId
    {
        get { return _dataListDataStructureEntityId; }
    }
    public Guid DataListFilterSetId
    {
        get { return _dataListFilterSetId; }
    }
    public IList<string> DataListLoadedColumns
    {
        get { return _dataListLoadedColumns; }
    }
    public DataSet InitialData
    {
        get { return DataList == null ? Data : DataList; }
    }
    public UIRequest Request
    {
        get { return _request; }
        set { _request = value; }
    }
    public IDataDocument XmlData
    {
        get
        {
            if (_isDisposed)
            {
                throw new ServerObjectDisposedException(message: Strings.SessionStoreDisposed);
            }
            return _xmlData;
        }
        private set => _xmlData = value;
    }
    public DataStructureRuleSet RuleSet
    {
        get
        {
            if (_isDisposed)
            {
                throw new ServerObjectDisposedException(message: Strings.SessionStoreDisposed);
            }
            return _ruleSet;
        }
        set
        {
            _ruleSet = value;
            InitEntityDependencies();
        }
    }
    public bool HasRules
    {
        get
        {
            // has ruleset
            if (this.RuleSet != null)
            {
                return true;
            }
            // has some lookup fields that are processed (looked up on changes)
            // by the rule engine
            if (this.Data != null)
            {
                foreach (DataTable table in this.Data.Tables)
                {
                    foreach (DataColumn column in table.Columns)
                    {
                        if (column.ExtendedProperties.Contains(key: Const.OriginalFieldId))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
    public DataStructureSortSet SortSet
    {
        get { return _sortSet; }
        set { _sortSet = value; }
    }
    public DateTime CacheExpiration
    {
        get { return _cacheExpiration; }
        set { _cacheExpiration = value; }
    }
    public Guid FormId
    {
        get { return _formId; }
        set { _formId = value; }
    }
    public IList<FormNotification> Notifications
    {
        get { return _notifications; }
        set { _notifications = value; }
    }
    public bool RefreshOnInitUI
    {
        get { return _refreshOnInitUI; }
        set { _refreshOnInitUI = value; }
    }
    public SaveRefreshType RefreshAfterSaveType
    {
        get { return _refreshAfterSaveType; }
        set { _refreshAfterSaveType = value; }
    }
    public bool RefreshPortalAfterSave
    {
        get { return _refreshPortalAfterSave; }
        set { _refreshPortalAfterSave = value; }
    }
    public IList<SessionStore> ChildSessions
    {
        get { return _childSessions; }
    }
    public bool IsDelayedLoading
    {
        get { return _isDelayedLoading; }
        set { _isDelayedLoading = value; }
    }
    public bool IsModalDialog
    {
        get { return _isModalDialog; }
        set { _isModalDialog = value; }
    }
    public bool IsModalDialogCommited
    {
        get { return _isModalDialogCommited; }
        set { _isModalDialogCommited = value; }
    }
    public bool SuppressSave
    {
        get { return _supressSave; }
        set { _supressSave = value; }
    }
    public IEndRule ConfirmationRule
    {
        get { return _confirmationRule; }
        set { _confirmationRule = value; }
    }
    public IDictionary<string, IDictionary> Variables
    {
        get { return _variables; }
    }
    public IList<string> DirtyEnabledEntities
    {
        get { return _dirtyEnabledEntities; }
    }
    public virtual bool SupportsFormXmlAsync
    {
        get { return false; }
    }
    public bool IsExclusive
    {
        get { return _isExclusive; }
        set { _isExclusive = value; }
    }
    public virtual string HelpTooltipFormId
    {
        get { return FormId.ToString(); }
    }

    public void AddChildSession(SessionStore ss)
    {
        this.ChildSessions.Add(item: ss);
        ss.ParentSession = this;
    }

    public void Clear()
    {
        lock (_lock)
        {
            XmlData = null;
            _data = null;
        }
    }

    public void SetDataList(
        DataSet list,
        string entity,
        DataStructure listDataStructure,
        DataStructureMethod method
    )
    {
        _dataList = list;
        if (method is DataStructureFilterSet filterSet)
        {
            _dataListFilterSetId = filterSet.Id;
        }
        else if (method is Schema.WorkflowModel.DataStructureWorkflowMethod workflowMethod)
        {
            _dataListFilterSetId = workflowMethod.Id;
        }
        else if (method != null)
        {
            throw new ArgumentOutOfRangeException(
                paramName: "method",
                message: "List method must be a filter set."
            );
        }
        if (this.DataList != null)
        {
            _dataListEntity = entity;
            foreach (DataStructureEntity e in listDataStructure.Entities)
            {
                if (e.Name == entity)
                {
                    _dataListDataStructureEntityId = e.Id;
                    break;
                }
            }
            DataList.RemoveNullConstraints();
        }
    }

    public static DataRowCollection LoadRows(
        IDataService dataService,
        DataStructureEntity entity,
        Guid dataStructureEntityId,
        Guid methodId,
        IList rowIds
    )
    {
        DataStructureQuery query = new DataStructureQuery
        {
            DataSourceType = QueryDataSourceType.DataStructureEntity,
            DataSourceId = dataStructureEntityId,
            Entity = entity.Name,
            EnforceConstraints = false,
            MethodId = methodId,
        };
        query.Parameters.Add(value: new QueryParameter(_parameterName: "Id", value: rowIds));
        DataSet dataSet = dataService.GetEmptyDataSet(
            dataStructureId: entity.RootEntity.ParentItemId,
            culture: CultureInfo.InvariantCulture
        );
        dataService.LoadDataSet(
            dataStructureQuery: query,
            userProfile: SecurityManager.CurrentPrincipal,
            dataSet: dataSet,
            transactionId: null
        );
        DataTable dataSetTable = dataSet.Tables[name: entity.Name];
        return dataSetTable.Rows;
    }

    public virtual bool HasChanges()
    {
        return false;
    }

    public void SetDataSource(object dataSource)
    {
        // set the new data
        if (dataSource is DataSet)
        {
            _data = dataSource as DataSet;
            bool selfJoinExists = false;
            foreach (DataRelation r in Data.Relations)
            {
                if (r.ParentTable.Equals(obj: r.ChildTable))
                {
                    selfJoinExists = true;
                    break;
                }
            }
            // no XML for self joins (incompatible with XmlDataDocument)
            if (!selfJoinExists)
            {
                XmlData = DataDocumentFactory.New(dataSet: Data);
            }
        }
        else if (dataSource is IDataDocument)
        {
            XmlData = dataSource as IDataDocument;
            _data = XmlData.DataSet;
        }
        else if (dataSource == null)
        {
            XmlData = null;
            _data = null;
        }
        else
        {
            throw new ArgumentOutOfRangeException(
                paramName: "dataSource",
                actualValue: dataSource,
                message: "Invalid session data format."
            );
        }
        if (this.Data != null)
        {
            Data.RemoveNullConstraints();
            DatasetGenerator.ApplyDynamicDefaults(
                data: this.Data,
                parameters: this.Request.Parameters
            );
            InitEntityDependencies();
        }
    }

    private void InitEntityDependencies()
    {
        _entityHasRuleDependencies.Clear();
        if (this.Data != null)
        {
            foreach (DataTable table in this.Data.Tables)
            {
                bool hasDependencies = HasColumnDependencies(table: table);
                if (!hasDependencies)
                {
                    hasDependencies = HasRuleDependencies(table: table);
                }
                _entityHasRuleDependencies[key: table.TableName] = hasDependencies;
            }
        }
    }

    private bool HasRuleDependencies(DataTable table)
    {
        bool result = false;
        // rule dependencies
        if (this.RuleSet != null)
        {
            foreach (DataStructureRule rule in RuleSet.Rules())
            {
                if (rule.Entity.Name != table.TableName)
                {
                    int count = 0;
                    foreach (DataStructureRuleDependency dependency in rule.RuleDependencies)
                    {
                        count++;
                        if (dependency.Entity.Name == table.TableName)
                        {
                            result = true;
                            break;
                        }
                    }
                    if (result)
                    {
                        break;
                    }
                    // if there are no dependencies, this entity refreshes on any updates,
                    // that means also on updates from our table
                    result = (count == 0);
                }
            }
        }
        return result;
    }

    private bool HasColumnDependencies(DataTable table)
    {
        string childRelationExpression = "CHILD(" + table.TableName.ToUpper() + ")";
        bool result = false;
        // check column expression dependencies
        foreach (DataTable otherTable in this.Data.Tables)
        {
            foreach (DataColumn col in otherTable.Columns)
            {
                if (
                    col.Expression != ""
                    && col.Expression != null
                    && (
                        col.Expression.ToUpper().Contains(value: childRelationExpression)
                        || (col.Expression.Contains(value: "Parent."))
                    )
                )
                {
                    result = true;
                    break;
                }
            }
            if (result)
            {
                break;
            }
        }
        return result;
    }

    public void RegisterEvents()
    {
        if (_eventsRegistered)
        {
            return;
        }
        _eventsRegistered = true;
        if (XmlData != null)
        {
            RuleHandler.RegisterDatasetEvents(
                xmlData: XmlData,
                ruleSet: RuleSet,
                ruleEngine: RuleEngine
            );
        }
    }

    public void UnregisterEvents()
    {
        if (!_eventsRegistered)
        {
            return;
        }
        _eventsRegistered = false;
        if (XmlData != null)
        {
            RuleHandler.UnregisterDatasetEvents(xmlData: XmlData);
        }
    }

    public virtual List<ChangeInfo> RestoreData(object parentId)
    {
        throw new NotImplementedException();
    }

    public virtual void LoadColumns(IList<string> columns) { }

    public abstract void Init();

    public object ExecuteAction(string actionId)
    {
        if (this.IsProcessing)
        {
            throw new UserOrigamException(message: Resources.ErrorCommandInProgress);
        }
        this.IsProcessing = true;
        try
        {
            lock (this._lock)
            {
                return ExecuteActionInternal(actionId: actionId);
            }
        }
        finally
        {
            this.IsProcessing = false;
        }
    }

    public abstract object ExecuteActionInternal(string actionId);
    public abstract XmlDocument GetFormXml();

    public virtual void PrepareFormXml()
    {
        throw new NotSupportedException();
    }

    #region IDisposable Members
    public void Dispose()
    {
        analytics.SetProperty(propertyName: "OrigamFormId", value: this.FormId);
        analytics.SetProperty(propertyName: "OrigamFormName", value: this.Name);
        analytics.Log(message: "UI_CLOSEFORM");
        if (this.ParentSession != null)
        {
            this.ParentSession.ChildSessions.Remove(item: this);
        }
        foreach (SessionStore child in this.ChildSessions)
        {
            child.ParentSession = null;
            child.Dispose();
        }
        OnDispose();
        this.Clear();
        _ruleHandler = null;
        _ruleSet = null;
        _ruleEngine = null;
        _service = null;
        _parentSession = null;
        _activeSession = null;
        _isDisposed = true;
    }

    public virtual void OnDispose() { }
    #endregion
    #region Private Methods
    public List<ChangeInfo> GetChangesByRow(
        string requestingGrid,
        DataRow row,
        Operation operation,
        bool hasErrors,
        bool hasChanges,
        bool fromTemplate
    )
    {
        return GetChangesByRow(
            requestingGrid: requestingGrid,
            row: row,
            operation: operation,
            ignoreKeys: null,
            includeRowStates: true,
            hasErrors: hasErrors,
            hasChanges: hasChanges,
            fromTemplate: fromTemplate
        );
    }

    internal List<ChangeInfo> GetChangesByRow(
        string requestingGrid,
        DataRow row,
        Operation operation,
        Hashtable ignoreKeys,
        bool includeRowStates,
        bool hasErrors,
        bool hasChanges,
        bool fromTemplate
    )
    {
        var listOfChanges = new List<ChangeInfo>();
        DataRow rootRow = DatasetTools.RootRow(childRow: row);
        DatasetTools.CheckRowErrorRecursive(
            row: rootRow,
            skipRow: null,
            includeChildErrorsInParent: false
        );
        // when there is an error, copy it to the list entity, too
        if (this.DataList != null)
        {
            DataRow listRow = GetListRow(
                entity: this.DataListEntity,
                id: DatasetTools.PrimaryKey(row: rootRow)[0]
            );
            CloneErrors(sourceRow: rootRow, destinationRow: listRow);
        }
        if (
            _entityHasRuleDependencies[key: row.Table.TableName]
            || (operation == Operation.CurrentRecordNeedsUpdate)
            || hasErrors
            || fromTemplate
        )
        {
            // entity has some dependencies (e.g. calculated columns in other tables)
            // so we return also the parents and children of this row
            GetChangesRecursive(
                changes: listOfChanges,
                requestingGrid: requestingGrid,
                row: rootRow,
                operation: operation,
                changedRow: row,
                allDetails: true,
                ignoreKeys: ignoreKeys,
                includeRowStates: includeRowStates
            );
        }
        else
        {
            // this entity has no dependencies in other tables, we only
            // return data from this row
            ChangeInfo ci = GetChangeInfo(
                requestingGrid: requestingGrid,
                row: row,
                operation: operation,
                rowStateProcessor: includeRowStates
                    ? new Func<string, object[], List<RowSecurityState>>(RowStates)
                    : null
            );
            listOfChanges.Add(item: ci);
        }
        if (this.SuppressSave && hasChanges)
        {
            // set "saved" flag also if the form is read only (it might actually change data
            // e.g. for virtual fields, these are not read only even in read only screens)
            // but we still don't want a dirty flag because the user would be asked to save
            // data when closing the screen
            this.Data.AcceptChanges();
        }
        else
        {
            // If the updates did not cause any changes, e.g. because only non-dirty-enabled
            // entity data were changed, we send an info to reset the dirty flag.
            // Non-dirty enabled entities are those that get not saved, e.g. entities in a workflow
            // session store that are not in the save-data structure for the workflow form.
            foreach (DataTable table in this.Data.Tables)
            {
                if (!this.DirtyEnabledEntities.Contains(item: table.TableName))
                {
                    table.AcceptChanges();
                }
            }
        }
        if (!hasChanges)
        {
            listOfChanges.Add(item: ChangeInfo.SavedChangeInfo());
        }
        return listOfChanges;
    }

    private static void CloneErrors(DataRow sourceRow, DataRow destinationRow)
    {
        destinationRow.ClearErrors();
        if (sourceRow.HasErrors)
        {
            destinationRow.RowError = destinationRow.RowError;
            foreach (DataColumn col in sourceRow.GetColumnsInError())
            {
                destinationRow.SetColumnError(
                    columnName: col.ColumnName,
                    error: sourceRow.GetColumnError(column: col)
                );
            }
        }
    }

    public List<ChangeInfo> GetChanges(
        string entity,
        object id,
        Operation operation,
        bool hasErrors,
        bool hasChanges
    )
    {
        return GetChangesByRow(
            requestingGrid: null,
            row: this.GetSessionRow(entity: entity, id: id),
            operation: operation,
            hasErrors: hasErrors,
            hasChanges: hasChanges,
            fromTemplate: false
        );
    }

    public List<ChangeInfo> GetChanges(
        string entity,
        object id,
        Operation operation,
        Hashtable ignoreKeys,
        bool includeRowStates,
        bool hasErrors,
        bool hasChanges
    )
    {
        return GetChangesByRow(
            requestingGrid: null,
            row: this.GetSessionRow(entity: entity, id: id),
            operation: operation,
            ignoreKeys: ignoreKeys,
            includeRowStates: includeRowStates,
            hasErrors: hasErrors,
            hasChanges: hasChanges,
            fromTemplate: false
        );
    }

    private void GetChangesRecursive(
        List<ChangeInfo> changes,
        string requestingGrid,
        DataRow row,
        Operation operation,
        DataRow changedRow,
        bool allDetails,
        Hashtable ignoreKeys,
        bool includeRowStates
    )
    {
        if (row.RowState != DataRowState.Deleted && row.RowState != DataRowState.Detached)
        {
            // Optimization. There are cases when calling UpdateObject can result in a lot of changes and a lot of
            // row states. This should reduce size of the returned data and improve the UpdateObject's time.
            // The missing row states should be loaded by the client.
            includeRowStates = includeRowStates && changes.Count < 20;

            object rowKey = DatasetTools.PrimaryKey(row: row)[0];
            string ignoreRowIndex = row.Table.TableName + rowKey.ToString();
            if (row.Equals(obj: changedRow))
            {
                ChangeInfo ci = GetChangeInfo(
                    requestingGrid: requestingGrid,
                    row: row,
                    operation: operation,
                    rowStateProcessor: includeRowStates
                        ? new Func<string, object[], List<RowSecurityState>>(RowStates)
                        : null
                );
                changes.Add(item: ci);
            }
            else if (ignoreKeys == null || !ignoreKeys.Contains(key: ignoreRowIndex))
            {
                // check if this is a child of the copied row
                bool isParentRow = !IsChildRow(row: row, changedRow: changedRow);
                // always parent rows because calculated fields do not change the RowState
                if (
                    allDetails
                    || isParentRow
                    || row.RowState != DataRowState.Unchanged
                    || row.HasErrors
                )
                {
                    Operation op = operation;
                    // this is a parent row of the copied row, we set the status Update
                    if ((op == Operation.CurrentRecordNeedsUpdate) && isParentRow)
                    {
                        op = Operation.Update;
                    }
                    // no copy (in that case we leave copy status), then this
                    // is update, because it is not the actual changed row
                    else if (isParentRow)
                    {
                        op = Operation.Update;
                    }
                    ChangeInfo ci = GetChangeInfo(
                        requestingGrid: null,
                        row: row,
                        operation: op,
                        rowStateProcessor: includeRowStates
                            ? new Func<string, object[], List<RowSecurityState>>(RowStates)
                            : null
                    );
                    changes.Add(item: ci);
                    // we processed it once so we do not want to get it again in a next iteration
                    if (ignoreKeys != null)
                    {
                        ignoreKeys.Add(key: ignoreRowIndex, value: null);
                    }
                }
            }
            Boolean tableAggregation = HasAggregation(row: row);
            foreach (DataRelation childRelation in row.Table.ChildRelations)
            {
                foreach (DataRow childRow in row.GetChildRows(relation: childRelation))
                {
                    if (RowIsChangedOrHasChangedChild(row: childRow) || tableAggregation)
                    {
                        // check recursion
                        foreach (DataRelation parentRelation in row.Table.ParentRelations)
                        {
                            foreach (
                                DataRow parentRow in row.GetParentRows(relation: parentRelation)
                            )
                            {
                                if (parentRow.Equals(obj: childRow))
                                {
                                    // Recursion found - this row has been checked already.
                                    return;
                                }
                            }
                        }
                        GetChangesRecursive(
                            changes: changes,
                            requestingGrid: requestingGrid,
                            row: childRow,
                            operation: operation,
                            changedRow: changedRow,
                            allDetails: allDetails,
                            ignoreKeys: ignoreKeys,
                            includeRowStates: includeRowStates
                        );
                    }
                }
            }
        }
    }

    private bool RowIsChangedOrHasChangedChild(DataRow row)
    {
        if (row.RowState != DataRowState.Unchanged)
        {
            return true;
        }
        foreach (DataRelation childRelation in row.Table.ChildRelations)
        {
            foreach (DataRow childRow in row.GetChildRows(relation: childRelation))
            {
                if (RowIsChangedOrHasChangedChild(row: childRow))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool HasAggregation(DataRow row)
    {
        if (row.Table.ExtendedProperties.ContainsKey(key: Const.HasAggregation))
        {
            return (Boolean)row.Table.ExtendedProperties[key: Const.HasAggregation];
        }
        return false;
    }

    private static bool IsChildRow(DataRow row, DataRow changedRow)
    {
        bool found = false;
        DataRow parentRow = row;
        while (parentRow != null && parentRow.Table.ParentRelations.Count > 0)
        {
            parentRow = parentRow.GetParentRow(relation: parentRow.Table.ParentRelations[index: 0]);
            if (changedRow.Equals(obj: parentRow))
            {
                // One of the parent rows is the copied row,
                // so this one is copied as well, so we leave the copy status.
                found = true;
                break;
            }
        }
        return found;
    }

    internal ChangeInfo GetChangeInfo(string requestingGrid, DataRow row, Operation operation)
    {
        return GetChangeInfo(
            requestingGrid: requestingGrid,
            row: row,
            operation: operation,
            rowStateProcessor: RowStates
        );
    }

    public static ChangeInfo GetChangeInfo(
        string requestingGrid,
        DataRow row,
        Operation operation,
        Func<string, object[], List<RowSecurityState>> rowStateProcessor
    )
    {
        ChangeInfo ci = new ChangeInfo();
        ci.Entity = row.Table.TableName;
        ci.Operation = (
            operation == Operation.CurrentRecordNeedsUpdate ? Operation.Create : operation
        ); // 4 = copy = create
        ci.RequestingGrid = requestingGrid;
        ci.ObjectId = row[column: row.Table.PrimaryKey[0]];
        // for create-update we return the updated state (read-only + colors)
        if (operation >= Operation.Update)
        {
            string[] columns = GetColumnNames(table: row.Table);
            ci.WrappedObject = GetRowData(row: row, columns: columns);
            if (rowStateProcessor != null)
            {
                ci.State = rowStateProcessor.Invoke(arg1: ci.Entity, arg2: new[] { ci.ObjectId })[
                    index: 0
                ];
            }
        }
        return ci;
    }

    internal static string ConvertTextToUnixStyle(string text)
    {
        return text.Replace(oldValue: "\r\n", newValue: "\n");
    }

    public static List<object> GetRowData(DataRow row, string[] columns)
    {
        return GetRowData(row: row, columns: columns, withErrors: true);
    }

    private static List<object> GetRowData(DataRow row, string[] columns, bool withErrors)
    {
        var result = new List<object>(capacity: columns.Length);
        foreach (string col in columns)
        {
            if (col != LIST_LOADED_COLUMN_NAME)
            {
                object value = null;
                DataColumn dataColumn = row.Table.Columns[name: col];
                if (IsWriteOnly(dataColumn: dataColumn))
                {
                    value = null;
                }
                else
                {
                    if (IsColumnArray(dataColumn: dataColumn))
                    {
                        value = GetRowColumnArrayValue(row: row, dataColumn: dataColumn);
                    }
                    else
                    {
                        value = GetRowColumnValue(row: row, col: dataColumn);
                    }
                }
                result.Add(item: value);
            }
        }
        if (withErrors)
        {
            result.Add(item: GetRowErrors(row: row));
        }
        return result;
    }

    public static bool IsColumnArray(DataColumn dataColumn)
    {
        if (dataColumn.ExtendedProperties.Contains(key: Const.OrigamDataType))
        {
            return ((OrigamDataType)dataColumn.ExtendedProperties[key: Const.OrigamDataType])
                == OrigamDataType.Array;
        }

        return false;
    }

    public static bool IsWriteOnly(DataColumn dataColumn)
    {
        if (dataColumn.ExtendedProperties.Contains(key: Const.IsWriteOnlyAttribute))
        {
            return ((bool)dataColumn.ExtendedProperties[key: Const.IsWriteOnlyAttribute]) == true;
        }

        return false;
    }

    private static object GetRowErrors(DataRow row)
    {
        object value = null;
        if (row.HasErrors)
        {
            ErrorList errorList = new ErrorList();
            errorList.RowError = row.RowError;
            foreach (DataColumn col in row.GetColumnsInError())
            {
                errorList.FieldErrors.Add(key: col.Ordinal, value: row.GetColumnError(column: col));
            }
            value = errorList;
        }
        return value;
    }

    public static List<object> GetRowColumnArrayValue(DataRow row, DataColumn dataColumn)
    {
        string relatedTableName = (string)dataColumn.ExtendedProperties[key: Const.ArrayRelation];
        string relatedColumnName = (string)
            dataColumn.ExtendedProperties[key: Const.ArrayRelationField];
        DataRow[] childRows = row.GetChildRows(relationName: relatedTableName);
        var list = new List<object>(capacity: childRows.Length);
        foreach (DataRow childRow in childRows)
        {
            list.Add(item: childRow[columnName: relatedColumnName]);
        }
        return list;
    }

    private static object GetRowColumnValue(DataRow row, DataColumn col)
    {
        object value = null;
        object o = row[column: col];
        string text = o as string;
        if (text != null)
        {
            if (text != "")
            {
                value = ConvertTextToUnixStyle(text: text);
            }
        }
        else
        {
            value = o;
        }
        return value;
    }

    public DataTable GetDataTable(string entity)
    {
        return GetDataTable(entity: entity, data: Data ?? DataList);
    }

    public DataTable GetDataTable(string entity, DataSet data)
    {
        if (!data.Tables.Contains(name: entity))
        {
            throw new OrigamDataException(message: "Entity not found: " + entity);
        }
        return data.Tables[name: entity];
    }

    public Guid GetEntityId(string entity)
    {
        var dataStructureEntityId = (Guid)
            GetDataTable(entity: entity).ExtendedProperties[key: "Id"]!;
        var dataStructureEntity = Workbench
            .Services.ServiceManager.Services.GetService<Workbench.Services.IPersistenceService>()
            .SchemaProvider.RetrieveInstance<DataStructureEntity>(
                instanceId: dataStructureEntityId
            );
        return dataStructureEntity.EntityDefinition.Id;
    }

    internal static object ShortGuid(Guid guid)
    {
        return _ascii85.Encode(ba: guid.ToByteArray());
    }

    public DataRow GetSessionRow(string entity, object id)
    {
        if (id == null)
        {
            throw new NullReferenceException(message: "Cannot find row. Id is null");
        }
        DataRow row = null;
        // first we try to find the row in the data
        if (this.Data != null && this.Data.Tables.Contains(name: entity))
        {
            row = this.Data.Tables[name: entity].Rows.Find(key: id);
        }
        // not found, we try to find it in the list
        if (this.DataList != null && row == null)
        {
            row = GetListRow(entity: entity, id: id);
        }
        return row;
    }

    public DataRow GetListRow(string entity, object id)
    {
        DataTable table = GetDataTable(entity: entity, data: this.DataList);
        return table.Rows.Find(key: id);
    }

    public void LazyLoadListRowData(object rowId, DataRow row)
    {
        lock (_lock)
        {
            if (row.Table.Columns.Contains(name: SessionStore.LIST_LOADED_COLUMN_NAME))
            {
                if (!(bool)row[columnName: SessionStore.LIST_LOADED_COLUMN_NAME])
                {
                    // the row has not been loaded from the database yet, we load the
                    // whole row even though only some of the columns are needed because
                    // e.g. color or row-level security rules must be evaluated on all the
                    // columns
                    QueryParameterCollection pms = new QueryParameterCollection();
                    foreach (DataColumn col in row.Table.PrimaryKey)
                    {
                        pms.Add(
                            value: new QueryParameter(_parameterName: col.ColumnName, value: rowId)
                        );
                    }
                    DataSet loadedRow = DatasetTools.CloneDataSet(dataset: row.Table.DataSet);
                    CoreServices.DataService.Instance.LoadRow(
                        dataStructureEntityId: DataListDataStructureEntityId,
                        filterSetId: DataListFilterSetId,
                        parameters: pms,
                        currentData: loadedRow,
                        transactionId: null
                    );
                    if (loadedRow.Tables[name: row.Table.TableName].Rows.Count == 0)
                    {
                        throw new ArgumentOutOfRangeException(
                            paramName: string.Format(
                                format: "Row {0} not found in {1}.",
                                arg0: rowId,
                                arg1: row.Table.TableName
                            )
                        );
                    }
                    SessionStore.MergeRow(
                        r: loadedRow.Tables[name: row.Table.TableName].Rows[index: 0],
                        listRow: row
                    );
                    row[columnName: SessionStore.LIST_LOADED_COLUMN_NAME] = true;
                    row.AcceptChanges();
                }
            }
        }
    }

    public List<RowSecurityState> RowStates(string entity, object[] ids)
    {
        var result = new List<RowSecurityState>();
        object profileId = SecurityTools.CurrentUserProfile().Id;
        if (dataRequested)
        {
            foreach (object id in ids)
            {
                if (dataRequested)
                {
                    if (id != null)
                    {
                        DataRow row;
                        try
                        {
                            row = GetSessionRow(entity: entity, id: id);
                        }
                        catch
                        {
                            // in case the id is not contained in the datasource anymore (e.g. form unloaded or new data piece loaded)
                            return new List<RowSecurityState>();
                        }
                        var formIdBeforeLock = FormId;
                        lock (_lock) // no update should be done in the meantime when rules are not handled
                        {
                            // The user may have pressed the Next button on a workflow screen causing a different
                            // screen to load while this thread was waiting for the lock.
                            if (formIdBeforeLock != FormId)
                            {
                                return new List<RowSecurityState>();
                            }
                            if (IsLazyLoadedRow(row: row))
                            {
                                // load lazily loaded rows in case they have not been loaded
                                // before calling for row-states
                                LazyLoadListRowData(rowId: id, row: row);
                            }
                            if (row == null)
                            {
                                result.Add(item: new RowSecurityState { Id = id, NotFound = true });
                            }
                            else
                            {
                                result.Add(
                                    item: RowSecurityStateBuilder.BuildFull(
                                        ruleEngine: RuleEngine,
                                        row: row,
                                        profileId: profileId,
                                        formId: FormId
                                    )
                                );
                            }
                        }
                    }
                }
            }
            return result;
        }
        // data not requested (data less session)
        lock (_lock) // no update should be done in the meantime when rules are not handled
        {
            return RowStatesForDataLessSessions(entity: entity, ids: ids, profileId: profileId);
        }
    }

    private List<RowSecurityState> RowStatesForDataLessSessions(
        string entity,
        object[] ids,
        object profileId
    )
    {
        var result = new List<RowSecurityState>();
        RowSearchResult rowSearchResult = GetRowsFromStore(entity: entity, ids: ids);
        foreach (var row in rowSearchResult.Rows)
        {
            result.Add(
                item: RowSecurityStateBuilder.BuildFull(
                    ruleEngine: RuleEngine,
                    row: row,
                    profileId: profileId,
                    formId: FormId
                )
            );
        }
        // try to get the rest from the database
        if (rowSearchResult.IdsNotFoundInStore.Count > 0)
        {
            DataRowCollection loadedRows = LoadMissingRows(
                entity: entity,
                idsNotFoundInStore: rowSearchResult.IdsNotFoundInStore
            );
            foreach (DataRow row in loadedRows)
            {
                RowSecurityState rowSecurity =
                    RowSecurityStateBuilder.BuildJustMainEntityRowLevelEvenWithoutFields(
                        ruleEngine: this.RuleEngine,
                        row: row
                    );
                if (rowSecurity != null)
                {
                    result.Add(item: rowSecurity);
                    rowSearchResult.IdsNotFoundInStore.Remove(
                        key: row[columnName: "Id"].ToString()
                    );
                }
            }
            // mark records not found as not found and put them into output as well
            rowSearchResult.IdsNotFoundInStore.Values.ForEach(action: id =>
                result.Add(item: new RowSecurityState { Id = id, NotFound = true })
            );
        }
        return result;
    }

    class RowSearchResult
    {
        public List<DataRow> Rows { get; set; }
        public Dictionary<string, Object> IdsNotFoundInStore { get; set; }
    }

    private RowSearchResult GetRowsFromStore(string entity, IEnumerable ids)
    {
        List<DataRow> result = new List<DataRow>();
        Dictionary<string, Object> notFoundIds = new Dictionary<string, Object>();
        // try to get from session first anyway (e.g. for the newly created records)
        foreach (object id in ids)
        {
            if (id != null)
            {
                try
                {
                    DataRow row = GetSessionRow(entity: entity, id: id);
                    if (row != null)
                    {
                        result.Add(item: row);
                    }
                    else
                    {
                        notFoundIds.Add(key: id.ToString(), value: id);
                    }
                }
                catch
                {
                    // not found in the session, save it for later
                    notFoundIds.Add(key: id.ToString(), value: id);
                }
            }
        }
        return new RowSearchResult { Rows = result, IdsNotFoundInStore = notFoundIds };
    }

    public List<DataRow> GetRows(string entity, IEnumerable ids)
    {
        RowSearchResult rowSearchResult = GetRowsFromStore(entity: entity, ids: ids);
        // try to get the rest from the database
        if (rowSearchResult.IdsNotFoundInStore.Count > 0)
        {
            var loadedRows = LoadMissingRows(
                entity: entity,
                idsNotFoundInStore: rowSearchResult.IdsNotFoundInStore
            );
            rowSearchResult.Rows.AddRange(collection: loadedRows.CastToList<DataRow>());
        }
        return rowSearchResult.Rows;
    }

    private DataRowCollection LoadMissingRows(
        string entity,
        Dictionary<string, object> idsNotFoundInStore
    )
    {
        var dataService = CoreServices.DataServiceFactory.GetDataService();
        var dataStructureEntityId = (Guid)Data.Tables[name: entity].ExtendedProperties[key: "Id"];
        var dataStructureEntity =
            Workbench
                .Services.ServiceManager.Services.GetService<Workbench.Services.IPersistenceService>()
                .SchemaProvider.RetrieveInstance(
                    type: typeof(DataStructureEntity),
                    primaryKey: new Key(id: dataStructureEntityId)
                ) as DataStructureEntity;
        return LoadRows(
            dataService: dataService,
            entity: dataStructureEntity,
            dataStructureEntityId: dataStructureEntityId,
            methodId: DataListFilterSetId,
            rowIds: idsNotFoundInStore.Values.ToArray()
        );
    }

    public bool IsLazyLoadedRow(DataRow row)
    {
        return DataList != null && row.Table.DataSet == DataList;
    }

    public bool IsLazyLoadedEntity(string entity)
    {
        return DataListEntity != null && entity == DataListEntity;
    }

    #region CRUD
    public virtual List<ChangeInfo> CreateObject(
        string entity,
        IDictionary<string, object> values,
        IDictionary<string, object> parameters,
        string requestingGrid
    )
    {
        lock (_lock)
        {
            DataTable table = GetDataTable(entity: entity, data: this.Data);
            UserProfile profile = SecurityTools.CurrentUserProfile();
            DataRow newRow;
            try
            {
                RegisterEvents();
                if (parameters.Count == 0)
                {
                    newRow = DatasetTools.CreateRow(
                        parentRow: null,
                        newRowTable: table,
                        relation: null,
                        profileId: profile.Id
                    );
                }
                else
                {
                    object[] keys = new object[parameters.Count];
                    parameters.Values.CopyTo(array: keys, arrayIndex: 0);
                    DataRelation relation = table.ParentRelations[index: 0];
                    DataRow parentRow = GetParentRow(
                        parameters: parameters,
                        keys: keys,
                        relation: relation
                    );
                    newRow = DatasetTools.CreateRow(
                        parentRow: parentRow,
                        newRowTable: table,
                        relation: relation,
                        profileId: profile.Id
                    );
                }
                // set any values passed by the client (e.g. when adding an entry into a calendar,
                // resource and date are known so they are handed over directly when adding a record
                if (values != null)
                {
                    foreach (KeyValuePair<string, object> entry in values)
                    {
                        newRow[columnName: entry.Key] = entry.Value;
                    }
                }
                table.Rows.Add(row: newRow);
                if (
                    !RowSecurityStateBuilder
                        .BuildJustMainEntityRowLevelEvenWithoutFields(
                            ruleEngine: RuleEngine,
                            row: newRow
                        )
                        .AllowCreate
                )
                {
                    table.Rows.Remove(row: newRow);
                    throw new Exception(message: Resources.ErrorCreateRecordNotAllowed);
                }
            }
            finally
            {
                UnregisterEvents();
            }
            NewRowToDataList(newRow: newRow);
            List<ChangeInfo> listOfChanges = GetChangesByRow(
                requestingGrid: requestingGrid,
                row: newRow,
                operation: Operation.Create,
                hasErrors: this.Data.HasErrors,
                hasChanges: this.Data.HasChanges(),
                fromTemplate: false
            );
            return listOfChanges;
        }
    }

    private static DataRow GetParentRow(
        IDictionary<string, object> parameters,
        object[] keys,
        DataRelation relation
    )
    {
        DataColumn parentKeyColumn = relation.ParentColumns[0];
        DataRow parentRow = null;
        if (
            parameters.Count == 1
            && parentKeyColumn.Equals(obj: relation.ParentTable.PrimaryKey[0])
        )
        {
            // if parent column is the primary key, then we just simply lookup the row by its primary key
            parentRow = relation.ParentTable.Rows.Find(keys: keys);
        }
        else
        {
            // if not, we have to construct a search
            StringBuilder searchBuilder = new StringBuilder();
            foreach (KeyValuePair<string, object> entry in parameters)
            {
                for (int i = 0; i < relation.ChildColumns.Length; i++)
                {
                    if (relation.ChildColumns[i].ColumnName == entry.Key)
                    {
                        parentKeyColumn = relation.ParentColumns[i];
                    }
                }
                if (parentKeyColumn == null)
                {
                    throw new ArgumentOutOfRangeException(
                        paramName: "key",
                        actualValue: entry.Key,
                        message: "Key not found in the parent table by the provided child key."
                    );
                }
                string value = entry.Value.ToString();
                if (
                    parentKeyColumn.DataType == typeof(Guid)
                    || parentKeyColumn.DataType == typeof(string)
                )
                {
                    value = DatasetTools.TextExpression(text: value);
                }
                else if (parentKeyColumn.DataType == typeof(DateTime))
                {
                    value = DatasetTools.DateExpression(dateValue: entry.Value);
                }
                if (searchBuilder.Length > 0)
                {
                    searchBuilder.Append(value: " AND ");
                }
                searchBuilder.Append(value: parentKeyColumn.ColumnName);
                searchBuilder.Append(value: " = ");
                searchBuilder.Append(value: value);
            }
            DataRow[] rows = relation.ParentTable.Select(
                filterExpression: searchBuilder.ToString()
            );
            if (rows.Length == 1)
            {
                parentRow = rows[0];
            }
        }
        return parentRow;
    }

    public virtual IEnumerable<ChangeInfo> UpdateObject(
        string entity,
        object id,
        string property,
        object newValue
    )
    {
        lock (_lock)
        {
            DataRow row = UpdateObjectInternal(
                entity: entity,
                id: id,
                property: property,
                newValue: newValue
            );
            return GetChanges(row: row);
        }
    }

    public void UpdateObjectsWithoutGetChanges(
        string entity,
        object id,
        string property,
        object newValue
    )
    {
        lock (_lock)
        {
            DataRow row = UpdateObjectInternal(
                entity: entity,
                id: id,
                property: property,
                newValue: newValue
            );
        }
    }

    private IEnumerable<ChangeInfo> GetChanges(DataRow row)
    {
        if (Data == null)
        {
            throw new Exception(
                message: "GetChanges cannot run because the session store property Data is null"
            );
        }
        List<ChangeInfo> listOfChanges = GetChangesByRow(
            requestingGrid: null,
            row: row,
            operation: Operation.Update,
            hasErrors: this.Data.HasErrors,
            hasChanges: this.Data.HasChanges(),
            fromTemplate: false
        );
        if (!this.Data.HasChanges())
        {
            listOfChanges.Add(item: ChangeInfo.SavedChangeInfo());
        }
        return listOfChanges;
    }

    private DataRow UpdateObjectInternal(string entity, object id, string property, object newValue)
    {
        DataRow row = GetSessionRow(entity: entity, id: id);
        try
        {
            RegisterEvents();
            UserProfile profile = SecurityTools.CurrentUserProfile();
            if (row == null)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "id",
                    actualValue: id,
                    message: Resources.ErrorRecordNotFound
                );
            }
            DataColumn dataColumn = row.Table.Columns[name: property];
            if (dataColumn == null)
            {
                throw new NullReferenceException(
                    message: String.Format(format: Resources.ErrorColumnNotFound, arg0: property)
                );
            }
            if (IsColumnArray(dataColumn: dataColumn))
            {
                UpdateRowColumnArray(
                    newValue: newValue,
                    profile: profile,
                    row: row,
                    dataColumn: dataColumn
                );
            }
            else
            {
                UpdateRowColumn(property: property, newValue: newValue, profile: profile, row: row);
            }
        }
        finally
        {
            UnregisterEvents();
        }
        return row;
    }

    private static void UpdateRowColumnArray(
        object newValue,
        UserProfile profile,
        DataRow row,
        DataColumn dataColumn
    )
    {
        string relatedTableName = (string)dataColumn.ExtendedProperties[key: Const.ArrayRelation];
        string relatedColumnName = (string)
            dataColumn.ExtendedProperties[key: Const.ArrayRelationField];
        DataTable relatedTable = row.Table.DataSet.Tables[name: relatedTableName];
        DataRow[] childRows = row.GetChildRows(relationName: relatedTableName);
        Array newArray =
            newValue is JArray ? ((JArray)newValue).ToObject<object[]>() : (Array)newValue;
        // handle null value (sent e.g. when updating dependent fields)
        // null = empty array
        if (newArray == null)
        {
            newArray = new object[] { };
        }
        var rowsToDelete = new List<DataRow>();
        var valuesToAdd = new List<object>();
        // values to add
        foreach (object arrayValue in newArray)
        {
            bool found = false;
            foreach (DataRow childRow in childRows)
            {
                if (childRow[columnName: relatedColumnName] == arrayValue)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                valuesToAdd.Add(item: arrayValue);
            }
        }
        // values to delete
        foreach (DataRow childRow in childRows)
        {
            bool found = false;
            {
                foreach (object arrayValue in newArray)
                {
                    if (childRow[columnName: relatedColumnName] == arrayValue)
                    {
                        found = true;
                        break;
                    }
                }
            }
            if (!found)
            {
                rowsToDelete.Add(item: childRow);
            }
        }
        foreach (DataRow rowToDelete in rowsToDelete)
        {
            rowToDelete.Delete();
        }
        foreach (object valueToAdd in valuesToAdd)
        {
            DataRow newRow = DatasetTools.CreateRow(
                parentRow: row,
                newRowTable: relatedTable,
                relation: row.Table.ChildRelations[name: relatedTableName],
                profileId: profile.Id
            );
            UpdateRowValue(property: relatedColumnName, newValue: valueToAdd, row: newRow);
            relatedTable.Rows.Add(row: newRow);
        }
    }

    private void UpdateRowColumn(string property, object newValue, UserProfile profile, DataRow row)
    {
        UpdateRowValue(property: property, newValue: newValue, row: row);
        DatasetTools.UpdateOrigamSystemColumns(
            row: row,
            isNew: row.RowState == DataRowState.Added,
            profileId: profile.Id
        );
        // update the data list
        if (this.DataList != null)
        {
            DataRow dataRow = row;
            while (dataRow != null && dataRow.Table.TableName != this.DataListEntity)
            {
                if (dataRow.Table.ParentRelations.Count > 0)
                {
                    dataRow = dataRow.GetParentRow(
                        relation: dataRow.Table.ParentRelations[index: 0]
                    );
                }
                else
                {
                    dataRow = null;
                }
            }
            object[] pk = DatasetTools.PrimaryKey(row: dataRow);
            DataRow listRow = this.DataList.Tables[name: this.DataListEntity].Rows.Find(keys: pk);
            MergeRow(r: dataRow, listRow: listRow);
        }
    }

    private static void UpdateRowValue(string property, object newValue, DataRow row)
    {
        if (
            newValue == null
            || (
                row.Table.Columns[name: property].DataType == typeof(string)
                & string.Empty.Equals(obj: newValue)
            )
            || (
                row.Table.Columns[name: property].DataType == typeof(Guid)
                & string.Empty.Equals(obj: newValue)
            )
        )
        {
            row[columnName: property] = DBNull.Value;
        }
        else if (
            (row.Table.Columns[name: property].DataType == typeof(decimal))
            && (newValue is string stringValue)
        )
        {
            row[columnName: property] = decimal.Parse(
                s: stringValue,
                provider: CultureInfo.InvariantCulture
            );
        }
        else
        {
            row[columnName: property] = newValue;
        }
    }

    public virtual List<ChangeInfo> DeleteObject(string entity, object id)
    {
        lock (_lock)
        {
            DataRow row = GetSessionRow(entity: entity, id: id);
            DataRow rootRow = DatasetTools.RootRow(childRow: row);
            DataSet dataset = row.Table.DataSet;
            // get the changes for the deleted row before we actually deleted, because then the row would be inaccessible
            var deletedItems = new List<ChangeInfo>();
            Dictionary<string, List<DeletedRowInfo>> backup = BackupDeletedRows(row: row);
            object[] listRowBackup = null;
            deletedItems.Add(
                item: GetChangeInfo(requestingGrid: null, row: row, operation: Operation.Delete)
            );
            AddChildDeletedItems(deletedItems: deletedItems, deletedRow: row);
            // get the parent rows for the rule handler in order to update them
            var parentRows = new List<DataRow>();
            foreach (DataRelation relation in row.Table.ParentRelations)
            {
                parentRows.AddRange(
                    collection: row.GetParentRows(
                        relation: relation,
                        version: DataRowVersion.Default
                    )
                );
            }
            bool isRowAggregated = DatasetTools.IsRowAggregated(row: row);
            try
            {
                // .NET BUGFIX: Dataset does not refresh aggregated calculated columns on delete, we have to raise change event
                if (isRowAggregated)
                {
                    try
                    {
                        RegisterEvents();
                        row.BeginEdit();
                        foreach (DataColumn col in row.Table.Columns)
                        {
                            if (
                                col.ReadOnly == false
                                & (
                                    col.DataType == typeof(int)
                                    | col.DataType == typeof(float)
                                    | col.DataType == typeof(decimal)
                                    | col.DataType == typeof(long)
                                )
                            )
                            {
                                object zero = Convert.ChangeType(
                                    value: 0,
                                    conversionType: col.DataType
                                );
                                if (!row[column: col].Equals(obj: zero))
                                {
                                    row[column: col] = 0;
                                }
                            }
                        }
                        row.EndEdit();
                    }
                    finally
                    {
                        UnregisterEvents();
                    }
                }
            }
            catch { }
            try
            {
                // DELETE THE ROW
                try
                {
                    RegisterEvents();
                    row.Delete();
                    // handle rules for the data changes after the row has been deleted
                    this.RuleHandler.OnRowDeleted(
                        parentRows: parentRows.ToArray(),
                        deletedRow: row,
                        data: this.XmlData,
                        ruleSet: this.RuleSet,
                        ruleEngine: this.RuleEngine
                    );
                }
                finally
                {
                    UnregisterEvents();
                }
                var listOfChanges = new List<ChangeInfo>();
                // get the changes - from root - e.g. recalculated totals after deletion
                if (
                    isRowAggregated
                    || (
                        (rootRow.Table.TableName != entity)
                        && _entityHasRuleDependencies[key: rootRow.Table.TableName]
                    )
                )
                {
                    listOfChanges.AddRange(
                        collection: GetChangesByRow(
                            requestingGrid: null,
                            row: rootRow,
                            operation: Operation.Update,
                            hasErrors: this.Data.HasErrors,
                            hasChanges: this.Data.HasChanges(),
                            fromTemplate: false
                        )
                    );
                }
                // include the deletions
                listOfChanges.AddRange(collection: deletedItems);
                if (IsLazyLoadedEntity(entity: entity))
                {
                    // delete the row from the list
                    if (this.DataList != null)
                    {
                        DataTable table = GetDataTable(
                            entity: this.DataListEntity,
                            data: this.DataList
                        );
                        row = table.Rows.Find(key: id);
                        listRowBackup = row.ItemArray;
                        row.Delete();
                        table.AcceptChanges();
                    }
                    // save the data
                    var actionResult = (
                        (IList)ExecuteAction(actionId: ACTION_SAVE)
                    ).CastToList<ChangeInfo>();
                    listOfChanges.AddRange(collection: actionResult);
                }
                return listOfChanges;
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                {
                    log.ErrorFormat(
                        format: "Caught an exception when trying to delete an object {0},{1} from session store: `{2}'",
                        arg0: entity,
                        arg1: id,
                        arg2: ex.ToString()
                    );
                }
                // delete the root row because we have a backup from the root row
                if (rootRow.RowState != DataRowState.Deleted)
                {
                    rootRow.Delete();
                }
                // we reset the changes
                dataset.AcceptChanges();
                // and then we import all the rows from the root row down
                foreach (KeyValuePair<string, List<DeletedRowInfo>> tablePair in backup)
                {
                    foreach (DeletedRowInfo info in tablePair.Value)
                    {
                        info.ImportData(table: dataset.Tables[name: tablePair.Key]);
                    }
                }
                // we also return the list row if it has been deleted
                if (listRowBackup != null)
                {
                    this.DataList.Tables[name: this.DataListEntity]
                        .Rows.Add(values: listRowBackup)
                        .AcceptChanges();
                }
                throw;
            }
        }
    }

    private static Dictionary<string, List<DeletedRowInfo>> BackupDeletedRows(DataRow row)
    {
        Dictionary<string, List<DeletedRowInfo>> backup =
            new Dictionary<string, List<DeletedRowInfo>>();
        if (!backup.ContainsKey(key: row.Table.TableName))
        {
            backup.Add(key: row.Table.TableName, value: new List<DeletedRowInfo>());
        }

        backup[key: row.Table.TableName].Add(item: new DeletedRowInfo(row: row));
        BackupChildRows(parentRow: row, backup: backup);
        return backup;
    }

    private static void BackupChildRows(
        DataRow parentRow,
        Dictionary<string, List<DeletedRowInfo>> backup
    )
    {
        foreach (DataRelation relation in parentRow.Table.ChildRelations)
        {
            foreach (DataRow childRow in parentRow.GetChildRows(relation: relation))
            {
                if (!backup.ContainsKey(key: childRow.Table.TableName))
                {
                    backup.Add(key: childRow.Table.TableName, value: new List<DeletedRowInfo>());
                }

                backup[key: childRow.Table.TableName].Add(item: new DeletedRowInfo(row: childRow));
                BackupChildRows(parentRow: childRow, backup: backup);
            }
        }
    }

    public List<ChangeInfo> CopyObject(
        string entity,
        object originalId,
        string requestingGrid,
        List<string> entities,
        IDictionary<string, object> forcedValues
    )
    {
        lock (_lock)
        {
            if (originalId == null)
            {
                throw new NullReferenceException(
                    message: "Original record not set. Cannot copy record."
                );
            }
            DataTable table = GetDataTable(entity: entity, data: this.Data);
            DataRow row = GetSessionRow(entity: entity, id: originalId);
            UserProfile profile = SecurityTools.CurrentUserProfile();
            var toSkip = new List<string>();
            foreach (DataTable t in this.Data.Tables)
            {
                if (!entities.Contains(item: t.TableName) && !IsArrayChild(table: t))
                {
                    toSkip.Add(item: t.TableName);
                }
            }
            DataSet tmpDS = DatasetTools.CloneDataSet(
                dataset: row.Table.DataSet,
                cloneExpressions: false
            );
            DatasetTools.GetDataSlice(
                target: tmpDS,
                rows: new List<DataRow> { row },
                profileId: profile.Id,
                copy: true,
                tablesToSkip: toSkip
            );
            try
            {
                DataRow newTmpRow = tmpDS.Tables[name: table.TableName].Rows[index: 0];
                if (
                    !RowSecurityStateBuilder
                        .BuildJustMainEntityRowLevelEvenWithoutFields(
                            ruleEngine: RuleEngine,
                            row: newTmpRow
                        )
                        .AllowCreate
                )
                {
                    throw new Exception(message: Resources.ErrorCreateRecordNotAllowed);
                }
                // set any values passed by the client (e.g. when adding an entry into a calendar,
                // resource and date are known so they are handed over directly when adding a record
                if (forcedValues != null)
                {
                    foreach (KeyValuePair<string, object> entry in forcedValues)
                    {
                        newTmpRow[columnName: entry.Key] = entry.Value;
                    }
                }
                this.Data.EnforceConstraints = false;
                if (IsLazyLoadedEntity(entity: entity))
                {
                    // we are copying on the root of delayed loaded form
                    // so we clear the dataset completely and merge back only the copy
                    DatasetTools.Clear(data: this.Data);
                }
                MergeParams mergeParams = new MergeParams(ProfileId: profile.Id);
                DatasetTools.MergeDataSet(
                    inout_dsTarget: this.Data,
                    in_dsSource: tmpDS,
                    changeList: null,
                    mergeParams: mergeParams
                );
                object[] pk = DatasetTools.PrimaryKey(row: newTmpRow);
                if (IsLazyLoadedEntity(entity: entity))
                {
                    this.CurrentRecordId = pk[0];
                }
                DataRow newRow = table.Rows.Find(keys: pk);
                try
                {
                    RegisterEvents();
                    RuleHandler.OnRowCopied(
                        row: newRow,
                        data: this.XmlData,
                        ruleSet: this.RuleSet,
                        ruleEngine: this.RuleEngine
                    );
                }
                finally
                {
                    UnregisterEvents();
                }
                NewRowToDataList(newRow: newRow);
                return GetChangesByRow(
                    requestingGrid: requestingGrid,
                    row: newRow,
                    operation: Operation.CurrentRecordNeedsUpdate,
                    hasErrors: this.Data.HasErrors,
                    hasChanges: this.Data.HasChanges(),
                    fromTemplate: false
                );
            }
            finally
            {
                this.Data.EnforceConstraints = true;
            }
        }
    }

    private bool IsArrayChild(DataTable table)
    {
        if (table.ParentRelations.Count == 1)
        {
            foreach (DataColumn column in table.ParentRelations[index: 0].ParentTable.Columns)
            {
                if (
                    column.ExtendedProperties.Contains(key: Const.ArrayRelation)
                    && (string)column.ExtendedProperties[key: Const.ArrayRelation]
                        == table.ParentRelations[index: 0].RelationName
                )
                {
                    return true;
                }
            }
            return false;
        }

        return false;
    }

    internal void NewRowToDataList(DataRow newRow)
    {
        lock (_lock)
        {
            // merge to the list
            if (IsLazyLoadedEntity(entity: newRow.Table.TableName))
            {
                if (this.DataList != null)
                {
                    DataTable listTable = this.DataList.Tables[name: this.DataListEntity];
                    DataRow newListRow = listTable.NewRow();
                    MergeRow(r: newRow, listRow: newListRow);
                    listTable.Rows.Add(row: newListRow);
                    newListRow.AcceptChanges();
                    OnNewRecord(
                        entity: newRow.Table.TableName,
                        id: DatasetTools.PrimaryKey(row: newRow)[0]
                    );
                }
            }
        }
    }

    internal void UpdateListRow(DataRow r)
    {
        lock (_lock)
        {
            if (this.DataList != null)
            {
                // find the list row
                DataRow listRow = this
                    .DataList.Tables[name: r.Table.TableName]
                    .Rows.Find(keys: DatasetTools.PrimaryKey(row: r));
                if (listRow != null)
                {
                    MergeRow(r: r, listRow: listRow);
                    // accept changes, the list row is read-only anyway, but...
                    listRow.AcceptChanges();
                }
            }
        }
    }

    internal virtual void OnNewRecord(string entity, object id) { }

    internal static void MergeRow(DataRow r, DataRow listRow)
    {
        listRow.BeginEdit();
        // merge in the changed data
        foreach (DataColumn col in listRow.Table.Columns)
        {
            if (
                (col.Expression == null || col.Expression == "")
                && col.ColumnName != LIST_LOADED_COLUMN_NAME
            )
            {
                bool wasReadOnly = col.ReadOnly;
                col.ReadOnly = false;
                listRow[column: col] = r[columnName: col.ColumnName];
                if (wasReadOnly)
                {
                    col.ReadOnly = true;
                }
            }
        }
        listRow.EndEdit();
    }

    public static string[] GetColumnNames(DataTable table)
    {
        string[] columns = new string[table.Columns.Count];
        for (int i = 0; i < table.Columns.Count; i++)
        {
            columns[i] = table.Columns[index: i].ColumnName;
        }
        return columns;
    }

    /// <summary>
    /// Gets data by parent record ID (only for delayed data loading).
    /// </summary>
    /// <param name="sessionFormIdentifier"></param>
    /// <param name="entity"></param>
    /// <param name="parentId"></param>
    /// <returns></returns>
    public virtual List<List<object>> GetData(
        string childEntity,
        object parentRecordId,
        object rootRecordId
    )
    {
        throw new Exception(message: "GetData not available for " + this.GetType().Name);
    }

    public virtual List<ChangeInfo> GetRowData(string entity, object id, bool ignoreDirtyState)
    {
        throw new Exception(message: "GetRowData not available for " + this.GetType().Name);
    }

    public virtual ChangeInfo GetRow(string entity, object id)
    {
        throw new Exception(message: "GetRow not available for " + GetType().Name);
    }
    #endregion
    private void AddChildDeletedItems(List<ChangeInfo> deletedItems, DataRow deletedRow)
    {
        foreach (DataRelation child in deletedRow.Table.ChildRelations)
        {
            foreach (DataRow childRow in deletedRow.GetChildRows(relation: child))
            {
                deletedItems.Add(
                    item: GetChangeInfo(
                        requestingGrid: null,
                        row: childRow,
                        operation: Operation.Delete
                    )
                );
                AddChildDeletedItems(deletedItems: deletedItems, deletedRow: childRow);
            }
        }
    }
    #endregion
    public List<ChangeInfo> UpdateObjectBatch(string entity, string property, Hashtable values)
    {
        var result = new List<ChangeInfo>();
        lock (_lock)
        {
            foreach (DictionaryEntry entry in values)
            {
                result.AddRange(
                    collection: UpdateObject(
                        entity: entity,
                        id: entry.Key,
                        property: property,
                        newValue: entry.Value
                    )
                );
            }
        }
        return result;
    }

    public List<ChangeInfo> UpdateObjectEx(string entity, object id, Hashtable values)
    {
        var result = new List<ChangeInfo>();
        lock (_lock)
        {
            foreach (DictionaryEntry entry in values)
            {
                result.AddRange(
                    collection: UpdateObject(
                        entity: entity,
                        id: id,
                        property: (string)entry.Key,
                        newValue: entry.Value
                    )
                );
            }
        }
        return result;
    }

    public List<ChangeInfo> UpdateObjectBatch(string entity, UpdateData[] updateDataArray)
    {
        var result = new List<ChangeInfo>();
        lock (_lock)
        {
            foreach (UpdateData updateData in updateDataArray)
            {
                foreach (KeyValuePair<string, object> entry in updateData.Values)
                {
                    result.AddRange(
                        collection: UpdateObject(
                            entity: entity,
                            id: updateData.RowId,
                            property: (string)entry.Key,
                            newValue: entry.Value
                        )
                    );
                }
            }
        }
        return result;
    }

    public virtual void RevertChanges()
    {
        throw new Exception(message: "RevertChanges not available for " + GetType().Name);
    }

    // The default implementation of GetData. The function itself is used only in FormSessionStore
    // and WorkQueueSessionStore. The introduction of a complex hierarchy isn't worth it, because
    // the session stores don't have too much in common. So we opted for protected implementation.
    protected List<List<object>> GetDataImplementation(
        string childEntity,
        object parentRecordId,
        object rootRecordId
    )
    {
        // check validity of the request
        if (!rootRecordId.Equals(obj: CurrentRecordId))
        {
            // we do not hold the data anymore, we throw-out the request
            return new List<List<object>>();
        }
        DataTable childTable = GetDataTable(entity: childEntity);
        var result = new List<List<object>>();
        if (childTable.ParentRelations.Count == 0)
        {
            throw new Exception(
                message: "Requested entity "
                    + childEntity
                    + " has no parent relations. Cannot load child records."
            );
        }
        DataRelation parentRelation = childTable.ParentRelations[index: 0];
        // get parent row again (the one before was most probably loaded from the list
        // now we have it in the cache
        DataRow parentRow = GetSessionRow(
            entity: parentRelation.ParentTable.TableName,
            id: parentRecordId
        );
        if (parentRow == null)
        {
            throw new ArgumentOutOfRangeException(
                paramName: $"Parent record id "
                    + $"{parentRecordId} not found in "
                    + $"{parentRelation.ParentTable.TableName} - "
                    + $"parent of {childEntity}."
            );
        }
        // get the requested entity data
        string[] columns = GetColumnNames(table: childTable);
        foreach (
            DataRow dataRow in parentRow.GetChildRows(relationName: parentRelation.RelationName)
        )
        {
            result.Add(item: GetRowData(row: dataRow, columns: columns));
        }
        return result;
    }
}
